using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkCallController
{
    class NetworkElement
    {
        public string NetworkElementName { get; set; }

        public IPAddress NetworkElementAddress { get; set; }

        public int NetworkElementPort {get; set; }

        public Socket NetworkElementSocket {get; set; }

        public NetworkElement(string name, string address)
        {
            this.NetworkElementName = name;
            this.NetworkElementAddress = IPAddress.Parse(address);
        }

        public NetworkElement(string name, string address, string port)
        {
            this.NetworkElementName = name;
            this.NetworkElementAddress = IPAddress.Parse(address);
            this.NetworkElementPort = int.Parse(port);
        }
    }
}
