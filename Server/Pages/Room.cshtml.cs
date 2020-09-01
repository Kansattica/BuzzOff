using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuzzOff.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuzzOff.Server.Pages
{
	public class RoomModel : PageModel
	{
		[BindProperty(SupportsGet = true)]
		public string RoomId { get; set; }

		public string UserName { get; set; }

		public void OnGet()
		{
			if (Request.Query.TryGetValue("user", out var value))
			{
				UserName = value.FirstOrDefault();
			}

			if (string.IsNullOrWhiteSpace(UserName))
			{
				UserName = RandomName.RandomString(1);
			}
		}
	}
}
