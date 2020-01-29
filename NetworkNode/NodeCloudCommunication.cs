using NetworkNode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace CableCloud
{
    public class NodeCloudCommunication
    {
        public readonly IPAddress instanceAddress;
        private Socket cloudSocket;
        private Socket clientSocket;
        private ManualResetEvent sendDone;
        private readonly int cloudPort;
        public readonly int instancePort;
        public readonly string emulationNodeId;
        public readonly string emulationNodeAddress;
        public bool isRouterUp;
        private List<FIBEntry> forwardingTable;

        public NodeCloudCommunication(string instancePort, string nodeId, string nodeEmulationAddress)
        {
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.cloudPort = 62572;
            this.instancePort = Int32.Parse(instancePort);
            this.emulationNodeId = nodeId;
            this.emulationNodeAddress = nodeEmulationAddress;
            this.sendDone = new ManualResetEvent(false);
            this.isRouterUp = true;
            forwardingTable = new List<FIBEntry>();
        }
        public void Start()
        {
            clientSocket = new Socket(instanceAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Bind(new IPEndPoint(instanceAddress, instancePort));
                clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), cloudPort), new AsyncCallback(ConnectCallback), clientSocket);
            }
            catch (Exception e)
            {

            }
        }

        public void Stop()
        {
            cloudSocket.Disconnect(false);
            clientSocket.Disconnect(false);
            clientSocket.Close();
            cloudSocket.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState state = new ReceiverState();
                state.WorkSocket = (Socket)ar.AsyncState;
                cloudSocket = state.WorkSocket;
                cloudSocket.EndConnect(ar);
                // Sending HELLO message to cloud
                Send(CreateHelloMessage());

                Task.Run(() => Receive(cloudSocket));
            }
            catch (ObjectDisposedException e0)
            {
                Console.WriteLine(e0);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        private byte[] SerializeMessage(NetworkPackage networkPackage)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, networkPackage);
                return ms.ToArray();
            }
        }

        private NetworkPackage DeserializeMessage(ReceiverState receiverState, int byteRead)
        {
            using (var memoryStream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                memoryStream.Write(receiverState.Buffer, 0, byteRead);
                memoryStream.Seek(0, SeekOrigin.Begin);
                NetworkPackage obj = (NetworkPackage)bf.Deserialize(memoryStream);
                return obj;
            }
        }

        public void Send(NetworkPackage networkPackage)
        {
            sendDone.Reset();
            ReceiverState state = new ReceiverState();
            state.WorkSocket = clientSocket;
            state.Buffer = SerializeMessage(networkPackage);
            clientSocket.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(SendCallback), state);
            sendDone.WaitOne();
        }

        private void Receive(Socket socket)
        {
            ReceiverState state = new ReceiverState();
            state.WorkSocket = socket;
            socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState state = (ReceiverState)ar.AsyncState;
                state.WorkSocket.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            ReceiverState receiverState = (ReceiverState)ar.AsyncState;
            try
            {
                if (isRouterUp)
                {
                    int bytesRead = receiverState.WorkSocket.EndReceive(ar);
                    NetworkPackage networkPackage = DeserializeMessage(receiverState, bytesRead);
                    if (networkPackage.helloMessage)
                    {
                        TimeStamp.WriteLine("Connected to cloud");
                    }
                    if (networkPackage.managementMessage)
                    {
                        ProcessReceivedManagementMessage(networkPackage);
                    }
                    if (!networkPackage.managementMessage && !networkPackage.helloMessage)
                    {
                        ProcessReceivedClientMessage(networkPackage);
                    }
                }
                else
                {
                    Console.WriteLine("Router is down. Message discarded");
                }
                receiverState.WorkSocket.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), receiverState);
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine(e.Message);
            }

        }

        private NetworkPackage CreateHelloMessage()
        {
            return new NetworkPackage(emulationNodeId, emulationNodeAddress, 0);
        }

        private void ProcessReceivedClientMessage(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("Client Message on port" + networkPackage.currentPort);

            FIBEntry searchedEntry = forwardingTable.Find(x => x.inputPort == networkPackage.currentPort);

            if (searchedEntry != null)
            {
                networkPackage.currentPort = searchedEntry.outputPort;
                Console.WriteLine(TimeStamp.TAB + " Passing to " + networkPackage.currentPort);
                Send(networkPackage);
            }
            else
            {
                Console.WriteLine("no forwarding entry");
            }
        }

        private void ProcessReceivedManagementMessage(NetworkPackage networkPackage)
        {
            if (networkPackage.MMsgType == Command.Set_OXC)
            {
                TimeStamp.WriteLine("{0} >> Received SET OXC from {1}", emulationNodeId, networkPackage.sendingClientId);
                string[] split = networkPackage.message.Split(' ');
                if (split.Length == 4)
                {
                    if (forwardingTable.Count > 0)
                    {
                        Console.WriteLine("{0} Setup for lambda {1} deleted", TimeStamp.TAB, forwardingTable[0].lambda);
                        forwardingTable.Clear();
                    }
                    
                    CreateEntry(split[0], split[1], split[2]);
                }
                else
                {
                    CreateEntry(split[0], split[1], split[2]);
                    Send(new NetworkPackage(emulationNodeId, networkPackage.sendingClientId, Command.OXC_Set));
                }
                TimeStamp.WriteLine("{0} >> Sent OXC SET to {1}", emulationNodeId, networkPackage.sendingClientId);
            }
        }

        public void CreateEntry(string port1, string port2, string frequency)
        {
            Console.WriteLine(String.Format("{0} Created entry: from port {1} pass to {2} when lambda is {3}", TimeStamp.TAB, port1, port2, frequency));
            forwardingTable.Add(new FIBEntry(Int32.Parse(port1), Int32.Parse(port2), Double.Parse(frequency)));
            Console.WriteLine(String.Format("{0} Created entry: from port {1} pass to {2} when lambda is {3}", TimeStamp.TAB, port2, port1, frequency));
            forwardingTable.Add(new FIBEntry(Int32.Parse(port2), Int32.Parse(port1), Double.Parse(frequency)));
        }

        public class FIBEntry
        {
            public int inputPort;
            public int outputPort;
            public double lambda;

            public FIBEntry(int inputPort, int outputPort, double lambda)
            {
                this.inputPort = inputPort;
                this.outputPort = outputPort;
                this.lambda = lambda;
            }
        }
    }
}

