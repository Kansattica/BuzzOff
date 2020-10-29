using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;

namespace BuzzOff.Server.Entities
{
	public class Room : IEquatable<Room>
	{
		[JsonIgnore]
		public string SignalRId { get; set; }

		[JsonIgnore]
		public User RoomHost { get; set; }

		[JsonIgnore]
		public ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();

		public List<User> Users { get; set; }
		public List<string> BuzzedInIds { get; set; } = new List<string>();
		public bool IsPrelocked { get; set; } = false;
        public bool BuzzButtonEnabled => BuzzedInIds.Count < 3;

        public override bool Equals(object obj) => Equals(obj as Room);

		public bool Equals(Room other) => other != null && SignalRId == other.SignalRId;

		public override int GetHashCode() => HashCode.Combine(SignalRId);

		public static bool operator ==(Room left, Room right) => EqualityComparer<Room>.Default.Equals(left, right);

		public static bool operator !=(Room left, Room right) => !(left == right);
	}
}
