using System;
using System.Collections.Generic;

namespace BuzzOff.Shared
{
    public class User : IEquatable<User>
    {
        public string Name { get; set; }

        public string SignalRId { get; set; }
        public bool IsRoomHost { get; set; }
        public bool BuzzedIn { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            return other != null &&
                   SignalRId == other.SignalRId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SignalRId);
        }

        public static bool operator ==(User left, User right)
        {
            return EqualityComparer<User>.Default.Equals(left, right);
        }

        public static bool operator !=(User left, User right)
        {
            return !(left == right);
        }
    }
}
