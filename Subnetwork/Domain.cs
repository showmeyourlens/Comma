using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class Domain
    {
        public readonly IPAddress instanceAddress;
        private Socket cloudSocket;
        private Socket clientSocket;
        private ManualResetEvent sendDone;
        private readonly int cloudPort;
        public readonly int instancePort;
        public readonly string emulationNodeId;
        public List<Subnetwork> subnetworks;
        public NCC ncc;
        public DomainCC domainCC;
        public DomainRC domainRC;


        static void Main(string[] args)
        {
            Domain domain = new Domain(args[0], args[1]);
            domain.Start();
            char key = 'k';
            while (key != 'c')
            {
                Console.ReadKey();
            }
            domain.Stop();
        }
        public Domain(string nodeId, string instancePort)
        {
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.instancePort = Int32.Parse(instancePort);
            this.cloudPort = 62572;
            this.emulationNodeId = nodeId;
            this.sendDone = new ManualResetEvent(false);
            this.ncc = new NCC(this);
            this.domainCC = new DomainCC(this);
            this.domainRC = new DomainRC(this);
            this.subnetworks = new List<Subnetwork>();
            subnetworks.Add(new Subnetwork(this, "1"));
            subnetworks.Add(new Subnetwork(this, "2"));
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
                int bytesRead = receiverState.WorkSocket.EndReceive(ar);
                NetworkPackage networkPackage = DeserializeMessage(receiverState, bytesRead);
                if (networkPackage.helloMessage)
                {
                   // TimeStamp.WriteLine("Connected to cloud");
                }
                if (networkPackage.managementMessage)
                {
                    ProcessReceivedManagementMessage(networkPackage);
                }
                receiverState.WorkSocket.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), receiverState);
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine("Connection lost");
            }
        }

        private NetworkPackage CreateHelloMessage()
        {
            return new NetworkPackage(emulationNodeId);
        }
        private void ProcessReceivedClientMessage(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("Received message from {0}. Message: {1}", networkPackage.sendingClientId, networkPackage.message);
        }
        private void ProcessReceivedManagementMessage(NetworkPackage networkPackage)
        {
            switch (networkPackage.MMsgType)
            {
                case Command.Call_Request_Request:
                    ncc.CallRequest(networkPackage);
                    break;
                case Command.Call_Accept_Confirmed:
                    ncc.CallIndication(networkPackage);
                    break;
                case Command.OXC_Set:
                    if(String.Equals(networkPackage.receivingClientId, domainRC.RC_Name))
                    {
                        domainRC.OXCSet(networkPackage);
                    }
                    else
                    {
                        subnetworks.Find(x => String.Equals(networkPackage.receivingClientId, x.emulationNodeId)).rc.OXCSet(networkPackage);
                    }
                    break;
                case Command.Path_Set:
                    domainCC.PathSet(networkPackage);
                    break;
                case Command.Used_Cracks_Response:
                    string[] split = networkPackage.receivingClientId.Split('_');
                    if (split.Length == 1)
                    {
                        domainRC.UsedCracksResponse(networkPackage);
                    }
                    else
                    {
                        subnetworks.Find(x => String.Equals(x.emulationNodeId, networkPackage.receivingClientId)).rc.UsedSlotsResponse(networkPackage);
                    }
                    break;
                case Command.Slots_Allocated:
                    string[] split1 = networkPackage.receivingClientId.Split('_');
                    if (split1.Length == 1)
                    {
                        domainCC.LinkConnectionResponse(networkPackage);
                    }
                    else
                    {
                        subnetworks.Find(x => String.Equals(x.emulationNodeId, networkPackage.receivingClientId)).cc.LinkConnectionResponse(networkPackage);
                    }
                    break;
                case Command.Link_Down:
                    if (String.Equals(networkPackage.receivingClientId, domainCC.CC_Name))
                    {
                        domainCC.LinkDown(networkPackage);
                    }
                    else
                    {
                        subnetworks.Find(x => String.Equals(networkPackage.receivingClientId, x.emulationNodeId)).cc.LinkDown(networkPackage);
                    }
                    break;
                case Command.Remove_Link_Connection_Response:
                    
                        subnetworks.Find(x => String.Equals(networkPackage.receivingClientId, x.emulationNodeId)).cc.RemoveLinkConnection(networkPackage);
                    
                    break;
                default:
                    if (!String.Equals(networkPackage.sendingClientId, "Cloud"))
                    {
                        Console.WriteLine();
                    }
                    break;
            }
        }
    }
}
