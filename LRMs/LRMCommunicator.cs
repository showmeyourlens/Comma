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

namespace LRMs
{
    public class LRMCommunicator
    {
        public readonly IPAddress instanceAddress;
        private Socket cloudSocket;
        private Socket clientSocket;
        private ManualResetEvent sendDone;
        private readonly int cloudPort;
        private readonly int instancePort;
        public List<LRM> LRMs;
        public LRMCommunicator(string instancePort)
        {
            this.instanceAddress = IPAddress.Parse("127.0.0.1");
            this.cloudPort = 62572;
            this.instancePort = Int32.Parse(instancePort);
            this.sendDone = new ManualResetEvent(false);
            this.LRMs = LRMParser.ReadLinks();
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
            return new NetworkPackage("LRMs");
        }
        private void ProcessReceivedClientMessage(NetworkPackage networkPackage)
        {
            throw new NotImplementedException();
        }
        private void ProcessReceivedManagementMessage(NetworkPackage networkPackage)
        {
            switch (networkPackage.MMsgType)
            {
                case Command.Used_Slots_Request:
                    UsedCracksRequest(networkPackage);
                    break;
                case Command.Link_Connection_Request:
                    AllocateSlots(networkPackage);
                    break;
                case Command.Remove_Link_Connection_Request:
                    DeallocateSlots(networkPackage);
                    break;
                default:
                    break;
            }
        }

        private void DeallocateSlots(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received RM L CON REQ from {1} {2}", "LRMs", networkPackage.sendingClientId, networkPackage.message);

            string[] splittedMessage = networkPackage.message.Split(':');
            string[] MessageLRMs = splittedMessage[0].Split(' ');

            for (int i = 0; i < MessageLRMs.Length; i++)
            {
                //TimeStamp.WriteLine("{0} >> Received ALLOCATE SLOTS from {1}", "LRM_" + MessageLRMs[i], networkPackage.sendingClientId);
                LRM lrm = LRMs.Find(x => x.linkId == Int32.Parse(MessageLRMs[i]));
                Console.WriteLine("{0} {1} :: slots from {2} to {3} released", TimeStamp.TAB, "LRM_" + MessageLRMs[i], splittedMessage[1].Split(' ')[0], splittedMessage[1].Split(' ')[1]);
                lrm.ReleaseCracks(splittedMessage[1]);
            }
            TimeStamp.WriteLine("{0} >> REMOVE LINK CONNECTION RESPONSE sent to {1}", "LRMs", networkPackage.sendingClientId);
            NetworkPackage response = new NetworkPackage(
                "LRMs",
                networkPackage.sendingClientId,
                Command.Remove_Link_Connection_Response
                );
            Send(response);
        }

        public void AlarmCC(int lrmID)
        {
            LRM lrm = LRMs.Find(x => x.linkId == lrmID);
            foreach(string contact in lrm.contacts)
            {
                TimeStamp.WriteLine("{0} >> LINK DOWN sent to {1}, link: {2}", "LRMs", contact, lrm.linkId);
                Send(new NetworkPackage(
                    "LRMs",
                    contact,
                    Command.Link_Down,
                    lrm.linkId.ToString()
                    ));
            }
        }

        private void AllocateSlots(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received L CON REQ from {1} {2}", "LRMs", networkPackage.sendingClientId, networkPackage.message);

            string[] splittedMessage = networkPackage.message.Split(':');
            string[] MessageLRMs = splittedMessage[0].Split(' ');
     
            for (int i=0; i<MessageLRMs.Length; i++)
            {
                //TimeStamp.WriteLine("{0} >> Received ALLOCATE SLOTS from {1}", "LRM_" + MessageLRMs[i], networkPackage.sendingClientId);
                LRM lrm = LRMs.Find(x => x.linkId == Int32.Parse(MessageLRMs[i]));
                if (lrm.AddCracks(splittedMessage[1]))
                {
                    Console.WriteLine("{0} {1} :: slots from {2} to {3} allocated", TimeStamp.TAB, "LRM_" + MessageLRMs[i], splittedMessage[1].Split(' ')[0], splittedMessage[1].Split(' ')[1]);
                }
                lrm.contacts.Add(networkPackage.sendingClientId);
            }
            TimeStamp.WriteLine("{0} >> LINK CONNECTION RESPONSE sent to {1}", "LRMs", networkPackage.sendingClientId);
            if (splittedMessage.Length != 3)
            {
                NetworkPackage response = new NetworkPackage(
                "LRMs",
                networkPackage.sendingClientId,
                Command.Slots_Allocated
                );
                Send(response);
            }
            
        }

        private void UsedCracksRequest(NetworkPackage networkPackage)
        {
            string[] lrmIds = networkPackage.message.Split(' ');
            List<int> usedCracks = new List<int>();
            for (int i=0; i<lrmIds.Length; i++)
            {
                //TimeStamp.WriteLine("{0} >> Received USED SLOTS REQUEST from {1}", "LRM_" + lrmIds[i], networkPackage.sendingClientId);
                usedCracks.AddRange(LRMs.Find(x => x.linkId == Int32.Parse(lrmIds[i])).busyCracks);
            }
            List<int> distinctList = usedCracks.Distinct().ToList();
            StringBuilder sb = new StringBuilder(); 
            foreach(int crack in distinctList)
            {
                if (distinctList.Count > 0)
                {
                    sb.Append(" ");
                }
                sb.Append(crack);
            }
            NetworkPackage response = new NetworkPackage(
                "LRMs",
                networkPackage.sendingClientId,
                Command.Used_Cracks_Response,
                sb.ToString()
                );
            TimeStamp.WriteLine("{0} >> USED SLOTS RESPONSE sent to {1}", "LRMs", networkPackage.sendingClientId);
            Send(response);
        }
    }
}
