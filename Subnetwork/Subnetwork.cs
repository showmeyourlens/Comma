using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;
using TSST_EON;

namespace Subnetwork
{
    class Subnetwork
    {
        IPAddress SubnetworkAddress;
        int SubnetworkPort;
        string SubnetworkId;
        Socket NccSocket;
        ManualResetEvent sendDone;
        static void Main(string[] args)
        {
            Subnetwork subnet = new Subnetwork(args[0],args[1],args[2]);
        }
        Subnetwork(string SubnetworkId, string SubnetworkAddress, string SubnetworkPort)
        {
            this.SubnetworkId = SubnetworkId;
            this.SubnetworkAddress = IPAddress.Parse(SubnetworkAddress);
            this.SubnetworkPort = int.Parse(SubnetworkPort);
            try
            {
                Task.Run(() => StartSubentwork());
            }
            catch (Exception e)
            {

            }
        }
        private void StartSubentwork()
        {
            NccSocket = new Socket(SubnetworkAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (SubnetworkId == "Subnetwork_1")
            {
                try
                {
                    NccSocket.Bind(new IPEndPoint(SubnetworkAddress, SubnetworkPort));
                    NccSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.10"), 63800), new AsyncCallback(ConnectCallback), NccSocket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (SubnetworkId == "Subnetwork_2")
            {
                try
                {
                    NccSocket.Bind(new IPEndPoint(SubnetworkAddress, SubnetworkPort));
                    NccSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("128.0.0.10"), 63800), new AsyncCallback(ConnectCallback), NccSocket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                ReceiverState state = new ReceiverState();
                state.WorkSocket = (Socket)ar.AsyncState;
                NccSocket = state.WorkSocket;
                NccSocket.EndConnect(ar);

                Task.Run(() => Receive(NccSocket));
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

        private void ReadCallback(IAsyncResult ar)
        {
            // Odbiór pakietu
            try
            {
                ReceiverState receiverState = (ReceiverState)ar.AsyncState;
                Socket handler = receiverState.WorkSocket;
                int bytesRead = handler.EndReceive(ar);


                NetworkPackage received = Deserialize(receiverState, bytesRead);

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

        public void Send(NetworkPackage networkPackage)
        {
            sendDone.Reset();
            ReceiverState state = new ReceiverState();
            state.WorkSocket = NccSocket;
            state.Buffer = SerializeMessage(networkPackage);
            NccSocket.BeginSend(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(SendCallback), state);
            sendDone.WaitOne();
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

                receiverState.WorkSocket.BeginReceive(receiverState.Buffer, 0, receiverState.Buffer.Length, 0, new AsyncCallback(ReceiveCallback), receiverState);
            }
            catch (Exception e)
            {
                TimeStamp.WriteLine(e.Message);
            }

        }
    }
}
