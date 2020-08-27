using BuzzOff.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuzzOff.Server
{
    public partial class RoomManager
    {
        private ConcurrentDictionary<string, Room> _activeRooms = new ConcurrentDictionary<string, Room>();

        public RoomUser EnterRoom(string userName, string userId, string roomId)
        {
            var user = new User
            {
                Name = userName,
                SignalRId = userId,
            };

            var updated = _activeRooms.AddOrUpdate(roomId, newRoomId =>
            {
                user.IsRoomHost = true;
                return new Room
                {
                    Name = newRoomId,
                    SignalRId = newRoomId,
                    RoomHost = user,
                    Users = new List<User> { user }
                };
            }, (existingRoomId, existingRoom) =>
            {
                lock(existingRoom.Users)
                {
                    existingRoom.Users.Add(user);
                }
                return existingRoom;
            });

            return new RoomUser { User = user, Room = updated };
        }


    }
}
