using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuzzOff.Server
{
	public static class BuildInfo
	{
		public const string GitHash = "@@GIT_HASH@@";
		public const string BuildTimestamp = "@@BUILD_TIME@@";
	}
}
