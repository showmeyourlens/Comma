using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CableCloud
{
    public class TargetNetworkObject
    {
        public int InputPort { get; private set; }

        public int LinkId { get; private set; }
        public int TargetPort { get; private set; }
        public string TargetObjectId { get; private set; }
        public string TargetObjectAddress { get; set; }
        public Socket TargetSocket { get; set; }
        public bool IsLinkDown {get; set;}

        public TargetNetworkObject(int linkId, int inputPort, int outputPort, string objectId)
        {
            this.InputPort = inputPort;
            this.TargetPort = outputPort;
            this.TargetObjectId = objectId;
            this.IsLinkDown = false;
            this.LinkId = linkId;
        }

        public TargetNetworkObject(string objectId)
        {
            this.TargetObjectId = objectId;
        }
    }
}
