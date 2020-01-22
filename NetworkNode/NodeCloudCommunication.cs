using NetworkNode;
using System;
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
        public FIBXMLReader Reader;
        public RoutingInfo routingInfo;
        public readonly IPAddress instanceAddress;
        private Socket cloudSocket;
        private Socket clientSocket;
        private ManualResetEvent sendDone;
        private readonly int cloudPort;
        public readonly int instancePort;
        public readonly string emulationNodeId;
        public readonly string emulationNodeAddress;
        public readonly int emulationNodePort;
        public bool isRouterUp; 

        
        public NodeCloudCommunication(string instancePort, string nodeId, string nodeEmulationAddress, string nodeEmulationPort)
        {
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.cloudPort = 62572;
            this.instancePort = Int32.Parse(instancePort);
            this.emulationNodeId = nodeId;
            this.emulationNodeAddress = nodeEmulationAddress;
            this.emulationNodePort = Int32.Parse(nodeEmulationPort);
            this.Reader = new FIBXMLReader();
            //this.routingInfo;
            this.sendDone = new ManualResetEvent(false);
            this.isRouterUp = true;
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
            return new NetworkPackage(emulationNodeId, emulationNodeAddress, emulationNodePort);
        }

        private void ProcessReceivedClientMessage(NetworkPackage networkPackage)
        {
            // TimeStamp.WriteLine("Received message from {0} to {1}.", networkPackage.sendingClientId, networkPackage.receivingClientId);
            TimeStamp.WriteLine("Received message.");

            // RISKY - if message from client, add label "0" (e.g. assume there is one)
            if (networkPackage.labelStack.Count == 0)
            {
                networkPackage.labelStack.Push(0);
            }

            int topLabel = networkPackage.labelStack.Peek();
            
            RouterLabel routerLabel = routingInfo.routerLabels.Find(x => x.inputPort == networkPackage.currentPort && x.label == topLabel);
            RouterAction action = routerLabel != null ? routingInfo.routerActions.Find(x => x.actionId == routerLabel.action) : null;
            
            
            if (DoSelectedAction(action, networkPackage))
            {
                Console.WriteLine("{0} Passing packet on port {1}.", TimeStamp.TAB, networkPackage.currentPort);
                Send(networkPackage);
            }
            
        }

        private void ProcessReceivedManagementMessage(NetworkPackage networkPackage)
        {
            if (networkPackage.message.Equals("ROUTING_SCHEME_2"))
            {
                TimeStamp.WriteLine("MANAGEMENT MESSAGE: upload new forwarding table");
                routingInfo = Reader.ReadFIB("ManagementSystem2.xml", emulationNodeId);
                Console.WriteLine("{0} upload done. ", TimeStamp.TAB);
            }
        }

        private bool DoSelectedAction(RouterAction action, NetworkPackage networkPackage)
        {
            if (action == null)
            {
                TimeStamp.WriteLine("ERROR - no action defined (port {0}, label {1}). Discarded");
                return false;
            }
            try
            {
                switch (action.actionString)
                {
                    case "POP":
                        int deletedLabel = networkPackage.labelStack.Pop();
                        Console.WriteLine("{0} Deleted label {1}, considering label {2}", TimeStamp.TAB, deletedLabel, networkPackage.labelStack.Peek());
                        action = routingInfo.routerActions.Find(x => x.actionId == routingInfo.routerLabels.Find(y => y.label == networkPackage.labelStack.Peek()).action);
                        DoSelectedAction(action, networkPackage);
                        break;
                    case "SWAP":
                        Console.WriteLine("{0} Label {1} switched to {2}", TimeStamp.TAB, networkPackage.labelStack.Pop(), action.outLabel);
                        networkPackage.labelStack.Push(action.outLabel);
                        break;
                    case "PUSH":
                        Console.WriteLine("{0} Added label {1}", TimeStamp.TAB, action.outLabel);
                        networkPackage.labelStack.Push(action.outLabel);
                        break;
                }
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine("Error with label stack");
                Console.WriteLine(e.Message);
                return false;

            }
            if(action.nextActionId != 0)
            {
                DoSelectedAction(routingInfo.routerActions.Find(x => x.actionId == action.nextActionId), networkPackage);
            }
            if(action.outPort != 0)
            {
                networkPackage.currentPort = action.outPort;
                networkPackage.currentIP = emulationNodeAddress;
            }
            return true;

        }
    }
}

