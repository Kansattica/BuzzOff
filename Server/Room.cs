using System;
using System.Collections.Generic;

namespace BuzzOff.Server
{
    public class Room : IEquatable<Room>
    {
        public string Name;
        public string SignalRId;
        public User RoomHost;
        public List<User> Users;

        public override bool Equals(object obj)
        {
            return Equals(obj as Room);
        }

        public bool Equals(Room other)
        {
            return other != null &&
                   SignalRId == other.SignalRId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SignalRId);
        }

        public static bool operator ==(Room left, Room right)
        {
            return EqualityComparer<Room>.Default.Equals(left, right);
        }

        public static bool operator !=(Room left, Room right)
        {
            return !(left == right);
        }
    }
}
