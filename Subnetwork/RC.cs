using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class RC
    {
        public Subnetwork subnetwork;
        public string RC_Name;
        string connectionMessage;
        Stack<NetworkPackage> messagesToSend;
        List<RCContact> contacts;
        public RC(Subnetwork subnetwork)
        {
            this.subnetwork = subnetwork;
            RC_Name = "RC_" + subnetwork.emulationNodeId;
            messagesToSend = new Stack<NetworkPackage>();
            contacts = new List<RCContact>();
            if(String.Equals(RC_Name, "RC_A_1"))
            {
                contacts.Add(new RCContact("C1", "OXC1"));
                contacts.Add(new RCContact("A_2", "OXC1"));
            }
            if(String.Equals(RC_Name, "RC_A_2"))
            {
                contacts.Add(new RCContact("C2", "OXC4"));
                contacts.Add(new RCContact("A_1", "OXC2"));
            }
        }
        public void SetPath(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received SET PATH from {1}", RC_Name, networkPackage.sendingClientId);

            messagesToSend.Clear();
            connectionMessage = networkPackage.message;
            Console.WriteLine(TimeStamp.TAB + " " + networkPackage.message);
            // Teraz Dijkstra magic z użyciem dostępnych routerów i kontaktów jako start / end point
            // albo lepiej - ify xD

            if (String.Equals(RC_Name, "RC_A_1"))
            {
                messagesToSend.Push(new NetworkPackage(
                    RC_Name,
                    "OXC1",
                    Command.Set_OXC,
                    "63 65 1"
                    ));
            }
            if (String.Equals(RC_Name, "RC_A_2"))
            {
                messagesToSend.Push(new NetworkPackage(
                    RC_Name,
                    "OXC2",
                    Command.Set_OXC,
                    "111 113 1"
                    ));
                messagesToSend.Push(new NetworkPackage(
                    RC_Name,
                    "OXC4",
                    Command.Set_OXC,
                    "157 159 1"
                    ));
            }
            
            subnetwork.domain.Send(messagesToSend.Pop());
        }

        public void OXCSet(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received OXC SET from {1}", RC_Name, networkPackage.sendingClientId);

            if (messagesToSend.Count() > 0)
            {
                Console.WriteLine(TimeStamp.TAB + " " + messagesToSend.Count() + " OXC confirmation needed");
                subnetwork.domain.Send(messagesToSend.Pop());
            }
            else
            {
                TimeStamp.WriteLine("{0} >> PATH SET to {1}", RC_Name, "CC_" + subnetwork.emulationNodeId);
                subnetwork.cc.PathSet(new NetworkPackage(
                    RC_Name,
                    "CC_" + subnetwork.emulationNodeId,
                    Command.Path_Set,
                    connectionMessage
                    ));

            }
        }
    }
}
