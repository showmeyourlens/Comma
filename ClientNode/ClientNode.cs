using System;
using System.Collections.Generic;
using System.Text;
using TSST_EON;

namespace ClientNode
{
    class ClientNode
    {
        public List<ClientSenderConfig> contactList;
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            ClientNode clientNode = new ClientNode();
            if (args.Length != 4)
            {
                Console.WriteLine("Wrong parameters quantity. Shutting down.");
                return;
            }

            clientNode.contactList = clientNode.CreateDumbClientConfig(args[1]);

            CloudCommunication cloudCommunicator = new CloudCommunication(args[0], args[1], args[2], args[3]);
            
            Console.WriteLine("Starting client node with following parameters:");
            Console.WriteLine("Address on device: {0}:{1}", cloudCommunicator.instanceAddress, cloudCommunicator.instancePort);
            Console.WriteLine("Address in emulated network: {0}:{1}", cloudCommunicator.emulationNodeAddress, cloudCommunicator.emulationNodePort);
            Console.WriteLine("Node identificator: {0}", cloudCommunicator.emulationNodeId);
            
            try
            {
                Console.WriteLine("Client is up!");
                cloudCommunicator.Start();
                Console.WriteLine("Type number of client for sending message, c to close");
                bool isFinish = true;
                char key;
                while (isFinish)
                {
                    key = Console.ReadKey().KeyChar;
                    ClientSenderConfig contact = clientNode.contactList.Find(x => x.key == key);
                    if (contact != null)
                    {
                        cloudCommunicator.Send(new NetworkPackage(
                        cloudCommunicator.emulationNodeId,
                        cloudCommunicator.emulationNodeAddress,
                        cloudCommunicator.emulationNodePort,
                        contact.receiverId,
                        "Very important message",
                        contact.label
                        ));
                        Console.WriteLine("Message sent");

                    }

                }

            }
            catch (Exception e)
            {

            }
            cloudCommunicator.Stop();
            Console.WriteLine("Closing");
            Console.ReadKey();
        }

        private List<ClientSenderConfig> CreateDumbClientConfig(string thisNodeId)
        {
            List<ClientSenderConfig> result = new List<ClientSenderConfig>();
            switch(thisNodeId)
            {
                case "C1":
                    result.Add(new ClientSenderConfig("C2", 17));
                    result.Add(new ClientSenderConfig("C3", 18));
                    result.Add(new ClientSenderConfig("C4", 19));
                    break;
                case "C2":
                    result.Add(new ClientSenderConfig("C1", 21));
                    result.Add(new ClientSenderConfig("C3", 23));
                    result.Add(new ClientSenderConfig("C4", 24));
                    break;
                case "C3":
                    result.Add(new ClientSenderConfig("C1", 38));
                    result.Add(new ClientSenderConfig("C2", 39));
                    result.Add(new ClientSenderConfig("C4", 40));
                    break;
                case "C4":
                    result.Add(new ClientSenderConfig("C1", 41));
                    result.Add(new ClientSenderConfig("C2", 42));
                    result.Add(new ClientSenderConfig("C3", 43));
                    break;
            }
            return result;
        }
    }

    class ClientSenderConfig
    {
        public char key;
        public string receiverId;
        public int label;

        public ClientSenderConfig(string receiverId, int label)
        {
            this.key = (char)receiverId[1];
            this.receiverId = receiverId;
            this.label = label;
        }

    }
}
