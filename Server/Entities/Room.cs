using System;
using System.Collections.Generic;

namespace BuzzOff.Server.Entities
{
    public class Room : IEquatable<Room>
    {
        public string Name { get; set; }
        public string SignalRId { get; set; }
        public User RoomHost { get; set; }
        public List<User> Users { get; set; }

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
