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

        public async Task BuzzIn(string user)
        {
            await Clients.All.SendAsync("BuzzedIn", user);
        }
    }
}
