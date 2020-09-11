using System;
using System.Collections.Generic;

namespace BuzzOff.Server.Entities
{
	public class User : IEquatable<User>
	{
		public string Name { get; set; }
		public string SignalRId { get; set; }
		public bool IsHost { get; set; } = false;
		public bool BuzzedIn { get; set; } = false;
		public bool LockedOut { get; set; } = false;

		public override bool Equals(object obj) => Equals(obj as User);

		public bool Equals(User other) => other != null && SignalRId == other.SignalRId;

		public override int GetHashCode() => HashCode.Combine(SignalRId);

		public static bool operator ==(User left, User right) => EqualityComparer<User>.Default.Equals(left, right);

		public static bool operator !=(User left, User right) => !(left == right);
	}
}
