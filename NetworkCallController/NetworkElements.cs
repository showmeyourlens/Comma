using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetworkCallController
{
    class NetworkElements
    {
        public int InputPort { get; private set; }
        public int TargetPort { get; private set; }
        public string TargetObjectId { get; private set; }
        public string TargetObjectAddress { get; set; }
        public Socket TargetSocket { get; set; }

        public NetworkElements(int inputPort, int outputPort, string objectId)
        {
            this.InputPort = inputPort;
            this.TargetPort = outputPort;
            this.TargetObjectId = objectId;
        }
    }
}
