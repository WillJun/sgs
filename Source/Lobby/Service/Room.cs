﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace Sanguosha.Lobby.Core
{
    [DataContract(Name = "RoomType")]
    public enum RoomType
    {
        [EnumMember]
        Role,
    }

    [DataContract(Name = "RoomType")]
    public enum RoomState
    {
        [EnumMember]
        Waiting,
        [EnumMember]
        Gaming
    }

    public class Room
    {
        public object RoomLock;

        public Room()
        {
            seats = new List<Seat>();
            TimeOutSeconds = 15;
        }

        bool dualHeroMode;

        public bool DualHeroMode
        {
            get { return dualHeroMode; }
            set { dualHeroMode = value; }
        }

        int optionalHeros;

        public int OptionalHeros
        {
            get { return optionalHeros; }
            set { optionalHeros = value; }
        }

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private RoomType type;

        public RoomType Type
        {
            get { return type; }
            set { type = value; }
        }
        private int timeOutSeconds;

        public int TimeOutSeconds
        {
            get { return timeOutSeconds; }
            set { timeOutSeconds = value; }
        }
        private RoomState state;

        public RoomState State
        {
            get { return state; }
            set { state = value; }
        }
        private bool spectatorDisabled;

        public bool SpectatorDisabled
        {
            get { return spectatorDisabled; }
            set { spectatorDisabled = value; }
        }
        private bool chatDisabled;

        public bool ChatDisabled
        {
            get { return chatDisabled; }
            set { chatDisabled = value; }
        }

        private int ownerId;

        public int OwnerId
        {
            get { return ownerId; }
            set { ownerId = value; }
        }

        List<Seat> seats;

        public List<Seat> Seats
        {
            get { return seats; }
            set { seats = value; }
        }

        private string ipAddress;

        public string IpAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }


        private int ipPort;

        public int IpPort
        {
            get { return ipPort; }
            set { ipPort = value; }
        }


    }
}
