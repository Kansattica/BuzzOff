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

            if (entered.User.IsRoomHost)
            {
                await Clients.Caller.SendAsync("IsRoomOwner", true);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UpdateUserList", entered.Room.Users);
        }

        public async Task BuzzIn()
        {
            var room = _rooms.GetRoomFromUser(Context.ConnectionId);
            // lock on something to make sure that the first one in wins?
            // like lock on the room object? I dunno. I'll think about it.
            await Clients.Group(room.SignalRId).SendAsync("SetButton", false);

            User buzzedIn = null;
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
                await Clients.All.SendAsync("BuzzedIn", buzzedIn);
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
                Clients.All.SendAsync("SetButton", true),
                Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users),
                Clients.Group(room.SignalRId).SendAsync("ClearMessage"));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = _rooms.LeaveRoom(Context.ConnectionId);
            await Clients.Group(room.SignalRId).SendAsync("UpdateUserList", room.Users);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
