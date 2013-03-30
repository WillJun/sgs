﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using System.Diagnostics;
using System.IO;
using Sanguosha.Core.UI;
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Network
{
    public class Client
    {
        NetworkGamer networkService;

        public NetworkGamer NetworkService
        {
            get { return networkService; }
            set { networkService = value; }
        }
        
        public int SelfId { get; set; }

        private string ipString;

        public string IpString
        {
            get { return ipString; }
            set { ipString = value; }
        }
        private int portNumber;

        public int PortNumber
        {
            get { return portNumber; }
            set { portNumber = value; }
        }

        public Client()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReplay">Set true if this is client is connected to a replayFile</param>
        /// <param name="replayStream"></param>
        /// <exception cref="System.ArgumentOutOfRangeException" />
        /// <exception cref="System.Net.Sockets.SocketException" />
        public void Start(Stream recordStream = null, LoginToken? token = null)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
            TcpClient client = new TcpClient();
            client.Connect(ep);
            NetworkStream stream = client.GetStream();
            networkService = new NetworkGamer();
            networkService.DataStream = stream;
            networkService.StartListening();
            if (token != null)
            {
                networkService.Send(new ConnectionRequest() { token = (LoginToken)token });
            }
        }

        public ReplayController ReplayController { get; set; }

        public void StartReplay(Stream replayStream)
        {
            this.replayStream = replayStream;
            networkService = new NetworkGamer();
            networkService.DataStream = replayStream;
            ReplayController = new Utils.ReplayController();
            ReplayController.EvenDelays = true;
        }

        private Stream replayStream;

        public Stream ReplayStream
        {
            get { return replayStream; }
        }

        public Stream RecordStream {get;set;}
        /*{
            get
            {
                if (receiver != null) return receiver.RecordStream;
                else return null;
            }
            set
            {
                Trace.Assert(receiver != null);
                receiver.RecordStream = value;
            }
        }*/

        public void MoveHandCard(int from, int to)
        {
            networkService.Send(new HandCardMovementNotification() { From = from, To = to, PlayerItem = PlayerItem.Parse(SelfId) });
        }

        public void CardChoiceCallBack(CardRearrangement arrange)
        {
            networkService.Send(new CardRearrangementNotification() { CardRearrangement = arrange });
        }

        public void Stop()
        {
/*            if (receiver.RecordStream != null)
            {
                receiver.RecordStream.Flush();
                receiver.RecordStream.Close();
            }*/
        }
    }
}
