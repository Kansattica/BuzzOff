﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BuzzOff.Server.Entities
{
    public class RoomUser
    {
        public Room Room { get; set; }
        public User User { get; set; }
    }
}