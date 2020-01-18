using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ToolsLibrary
{
    class Connector
    {
        private Socket _connectingSocket;

        public void TryToConnect()
        {
            _connectingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
            ProtocolType.Tcp);
            while (!_connectingSocket.Connected)
            {
                Thread.Sleep(1000);
                try
                {
                    _connectingSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.2"), 1234));
                }
                catch { }
            }
            SetupForReceiveing();
        }
        private void SetupForReceiveing()
        {
            // View Client Class bottom of Client Example
        }
    }
}
