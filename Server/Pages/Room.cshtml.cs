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

		public void OnGet()
		{
			RoomId = HttpUtility.HtmlEncode(RoomId);
		}
	}
}
