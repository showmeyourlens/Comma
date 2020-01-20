using CableCloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;
using CableCloud;

namespace CableCloud
{
    class CableCloud
    {
        private ManualResetEvent AllDone = new ManualResetEvent(false);
        private Socket Server;
        private IPAddress CloudAddress;
        private int CloudPort;
        private List<TargetNetworkObject> targetNetworkObjects;
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            CableCloud cloud = new CableCloud();
            char key = 'a';
            do
            {
                key = Console.ReadKey().KeyChar;
            }
            while (key == 'c');
            cloud.targetNetworkObjects.FindAll(x => x.TargetSocket != null).All(x => { x.TargetSocket.Disconnect(false); return true; });
        }

        CableCloud()
        {
            CloudAddress = IPAddress.Parse("127.0.0.1");
            CloudPort = 62572;
            CloudConnectionsXMLReader reader = new CloudConnectionsXMLReader();
            List<Link> networkLinks = reader.ReadCloudConnections();
            GenerateTargetObjectsList(networkLinks);
            //Console.WriteLine(networkLinks[0].LinkId);
            reader.UpdateTargetsWithIPs(targetNetworkObjects);
            try
            {
                Task.Run(() => StartCloud());
            }
            catch (Exception e)
            {

            }
        }

        private void StartCloud()
        {
            Server = new Socket(CloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Server.Bind(new IPEndPoint(CloudAddress, CloudPort));
                Server.Listen(100);
                TimeStamp.WriteLine("Cloud suddenly appeared on a blue sky");
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
            handler.BeginReceive(state.Buffer, 0, ReceiverState.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
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

                ProcessPackageAndResponse(handler, received);

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

        private void ProcessPackageAndResponse(Socket socket, NetworkPackage received)
        {
            if (received.helloMessage)
            {
                try
                {
                    if (received.managementMessage)
                    {
                        targetNetworkObjects.Find(x => x.TargetObjectId == received.sendingClientId).TargetSocket = socket;
                    }
                    else
                    {
                        bool found = false;
                        targetNetworkObjects.FindAll(x => x.TargetObjectId.Equals(received.sendingClientId)).All(x => { x.TargetSocket = socket; found = true; return true; });
                        if (!found) throw new NullReferenceException();
                        TimeStamp.WriteLine("{0} using address {1} connected to cloud.", received.sendingClientId, String.Concat(received.currentIP, ":", received.currentPort));
                    }
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("Received \"Hello\" message from unrecognised object");
                    received.message = String.Format("Sender {0} not recognised by cloud", received.sendingClientId);
                    received.helloMessage = false;
                    received.sendingClientId = "Cloud";
                }

                AllDone.Reset();
                Send(socket, received);
                AllDone.WaitOne();
                return;
            }
            if (received.managementMessage)
            {
                if (received.receivingClientId == "ROUTERS")
                {
                    try
                    {
                        List<TargetNetworkObject> targets = targetNetworkObjects.FindAll(x => x.TargetObjectId[0] == 'N').GroupBy(x => x.TargetObjectId).Select(group => group.First()).ToList();
                        for (int i = 0; i < targets.Count; i++)
                        {
                            if (targets[i].TargetSocket != null)
                            {
                                AllDone.Reset();
                                Send(targets[i].TargetSocket, received);
                                AllDone.WaitOne();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                return;
            }
            if (!received.helloMessage && !received.managementMessage)
            {
                TargetNetworkObject target = targetNetworkObjects.Find(x => x.InputPort == received.currentPort);
                if (target != null) {
                    TimeStamp.WriteLine("Received package from {0}", String.Concat(received.currentIP, ":", received.currentPort));
                    if (target.TargetSocket != null)
                    {
                        received.currentIP = target.TargetObjectAddress;
                        received.currentPort = target.TargetPort;
                        Send(target.TargetSocket, received);
                        Console.WriteLine("{0} Passing packet to {1}", TimeStamp.TAB, String.Concat(target.TargetObjectAddress, ":", target.TargetPort));
                    }
                    else
                    {
                        Console.WriteLine("Socket for target node {0} is null", received.receivingClientId);
                    }
                }
                else
                {
                    Console.WriteLine("Target for port {0} not found. XML problem?", received.currentPort);
                }
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

        private void GenerateTargetObjectsList(List<Link> networkLinks)
        {
            targetNetworkObjects = new List<TargetNetworkObject>();

            // tworzenie "książki adresowej" w chmurze. Adresy węzłów są pobierane z linków, a te z XMLa
            foreach(Link link in networkLinks)
            {
                targetNetworkObjects.Add(new TargetNetworkObject(link.ConnectedPorts[0], link.ConnectedPorts[1], link.ConnectedNodes[1]));
                targetNetworkObjects.Add(new TargetNetworkObject(link.ConnectedPorts[1], link.ConnectedPorts[0], link.ConnectedNodes[0]));
            }

        }

    }
}
