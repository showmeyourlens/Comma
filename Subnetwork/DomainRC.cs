using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class DomainRC
    {
        public Domain domain;
        public string RC_Name;
        string connectionMessage;
        Stack<NetworkPackage> messagesToSend;
        List<RCContact> contacts;
        public DomainRC(Domain domain)
        {
            this.domain = domain;
            RC_Name = "RC_" + domain.emulationNodeId;
            messagesToSend = new Stack<NetworkPackage>();
            contacts = new List<RCContact>();
            contacts.Add(new RCContact("C1", "A_1"));
            contacts.Add(new RCContact("C2", "A_2"));
        }
        public void SetPath(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received SET PATH from {1}", RC_Name, networkPackage.sendingClientId);
            // Wyznacz którędy
            connectionMessage = networkPackage.message;
            // Wielkie sprawdzanie jak dojść do takiego klienta. Na razie oba są w domenie wiec do RC_A_1 i RC_A_2

            RCContact contact1 = contacts.Find(x => String.Equals(x.contactName, networkPackage.message.Split(' ')[0]));
            RCContact contact2 = contacts.Find(x => String.Equals(x.contactName, networkPackage.message.Split(' ')[1]));

            string message = contact1.contactName + " " + contact1.subjectToAsk + " " + contact2.contactName + " " + contact2.subjectToAsk;
            NetworkPackage response = new NetworkPackage(
                RC_Name,
                domain.domainCC.CC_Name,
                Command.Path_Set,
                message
                );
            domain.domainCC.PathSet(response);
            //if (DomainSubnetCheck.IsSubnet(contact1.subjectToAsk))
            //{
            //    NetworkPackage np = networkPackage;
            //    np.message = contact1.contactName + " " + contact1.subjectToAsk;
            //    np.receivingClientId = "CC_" + contact1.subjectToAsk;
            //    np.sendingClientId = RC_Name;
            //    np.MMsgType = Command.Set_Path;
            //    domain.subnetworks.Find(x => x.emulationNodeId == contact1.subjectToAsk).cc.ConnectionRequest(np);
            //}
            //else
            //{
            //    Console.WriteLine("Is not");
            //}

            //if (DomainSubnetCheck.IsSubnet(contact2.subjectToAsk))
            //{
            //    NetworkPackage np = networkPackage;
            //    np.message = contact2.contactName + " " + contact2.subjectToAsk;
            //    np.receivingClientId = "CC_" + contact2.subjectToAsk;
            //    np.sendingClientId = RC_Name;
            //    np.MMsgType = Command.Set_Path;
            //    domain.subnetworks.Find(x => x.emulationNodeId == contact2.subjectToAsk).cc.ConnectionRequest(np);
            //}
            //else
            //{
            //    Console.WriteLine("Is not");
            //}
            //C1 -> N1 -> N4 -> N5 -> C3
            /*
            101 10107 
            10407 10409 
            10509 10511 
            
            */
            //messagesToSend.Clear();
            //connectionMessage = networkPackage.message;
            //messagesToSend.Push(new NetworkPackage(
            //    RC_Name,
            //    "OXC1",
            //    Command.Set_OXC,
            //    "63 65 1"));
            //messagesToSend.Push(new NetworkPackage(
            //    RC_Name,
            //    "OXC2",
            //    Command.Set_OXC,
            //    "111 113 1"
            //    ));
            //messagesToSend.Push(new NetworkPackage(
            //    RC_Name,
            //    "OXC4",
            //    Command.Set_OXC,
            //    "157 159 1"
            //    ));
            //domain.Send(messagesToSend.Pop());
        }

        public void OXCSet(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received OXC SET from {1}", RC_Name, networkPackage.sendingClientId);

            if (messagesToSend.Count() > 0)
            {
                Console.WriteLine(TimeStamp.TAB + " " + messagesToSend.Count() + " OXC confirmation needed");
                domain.Send(messagesToSend.Pop());
            }
            else
            {
                domain.domainCC.PathSet(new NetworkPackage(
                    RC_Name,
                    "CC_" + domain.emulationNodeId,
                    Command.Path_Set,
                    connectionMessage
                    ));
                TimeStamp.WriteLine("{0} >> PATH SET to {1}", RC_Name, "CC_" + domain.emulationNodeId);

            }
        }
    }
}
