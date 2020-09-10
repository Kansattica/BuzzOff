using BuzzOff.Server.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuzzOff.Server.Hubs
{
	public class BuzzHub : Hub
	{
		private readonly RoomManager _rooms;

		public BuzzHub(RoomManager rooms) => _rooms = rooms;

		public Task JoinRoom(string roomId, string userName)
		{
			var entered = _rooms.EnterRoom(userName, Context.ConnectionId, roomId);

			return Task.WhenAll(Groups.AddToGroupAsync(Context.ConnectionId, roomId),
			  Clients.Group(roomId).SendAsync("UpdateUserList", entered.Room.Users),
			  Clients.Group(roomId).SendAsync("PrelockStatus", entered.Room.IsPrelocked));
		}

		public Task BuzzIn()
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			if (roomUser.Room.IsPrelocked)
			{
				if (roomUser.User.LockedOut)
				{
					// if they're already locked out, do nothing
					return Task.CompletedTask;
				}
				else
				{
					roomUser.User.LockedOut = true;
					return Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateUserList", roomUser.Room.Users);
				}
			}

			// locked out users can't buzz in
			if (roomUser.User.LockedOut) { return Task.CompletedTask; }

			// this lock ensures that the first one in (from the server's perspective) wins.
			// this is an exclusive lock to make sure only one person can have buzzed in.
			roomUser.Room.Lock.EnterWriteLock();
			try
			{
				// if someone's already buzzed in, no prize
				if (roomUser.Room.Users.Any(x => x.BuzzedIn)) { return Task.CompletedTask; } 
				roomUser.User.BuzzedIn = true;
			}
			finally
			{
				roomUser.Room.Lock.ExitWriteLock();
			}


			return Task.WhenAll(Clients.Group(roomUser.Room.SignalRId).SendAsync("SetButton", false),
				 Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateUserList", roomUser.Room.Users));
		}

		public Task UpdateName(string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
				newName = RandomHelpers.RandomUserName();

			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			roomUser.Room.Lock.EnterReadLock();
			try
			{
				// if someone tries some funny business where they change their name to someone else's
				if (roomUser.Room.Users.Any(x => x.SignalRId != Context.ConnectionId && x.Name.Trim() == newName.Trim()))
					newName = "Counterfeit " + newName;
			}
			finally
			{
				roomUser.Room.Lock.ExitReadLock();
			}

			roomUser.User.Name = newName;

			return Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateUserList", roomUser.Room.Users);
		}

		public Task Reset()
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			// only the room owner can clear
			if (roomUser.Room.RoomHost.SignalRId != Context.ConnectionId)
			{
				return Task.CompletedTask;
			}

			roomUser.Room.Lock.EnterWriteLock();
			try
			{
				roomUser.Room.Users.ForEach(x => { x.BuzzedIn = false; x.LockedOut = false; });
				roomUser.Room.IsPrelocked = false;
			}
			finally
			{
				roomUser.Room.Lock.ExitWriteLock();
			}

			return Task.WhenAll(
				Clients.Group(roomUser.Room.SignalRId).SendAsync("SetButton", true),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateUserList", roomUser.Room.Users),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("SendMessage", ""),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("PrelockStatus", roomUser.Room.IsPrelocked));
		}

		public Task SetPrelock(bool isLocked)
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			// only the room owner can prelock and unlock
			if (roomUser.Room.RoomHost.SignalRId != Context.ConnectionId)
			{
				return Task.CompletedTask;
			}

			roomUser.Room.IsPrelocked = isLocked;

			// disable the button when prelocked, maybe?
			// if the room host wants to disable the button while they're talking, they can just buzz in and reset when ready.
			return Task.WhenAll(
				Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateUserList", roomUser.Room.Users),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("SendMessage", isLocked ? "🔒 Please wait to buzz in. 🔒" : "🔓 Go! 🔓"),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("PrelockStatus", isLocked));
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var roomUser = _rooms.LeaveRoom(Context.ConnectionId);

			// shouldn't happen, but it's possible with a misbehaving client and it's cheap to guard against
			if (roomUser != null)
				await Clients.Group(roomUser.SignalRId).SendAsync("UpdateUserList", roomUser.Users);

			await base.OnDisconnectedAsync(exception);
		}
	}
}
