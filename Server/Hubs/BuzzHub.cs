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
			  Clients.Group(roomId).SendAsync("UpdateRoom", entered.Room));
		}

		public Task BuzzIn()
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			// if the room is prelocked and the user isn't locked out yet, lock them out
			if (roomUser.Room.IsPrelocked && !roomUser.User.LockedOut)
			{
				roomUser.User.LockedOut = true;
				return Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateRoom", roomUser.Room);
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
				roomUser.Room.BuzzButtonEnabled = false;
			}
			finally
			{
				roomUser.Room.Lock.ExitWriteLock();
			}

			return Task.WhenAll(Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateRoom", roomUser.Room),
				 // only make buzz noises for the host and the person who buzzed in.
				 // if you want this to buzz for everyone, replace the Clients.Clients() part with Clients.Group(roomUser.Room.SignalRId).
				 Clients.Clients(roomUser.Room.RoomHost.SignalRId, Context.ConnectionId).SendAsync("Buzz", true));
		}

        private const int MaximumNameLength = 40;
		public Task UpdateName(string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
				newName = RandomHelpers.RandomUserName();
			else
				newName = newName.Trim();

			if (newName.Length > MaximumNameLength) { newName = newName.Remove(MaximumNameLength - 1); }

			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			roomUser.Room.Lock.EnterReadLock();
			try
			{
				// if someone tries some funny business where they change their name to someone else's
				if (roomUser.Room.Users.Any(x => x.SignalRId != Context.ConnectionId && x.Name == newName))
					newName = "Counterfeit " + newName;
			}
			finally
			{
				roomUser.Room.Lock.ExitReadLock();
			}

			roomUser.User.Name = newName;

			return Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateRoom", roomUser.Room);
		}

		public Task Reset()
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			// only the room owner can clear
			if (!roomUser.User.IsHost)
			{
				return Task.CompletedTask;
			}

			roomUser.Room.Lock.EnterWriteLock();
			try
			{
				roomUser.Room.Users.ForEach(x => { x.BuzzedIn = false; x.LockedOut = false; });
				roomUser.Room.IsPrelocked = false;
				roomUser.Room.BuzzButtonEnabled = true;
			}
			finally
			{
				roomUser.Room.Lock.ExitWriteLock();
			}

			return Task.WhenAll(
				Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateRoom", roomUser.Room),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("SendMessage", ""),
				Clients.Group(roomUser.Room.SignalRId).SendAsync("Buzz", false));
		}

		public Task SetPrelock(bool isLocked)
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			// only the room owner can prelock and unlock
			if (!roomUser.User.IsHost)
			{
				return Task.CompletedTask;
			}

			roomUser.Room.IsPrelocked = isLocked;

			// this one's a little borderline. I can see the use of having a separate SetPrelocked call, but that leads to weird edge cases like:
			// if a new user joins, do we call SetPrelocked on them, or do we let the isPrelocked on the room handle it?
			return Clients.Group(roomUser.Room.SignalRId).SendAsync("UpdateRoom", roomUser.Room);
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var room = _rooms.LeaveRoom(Context.ConnectionId);

			// the room shouldn't be null, but it's possible with a misbehaving client (say, one that connects with SignalR, but never calls JoinRoom)
			// and it's cheap to guard against anyways.
			if (room != null)
				await Clients.Group(room.SignalRId).SendAsync("UpdateRoom", room);

			await base.OnDisconnectedAsync(exception);
		}
	}
}
