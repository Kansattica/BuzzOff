using BuzzOff.Shared;
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

        public async Task BuzzIn(string roomId)
        {
            // lock on something to make sure that the first one in wins?
            // like lock on the room object? I dunno. I'll think about it.
            await Clients.All.SendAsync("SetButton", false);
            //await Clients.All.SendAsync("BuzzedIn", user);
        }
    }
}
