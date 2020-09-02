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

        public BuzzHub(RoomManager rooms)
        {
            _rooms = rooms;
        }

        public async Task JoinRoom(string roomId, string userName)
        {
            var entered = _rooms.EnterRoom(userName, Context.ConnectionId, roomId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UpdateUserList", entered.Room.Users);
        }

        public async Task BuzzIn()
        {
            var room = _rooms.GetRoomFromUser(Context.ConnectionId);

            await Clients.Group(room.Room.SignalRId).SendAsync("SetButton", false);

            // this lock should ensure that the first one in (from the server's perspective) wins.
            lock (room.Room.Users)
            { 
                if (room.Room.Users.Any(x => x.BuzzedIn)) { return; } // if someone's already buzzed in, no prize
            }

            room.User.BuzzedIn = true;

            await Clients.Group(room.Room.SignalRId).SendAsync("UpdateUserList", room.Room.Users);
        }

        public async Task UpdateName(string newName)
		{
            if (string.IsNullOrWhiteSpace(newName))
                newName = RandomHelpers.RandomName(1);

            var room = _rooms.GetRoomFromUser(Context.ConnectionId);

            // if someone tries some funny business where they change their name to someone else's
            if (room.Room.Users.Any(x => x.SignalRId != Context.ConnectionId && x.Name.Trim() == newName.Trim()))
                newName = "Counterfeit " + newName;

            room.User.Name = newName;

            await Clients.Group(room.Room.SignalRId).SendAsync("UpdateUserList", room.Room.Users);
		}

		public async Task Reset()
        {
            var room = _rooms.GetRoomFromUser(Context.ConnectionId);

            // only the room owner can clear
            if (room.Room.RoomHost.SignalRId != Context.ConnectionId)
            {
                return;
            }

            lock (room.Room.Users)
			{
				room.Room.Users.ForEach(x => x.BuzzedIn = false);
			}

			await Task.WhenAll(
                Clients.Group(room.Room.SignalRId).SendAsync("SetButton", true),
                Clients.Group(room.Room.SignalRId).SendAsync("UpdateUserList", room.Room.Users),
                Clients.Group(room.Room.SignalRId).SendAsync("SendMessage", ""));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = _rooms.LeaveRoom(Context.ConnectionId);

            // shouldn't happen, but it's possible with a misbehaving client and it's cheap to guard against
            if (room != null)
				await Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users);

			await base.OnDisconnectedAsync(exception);
        }
    }
}
