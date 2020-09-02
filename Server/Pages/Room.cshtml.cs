using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BuzzOff.Server.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuzzOff.Server.Pages
{
	public class RoomModel : PageModel
	{
		[BindProperty(SupportsGet = true)]
		public string RoomId { get; set; }
		public string UserName { get; set; }

		private static readonly string[] _javascriptPaths = new string[] { "~/js/dist/browser/signalr.min.js", "~/js/buzz.js" };
		public string[] JavascriptPaths => _javascriptPaths;

		public void OnGet()
		{
			if (Request.Query.TryGetValue("user", out var value))
			{
				UserName = value.FirstOrDefault();
			}

			if (string.IsNullOrWhiteSpace(UserName))
			{
				UserName = RandomHelpers.RandomUserName();
			}

			UserName = HttpUtility.HtmlEncode(UserName);
			RoomId = HttpUtility.HtmlEncode(RoomId);
		}
	}
}
