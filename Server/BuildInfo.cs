using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuzzOff.Server
{
	public static class BuildInfo
	{
		public const string GitHash = "@@GIT_HASH@@";
		public static string ShortGitHash = GitHash.Substring(0, 8);
		public static string BuildTimestamp = GetTimestamp().ToString("R");
		
		private static DateTimeOffset GetTimestamp()
		{
			if (long.TryParse("@@BUILD_TIME@@", out var timestamp))
				return DateTimeOffset.FromUnixTimeSeconds(timestamp);
			return DateTimeOffset.MinValue;
		}
	}
}
