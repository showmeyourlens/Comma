using System;
using System.Collections.Generic;
using System.Text;
using ToolsLibrary;

namespace ClientNodeNS
{
    public class ClientNode
    {
        public List<ClientSenderConfig> contactList;
        public CloudCommunication cloudCommunicator;
        public CPCC cpcc;
        public string domainId;


        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            ClientNode clientNode = new ClientNode();
            if (args.Length != 5)
            {
                Console.WriteLine("Wrong parameters quantity. Shutting down.");
                return;
            }

            clientNode.contactList = clientNode.CreateDumbClientConfig(args[1]);

            clientNode.cloudCommunicator = new CloudCommunication(clientNode, args[0], args[1], args[2], args[3]);
            clientNode.domainId = args[4];
            clientNode.cpcc = new CPCC(clientNode);


            Console.WriteLine("Starting client node with following parameters:");
            Console.WriteLine("Address on device: {0}:{1}", clientNode.cloudCommunicator.instanceAddress, clientNode.cloudCommunicator.instancePort);
            Console.WriteLine("Address in emulated network: {0}:{1}, domain: {2}", clientNode.cloudCommunicator.emulationNodeAddress, clientNode.cloudCommunicator.emulationNodePort, clientNode.domainId);
            Console.WriteLine("Node identificator: {0}", clientNode.cloudCommunicator.emulationNodeId);
            

            try
            {
                Console.WriteLine("Client is up!");
                clientNode.cloudCommunicator.Start();
                Console.WriteLine("Type number of client for sending message, \"k\" and nunber to establish connection, \"c\" to close");
                bool isFinish = true;
                while (isFinish)
                {
                    char key = Console.ReadKey().KeyChar;
                    char connectionClientNumber = 'z';
                    if (key == 'k')
                    {
                        connectionClientNumber = Console.ReadKey().KeyChar;
                        key = connectionClientNumber;
                    }
                    ClientSenderConfig contact = clientNode.contactList.Find(x => x.key == key);

                    contact.demandedCapacity = Convert.ToInt32(2);

                    if (contact != null)
                    {
                        if (connectionClientNumber == 'z')
                        {
                            clientNode.cloudCommunicator.Send(new NetworkPackage(
                            clientNode.cloudCommunicator.emulationNodeId,
                            clientNode.cloudCommunicator.emulationNodeAddress,
                            clientNode.cloudCommunicator.emulationNodePort,
                            contact.receiverId,
                            0.0,
                            "Very important message"));
                            Console.WriteLine("/nMessage sent!");
                        }
                        else //string sendingClientId, string receivingClientId, int sendingClientPort, int capacity
                        {
                            if (!contact.isConnection)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Type bandwidth (in GHz/s)");
                                string ghz = Console.ReadLine();
                                clientNode.cpcc.CallRequest(contact.receiverId, ghz);
                            }
                        }
                    }

                }

            }
            catch (Exception e)
            {

            }
            clientNode.cloudCommunicator.Stop();
            Console.WriteLine("Closing");
            Console.ReadKey();
        }

        private List<ClientSenderConfig> CreateDumbClientConfig(string thisNodeId)
        {
            List<ClientSenderConfig> result = new List<ClientSenderConfig>();
            switch (thisNodeId)
            {
                case "C1":
                    result.Add(new ClientSenderConfig("C2"));
                    result.Add(new ClientSenderConfig("C3"));
                    result.Add(new ClientSenderConfig("C4"));
                    break;
                case "C2":
                    result.Add(new ClientSenderConfig("C1"));
                    result.Add(new ClientSenderConfig("C3"));
                    result.Add(new ClientSenderConfig("C4"));
                    break;
                case "C3":
                    result.Add(new ClientSenderConfig("C1"));
                    result.Add(new ClientSenderConfig("C2"));
                    result.Add(new ClientSenderConfig("C4"));
                    break;
                case "C4":
                    result.Add(new ClientSenderConfig("C1"));
                    result.Add(new ClientSenderConfig("C2"));
                    result.Add(new ClientSenderConfig("C3"));
                    break;
            }
            return result;
        }
    }

    public class ClientSenderConfig
    {
        public char key;
        public string receiverId;
        public bool isConnection;
        public int frequency;
        public int demandedCapacity;

        public ClientSenderConfig(string receiverId)
        {
            this.key = (char)receiverId[1];
            this.receiverId = receiverId;
            this.isConnection = false;
            this.demandedCapacity = 0;
        }

    }
}



