using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuzzOff.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> logger;

        public RoomController(ILogger<RoomController> logger)
        {
            this.logger = logger;
        }

        
        [HttpGet]
        public IEnumerable<int> Get()
        {
            return Enumerable.Range(1, 69);
        }
    }
}
