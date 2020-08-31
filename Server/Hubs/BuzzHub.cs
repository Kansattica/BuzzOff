using BuzzOff.Shared;
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
            lock (room)
            {
                if (room.Users.Any(x => x.BuzzedIn)) { return; } // if someone's already buzzed in, no prize

                room.Users.ForEach(x =>
                {
                    if (x.SignalRId == Context.ConnectionId)
                    {
                        x.BuzzedIn = true;
                        buzzedIn = x;
                    }
                });
            }


            if (buzzedIn != null)
                await Clients.Group(room.SignalRId).SendAsync("BuzzedIn", buzzedIn);
        }

        public async Task Reset()
        {
            var room = _rooms.GetRoomFromUser(Context.ConnectionId);

            // only the room owner can clear
            if (room.RoomHost.SignalRId != Context.ConnectionId)
            {
                return;
            }

            room.Users.ForEach(x => x.BuzzedIn = false);

            await Task.WhenAll(
                Clients.Group(room.SignalRId).SendAsync("SetButton", true),
                Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users),
                Clients.Group(room.SignalRId).SendAsync("SendMessage", ""));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = _rooms.LeaveRoom(Context.ConnectionId);
            await Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
