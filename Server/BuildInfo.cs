using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuzzOff.Server
{
	public static class BuildInfo
	{
		public const string GitHash = "@@GIT_HASH@@";
		public static string ShortGitHash = GitHash[0] == '@' ? "(local build)" : GitHash.Substring(0, 12);
		public static string BuildTimestamp = GetTimestamp().ToString("dddd, MMMM dd, yyyy \"at\" hh:mm:ss tt \"GMT\"zzz");
		
		private static DateTimeOffset GetTimestamp()
		{
			if (long.TryParse("@@BUILD_TIME@@", out var timestamp))
				return TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(timestamp), TimeZoneInfo.GetSystemTimeZones().First(x => x.Id == "Pacific Standard Time" || x.Id == "America/Los_Angeles"));
			return DateTimeOffset.MinValue;
		}
	}
}
