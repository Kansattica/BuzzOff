using BuzzOff.Server.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

            await Clients.Group(room.SignalRId).SendAsync("SetButton", false);

            User buzzedIn = null;

            // this lock should ensure that the first one in (from the server's perspective) wins.
            lock (room.Users)
            { 
                if (room.Users.Any(x => x.BuzzedIn)) { return; } // if someone's already buzzed in, no prize

                var foundUser = room.Users.FirstOrDefault(x => x.SignalRId == Context.ConnectionId);

                if (foundUser != null)
				{
					foundUser.BuzzedIn = true;
                    buzzedIn = foundUser;
				}
			}

			if (buzzedIn != null)
            {
                await Task.WhenAll(
                    Clients.Group(room.SignalRId).SendAsync("SendMessage", $"{buzzedIn.Name} buzzed in!"),
                    Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users));
            }
        }

        public async Task UpdateName(string newName)
		{
            var room = _rooms.GetRoomFromUser(Context.ConnectionId);

            lock (room.Users)
			{
                var toChange = room.Users.FirstOrDefault(x => x.SignalRId == Context.ConnectionId);
                if (toChange != null)
				{
                    toChange.Name = newName;
				}
			}

            await Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users);

		}

		public async Task Reset()
        {
            var room = _rooms.GetRoomFromUser(Context.ConnectionId);

            // only the room owner can clear
            if (room.RoomHost.SignalRId != Context.ConnectionId)
            {
                return;
            }

            lock (room.Users)
			{
				room.Users.ForEach(x => x.BuzzedIn = false);
			}

			await Task.WhenAll(
                Clients.Group(room.SignalRId).SendAsync("SetButton", true),
                Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users),
                Clients.Group(room.SignalRId).SendAsync("SendMessage", ""));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = _rooms.LeaveRoom(Context.ConnectionId);

            // shouldn't happen, but it's possible and cheap to guard against
            if (room != null)
				await Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users);

			await base.OnDisconnectedAsync(exception);
        }

    }
}
