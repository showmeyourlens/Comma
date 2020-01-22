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
    public class CloudCommunication
    {
        public readonly IPAddress instanceAddress;
        private Socket cloudSocket;
        private Socket clientSocket;
        private ManualResetEvent sendDone;
        private readonly int cloudPort;
        public readonly int instancePort;
        public readonly string emulationNodeId;
        public readonly string emulationNodeAddress;
        public readonly int emulationNodePort;       

        public CloudCommunication(string instancePort, string nodeId, string nodeEmulationAddress, string nodeEmulationPort)
        { 
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.cloudPort = 62572;
            this.instancePort = Int32.Parse(instancePort);
            this.emulationNodeId = nodeId;
            this.emulationNodeAddress = nodeEmulationAddress;
            this.emulationNodePort = Int32.Parse(nodeEmulationPort);
            this.sendDone = new ManualResetEvent(false);
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
                receiverState.WorkSocket.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), receiverState);
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine("Connection lost");
            }
        }

        private NetworkPackage CreateHelloMessage()
        {
            return new NetworkPackage(emulationNodeId, emulationNodeAddress, emulationNodePort);
        }
        private void ProcessReceivedClientMessage(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("Received message from {0}. Message: {1}", networkPackage.sendingClientId, networkPackage.message);
        }
        private void ProcessReceivedManagementMessage(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine(networkPackage.message);
        }
    }
}
