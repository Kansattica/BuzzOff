using System;
using System.Collections.Generic;
using System.Text;

namespace BuzzOff.Server.Entities
{
	public class RoomUser : IEquatable<RoomUser>
	{
		public Room Room { get; set; }
		public User User { get; set; }

		public override bool Equals(object obj) => Equals(obj as RoomUser);
		public bool Equals(RoomUser other) => other != null && EqualityComparer<Room>.Default.Equals(Room, other.Room) && EqualityComparer<User>.Default.Equals(User, other.User);
		public override int GetHashCode() => HashCode.Combine(Room, User);

		public static bool operator ==(RoomUser left, RoomUser right) => EqualityComparer<RoomUser>.Default.Equals(left, right);
		public static bool operator !=(RoomUser left, RoomUser right) => !(left == right);
	}
}
