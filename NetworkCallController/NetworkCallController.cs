using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using ToolsLibrary;
using CableCloud;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkCallController
{
    class NetworkCallController
    {
        public ManualResetEvent AllDone = new ManualResetEvent(false);
        public Socket Server;
        public IPAddress NetworkCallControllerAddress;
        public int NetworkCallControllerPort;
        public string NetworkCallControlerId;
        public List<NetworkElements> NetworkElements = new List<NetworkElements>();
        static void Main(string[] args)
        {
            NetworkCallController ncc = new NetworkCallController(args[0], args[1], args[2]);
        }

        NetworkCallController(string NetworkCallControlerId, string NetworkCallControllerAddress, string NetworkCallControllerPort)
        {
            this.NetworkCallControlerId = NetworkCallControlerId;
            this.NetworkCallControllerAddress = IPAddress.Parse(NetworkCallControllerAddress);
            this.NetworkCallControllerPort = int.Parse(NetworkCallControllerPort);
            try
            {
                Task.Run(() => StartNetworkCallController());
            }
            catch (Exception e)
            {

            }
        }

        private void StartNetworkCallController()
        {
            Server = new Socket(NetworkCallControllerAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Server.Bind(new IPEndPoint(NetworkCallControllerAddress, NetworkCallControllerPort));
                Server.Listen(100);
                TimeStamp.WriteLine("Network Call Controler startuje");
                while (true)
                {
                    AllDone.Reset();
                    Server.BeginAccept(new AsyncCallback(AcceptCallback), Server);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {

            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            AllDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            ReceiverState state = new ReceiverState();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, ReceiverState.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Odbiór pakietu
            try
            {
                ReceiverState receiverState = (ReceiverState)ar.AsyncState;
                Socket handler = receiverState.WorkSocket;
                int bytesRead = handler.EndReceive(ar);


                NetworkPackage received = Deserialize(receiverState, bytesRead);

                if (NetworkCallControlerId == "NetworkCallController1")
                {
                    Socket to_ncc = 
                    //if received.CPCCmessage (jeszcze nie ma takiej wersji pakietu, po stworzeniu trzeba odkomentować)
                    //Send(received.
                }

                handler.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReadCallback), receiverState);
            }
            catch (Exception e)
            {

            }
        }

        private NetworkPackage Deserialize(ReceiverState receiverState, int byterRead)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(receiverState.Buffer, 0, byterRead);
                memStream.Seek(0, SeekOrigin.Begin);
                NetworkPackage obj = (NetworkPackage)binForm.Deserialize(memStream);
                return obj;
            }
        }

        public byte[] SerializeMessage(NetworkPackage networkPackage)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, networkPackage);
                return ms.ToArray();
            }
        }
        private void Send(Socket socket, NetworkPackage received)
        {
            ReceiverState state = new ReceiverState();
            state.WorkSocket = socket;
            state.Buffer = SerializeMessage(received);
            socket.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(SendCallback), state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState client = (ReceiverState)ar.AsyncState;
                client.WorkSocket.EndSend(ar);
                AllDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
