using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BuzzOff.Server.Entities;
using Microsoft.ApplicationInsights;

namespace BuzzOff.Server
{
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _activeRooms = new ConcurrentDictionary<string, Room>();
        private readonly ConcurrentDictionary<string, RoomUser> _userConnectionToRoom = new ConcurrentDictionary<string, RoomUser>();

        private readonly TelemetryClient _telemetry;

        public RoomManager(TelemetryClient telemetryClient) => _telemetry = telemetryClient;

        public RoomUser EnterRoom(string userName, string userId, string roomId)
        {
            var user = new User {
                Name = userName,
                SignalRId = userId,
            };

            var updated = _activeRooms.AddOrUpdate(roomId, newRoomId =>
            {
                user.IsRoomHost = true;
                return new Room {
                    Name = newRoomId,
                    SignalRId = newRoomId,
                    RoomHost = user,
                    Users = new List<User> { user }
                };
            }, (existingRoomId, existingRoom) =>
            {
                lock (existingRoom.Users)
                {
                    existingRoom.Users.Add(user);
                }
                return existingRoom;
            });

            var roomuser = new RoomUser { User = user, Room = updated };
            _userConnectionToRoom.TryAdd(userId, roomuser);

            _telemetry.TrackEvent("JoinRoom", new Dictionary<string, string> { { "UserId", userId }, { "RoomId", roomId } });
            CollectMetrics();
            return roomuser;
        }

        public RoomUser GetRoomFromUser(string userId) => _userConnectionToRoom.GetValueOrDefault(userId);

        public bool RoomExists(string roomId) => _activeRooms.ContainsKey(roomId);

        public Room LeaveRoom(string userId)
        {
            if (_userConnectionToRoom.TryRemove(userId, out var roomuser))
            {
                if (_activeRooms.ContainsKey(roomuser.Room.SignalRId))
                {
                    lock (roomuser.Room.Users)
                    {
                        if (roomuser.Room.Users.RemoveAll(x => x.SignalRId == userId) > 0)
                        {
                            _userConnectionToRoom.TryRemove(roomuser.User.SignalRId, out var _);

                            if (roomuser.User.IsRoomHost && roomuser.Room.Users.Count > 0)
                            {
                                roomuser.Room.RoomHost = roomuser.Room.Users.First();
                                roomuser.Room.RoomHost.IsRoomHost = true;
                            }
                        }

                        if (roomuser.Room.Users.Count == 0)
                        {
                            // I don't think this leads to a race condition if the last person leaves while someone new comes in
                            // worst case scenario, new person just has to refresh the page
                            _activeRooms.TryRemove(roomuser.Room.SignalRId, out var _);
                        }
                    }
                }

                // only track that they left if it's someone that entered in the first place.
                // this can happen if the signalr connection is established, but the client doesn't call JoinRoom for some reason.
				_telemetry.TrackEvent("LeaveRoom", new Dictionary<string, string> { { "UserId", userId }, { "RoomId", roomuser.Room.SignalRId } });
			}

			CollectMetrics();
            return roomuser?.Room;
        }

        private void CollectMetrics()
        {
            _telemetry.GetMetric("TotalUsers").TrackValue(_userConnectionToRoom.Count);
            _telemetry.GetMetric("TotalRooms").TrackValue(_activeRooms.Count);
        }

    }
}
