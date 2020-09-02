using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuzzOff.Server.Pages
{
    public class IndexModel : PageModel
    {
        private readonly RoomManager _roomManager;

        public string SuggestedName { get; set; }
        public string FlankEmoji { get; set; }

		public IndexModel(RoomManager roomManager) => _roomManager = roomManager;

		public void OnGet()
        {
			FlankEmoji = RandomHelpers.RandomEmoji();

			SuggestedName = RandomHelpers.RandomName(3);

            while (_roomManager.RoomExists(SuggestedName))
                SuggestedName = RandomHelpers.RandomName(3);
        }
    }
}
