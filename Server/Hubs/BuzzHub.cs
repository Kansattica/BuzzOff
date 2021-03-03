using BuzzOff.Server.Entities;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BuzzOff.Server.Hubs
{
	public class BuzzHub : Hub
	{
		private readonly RoomManager _rooms;

		public BuzzHub(RoomManager rooms) => _rooms = rooms;

		public Task JoinRoom(string roomId, string userName)
		{
			var entered = _rooms.EnterRoom(string.IsNullOrWhiteSpace(userName) ? RandomHelpers.RandomUserName() : userName, Context.ConnectionId, roomId);

			return Task.WhenAll(Groups.AddToGroupAsync(Context.ConnectionId, roomId),
			  UpdateRoomIfNeeded(entered.Room));
		}

		private Task UpdateRoomIfNeeded(Room room)
        {
			// basically, if another thread has the writelock (because they're mutating the room), they'll call UpdateRoom after
			// if this is the case, don't bother updating, the next thread will do it.
			// might as well check readlock, because the name-updating case also calls update room.
			return room.Lock.IsWriteLockHeld || room.Lock.IsReadLockHeld ? Task.CompletedTask : Clients.Group(room.SignalRId).SendAsync("UpdateRoom", room);
        }

		public Task BuzzIn()
		{
			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			// locked out users can't buzz in
			if (roomUser.User.LockedOut) { return Task.CompletedTask; }

			// if the room is prelocked and the user isn't locked out yet, lock them out and tell everyone.
			if (roomUser.Room.IsPrelocked)
			{
				roomUser.User.LockedOut = true;
				return UpdateRoomIfNeeded(roomUser.Room);
			}

			// this lock ensures that the first one in (from the server's perspective) wins.
			// this is an exclusive lock to make sure only one person can have buzzed in.
			roomUser.Room.Lock.EnterWriteLock();
			try
			{
				// if you've already buzzed in or we're full, no prize
				if (roomUser.Room.BuzzedInIds.Count >= roomUser.Room.MaxBuzzedIn || roomUser.Room.BuzzedInIds.Any(x => x == roomUser.User.SignalRId)) { return Task.CompletedTask; }
				roomUser.User.BuzzedIn = true;
				roomUser.Room.BuzzedInIds.Add(roomUser.User.SignalRId);
				//roomUser.Room.BuzzButtonEnabled = false;
			}
			finally
			{
				roomUser.Room.Lock.ExitWriteLock();
			}

			return Task.WhenAll(UpdateRoomIfNeeded(roomUser.Room),
				 // only make buzz noises for the host and the person who buzzed in.
				 // if you want this to buzz for everyone, replace the Clients.Clients() part with Clients.Group(roomUser.Room.SignalRId).
				 Clients.Clients(roomUser.Room.RoomHost.SignalRId, Context.ConnectionId).SendAsync("Buzz", true));
		}

		private const int MaximumNameLength = 40;
		private static readonly Regex EmojisToStrip = new Regex(@"🌟|⭐|🐝|🥇|🥈|🥉|🔒|🔓|🔏|🔐|1️⃣|2️⃣|3️⃣", RegexOptions.Compiled);
		private static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);

        private static string StripMeaningfulEmojis(string name) =>
            // Don't let people put any of the emojis that the frontend uses to denote meaning in their names
            EmojisToStrip.Replace(name, "");

		private static string FoldWhitespace(string name) => Whitespace.Replace(name, "\u00A0"); // non-breaking space

        public Task UpdateName(string newName)
		{
			newName = newName == null ? null : FoldWhitespace(StripMeaningfulEmojis(newName)).Trim();
			if (string.IsNullOrWhiteSpace(newName))
				newName = RandomHelpers.RandomUserName();

			if (newName.Length > MaximumNameLength) { newName = newName.Remove(MaximumNameLength - 1); }

			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			roomUser.Room.Lock.EnterReadLock();
			try
			{
				// if someone tries some funny business where they change their name to someone else's
				if (roomUser.Room.Users.Any(x => x.Name == newName && x.SignalRId != Context.ConnectionId))
					newName = "Counterfeit " + newName;
			}
			finally
			{
				roomUser.Room.Lock.ExitReadLock();
			}

			roomUser.User.Name = newName;

			return UpdateRoomIfNeeded(roomUser.Room);
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
				roomUser.Room.Users.ForEach(x => { x.BuzzedIn = x.LockedOut = false; });
				roomUser.Room.BuzzedInIds.Clear();
			}
			finally
			{
				roomUser.Room.Lock.ExitWriteLock();
			}

			return Task.WhenAll(
				UpdateRoomIfNeeded(roomUser.Room),
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
			return UpdateRoomIfNeeded(roomUser.Room);
		}

		public Task UpdateMaxBuzzedIn(int count)
        {
			if (count != 1 && count != 3) { return Task.CompletedTask; }

			var roomUser = _rooms.GetRoomFromUser(Context.ConnectionId);

			if (!roomUser.User.IsHost)
			{
				return Task.CompletedTask;
			}

			roomUser.Room.MaxBuzzedIn = count;

			return UpdateRoomIfNeeded(roomUser.Room);
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var room = _rooms.LeaveRoom(Context.ConnectionId);

			// the room shouldn't be null, but it's possible with a misbehaving client (say, one that connects with SignalR, but never calls JoinRoom)
			// and it's cheap to guard against anyways.
			if (room != null)
				await UpdateRoomIfNeeded(room);

			await base.OnDisconnectedAsync(exception);
		}
	}
}
