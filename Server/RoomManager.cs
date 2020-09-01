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
		private readonly ConcurrentDictionary<string, Room> _userConnectionToRoom = new ConcurrentDictionary<string, Room>();

        // note that this guy is null if there's no app insights!
        private readonly TelemetryClient _telemetry;

        public RoomManager(TelemetryClient telemetryClient)
        {
            _telemetry = telemetryClient;
        }

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

            _userConnectionToRoom.TryAdd(userId, updated);

            _telemetry.TrackEvent("JoinRoom", new Dictionary<string, string> { { "UserId", userId }, { "RoomId", roomId } });
            CollectMetrics();
            return new RoomUser { User = user, Room = updated };
        }

        public Room GetRoomFromUser(string userId)
        {
            return _userConnectionToRoom.GetValueOrDefault(userId);
        }

        public bool RoomExists(string roomId)
        {
            return _activeRooms.ContainsKey(roomId);
        }

        public Room LeaveRoom(string userId)
        {
            if (_userConnectionToRoom.TryRemove(userId, out var room))
            {
                if (_activeRooms.TryGetValue(room.SignalRId, out room))
                {
                    lock(room.Users)
                    {
                        var userIdx = room.Users.FindIndex(x => x.SignalRId == userId);

                        if (userIdx != -1)
                        {
                            var user = room.Users[userIdx];
                            room.Users.RemoveAt(userIdx);
                            _userConnectionToRoom.TryRemove(user.SignalRId, out var _);

                            if (user.IsRoomHost && room.Users.Count > 0)
                            {
                                room.Users.First().IsRoomHost = true;
                                room.RoomHost = room.Users.First();
                            }

                        }

                        if (room.Users.Count == 0)
                        {
                            // I don't think this leads to a race condition if the last person leaves while someone new comes in
                            // worst case scenario, new person just has to refresh the page
                            _activeRooms.TryRemove(room.SignalRId, out var _);
                        }
                    }
                }
            }

            _telemetry.TrackEvent("LeaveRoom", new Dictionary<string, string> { { "UserId", userId }, { "RoomId", room.SignalRId } });
            CollectMetrics();
            return room;
        }

        private void CollectMetrics()
        {
            _telemetry.GetMetric("TotalUsers").TrackValue(_userConnectionToRoom.Count);
            _telemetry.GetMetric("TotalRooms").TrackValue(_activeRooms.Count);
        }

    }
}
