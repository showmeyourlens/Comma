using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using ToolsLibrary;
using TSST_EON;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CableCloud;

namespace NetworkCallController
{
    class NetworkCallController
    {
        public ManualResetEvent AllDone = new ManualResetEvent(false);
        public Socket Server;
        public Socket NCC2Socket;
        Socket SubnetworkSocket;
        public IPAddress NetworkCallControllerAddress;
        public int NetworkCallControllerPort;
        public IPAddress NetworkCallControllerAddress2;
        public int NetworkCallControllerPort2;
        public IPAddress SubnetworkAddress;
        public int SubnetworkPort;
        public string NetworkCallControlerId;
        public List<NetworkElement> NetworkElements = new List<NetworkElement>();
        static void Main(string[] args)
        {
            NetworkCallController ncc = new NetworkCallController(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
        }

        NetworkCallController(string NetworkCallControlerId, string NetworkCallControllerAddress, string NetworkCallControllerPort, string NetworkCallControllerAddress2, string NetworkCallControllerPort2, string SubnetworkAddress, string SubnetworkPort)
        {
            this.NetworkCallControlerId = NetworkCallControlerId;
            this.NetworkCallControllerAddress = IPAddress.Parse(NetworkCallControllerAddress);
            this.NetworkCallControllerPort = int.Parse(NetworkCallControllerPort);
            this.NetworkCallControllerAddress2 = IPAddress.Parse(NetworkCallControllerAddress2);
            this.NetworkCallControllerPort2 = int.Parse(NetworkCallControllerPort2);
            this.SubnetworkAddress = IPAddress.Parse(SubnetworkAddress);
            this.SubnetworkPort = int.Parse(SubnetworkPort);
            if(NetworkCallControlerId=="NetworkCallController1")
            {
                ParseHostsInDomain("nodesindomain1.txt");
            }
            else if(NetworkCallControlerId=="NetworkCallController2")
            {
                ParseHostsInDomain("nodesindomain2.txt");
            }
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
                TimeStamp.WriteLine("Network Call Controler is starting");
                while (true)
                {
                    AllDone.Reset();
                    Server.BeginAccept(new AsyncCallback(AcceptCallback), Server);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine(e.Message);
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

                if(received.fromCPCC==true && received.message.Contains("CallRequest_req"))
                {
                    TimeStamp.WriteLine("Recieved Call Request request from" + received.sendingClientId + "to" + received.receivingClientId + "with capacity" + received.capacity);
                    NetworkElement networkelement;
                    if (NetworkElements.Exists(x => x.NetworkElementName == received.receivingClientId))
                    {
                        networkelement = NetworkElements.Find(x => x.NetworkElementName == received.receivingClientId);
                        if (networkelement.NetworkElementSocket == null)
                        {
                            networkelement.NetworkElementSocket = handler;
                        }
                        TimeStamp.WriteLine("Trying to connect to subnetwork");
                        string sendingIP = NetworkElements.Find(x => x.NetworkElementName == received.sendingClientId).NetworkElementAddress.ToString();
                        string receivingIP = NetworkElements.Find(x => x.NetworkElementName == received.receivingClientId).NetworkElementAddress.ToString();
                        NetworkPackage toSubnetwork = new NetworkPackage(sendingIP, receivingIP, received.capacity, true);
                        SubnetworkSocket = new Socket(SubnetworkAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        SubnetworkSocket.Connect(new IPEndPoint(SubnetworkAddress, SubnetworkPort));
                        Send(SubnetworkSocket, toSubnetwork);
                    }
                    else
                    {
                        TimeStamp.WriteLine("Trying to find host in other domain");
                        NCC2Socket = new Socket(NetworkCallControllerAddress2.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        NCC2Socket.Connect(new IPEndPoint(NetworkCallControllerAddress2, NetworkCallControllerPort2));
                    }
                }
                else if(received.message.Contains("ConnectionRequest_rsp"))
                {
                    TimeStamp.WriteLine("Recieved Connection Request response with parameters:/nIP of sending host" + received.currentIP + "/nIP of reciving host" + received.receivingClientIP + "/nUsed slots" + received.slots);
                    NetworkPackage toncc2 = new NetworkPackage(received.currentIP, received.receivingClientIP, received.slots, true);
                    Send(NCC2Socket, toncc2);
                }
                else if(received.message.Contains("CallCoordination_req"))
                {
                    TimeStamp.WriteLine("Recieved Call Coordination request with parameters:/nIP of sending host" + received.currentIP + "/nIP of reciving host" + received.receivingClientIP + "/nUsed slots" + received.slots);
                    NetworkPackage toSubnetwork2 = new NetworkPackage(received.currentIP, received.receivingClientIP, received.slots, true);
                    SubnetworkSocket = new Socket(SubnetworkAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    SubnetworkSocket.Connect(new IPEndPoint(SubnetworkAddress, SubnetworkPort));
                    Send(SubnetworkSocket, toSubnetwork2);
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

        public void ParseHostsInDomain (string FileName)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(FileName);
            while((line = file.ReadLine()) != null)  
            {  
                string[] data;
                data = line.Split(' ');
                this.NetworkElements.Add(new NetworkElement(data[0], data[1]));
            }  
  
            file.Close();  
        }

    }
}
