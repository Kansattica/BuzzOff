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

			do
			{
				SuggestedName = RandomHelpers.RandomRoomName();
			} while (_roomManager.RoomExists(SuggestedName));
		}
	}
}
