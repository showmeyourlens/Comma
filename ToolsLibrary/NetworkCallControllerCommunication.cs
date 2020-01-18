using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ToolsLibrary
{
    class NetworkCallControllerCommunication
    {
        public Socket ListenerSocket;
        public int instancePort;

        public NetworkCallControllerCommunication(string instancePort)
        {
            this.ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.instancePort = Int32.Parse(instancePort);
        }
        public void StartListening(IPAddress NCCIPAdress, short Port)
        {
            try
            {
                Console.WriteLine($"Listening started protocol type:{ ProtocolType.Tcp}");
                ListenerSocket.Bind(new IPEndPoint(NCCIPAdress, Port));
                ListenerSocket.Listen(10);
                ListenerSocket.BeginAccept(AcceptCallback, ListenerSocket);
            }
            catch (Exception ex)
            {
                throw new Exception("listening error" + ex);
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine($"Accept CallBack protocol type: {ProtocolType.Tcp}");
                Socket acceptedSocket = ListenerSocket.EndAccept(ar);
                //Instrukcja dla serwera
                ListenerSocket.BeginAccept(AcceptCallback, ListenerSocket);
            }
            catch (Exception ex)
            {
                throw new Exception("Base Accept error" + ex);
            }
        }
    }
}
