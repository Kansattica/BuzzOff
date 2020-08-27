using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuzzOff.Server.Hubs
{
    public class BuzzHub : Hub
    {
        public async Task BuzzIn(string user)
        {
            await Clients.All.SendAsync("BuzzedIn", user);
        }
    }
}
