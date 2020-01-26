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
        public Domain subnetwork;
        public string RC_Name;
        string connectionMessage;
        Stack<NetworkPackage> messagesToSend;
        public RC(Domain subnetwork)
        {
            this.subnetwork = subnetwork;
            RC_Name = "RC_" + subnetwork.emulationNodeId;
            messagesToSend = new Stack<NetworkPackage>();
        }
        public void SetPath(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CONNECTION REQUEST from {1}", RC_Name, networkPackage.sendingClientId);
            //C1 -> N1 -> N4 -> N5 -> C3
            /*
            101 10107 
            10407 10409 
            10509 10511 
            
            */
            messagesToSend.Clear();
            connectionMessage = networkPackage.message;
            messagesToSend.Push(new NetworkPackage(
                RC_Name,
                "OXC1",
                Command.Set_OXC,
                "63 65 1"));
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
            subnetwork.Send(messagesToSend.Pop());
        }

        public void OXCSet(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received OXC SET from {1}", RC_Name, networkPackage.sendingClientId);

            if (messagesToSend.Count() > 0)
            {
                Console.WriteLine(TimeStamp.TAB + " " + messagesToSend.Count() + " OXC confirmation needed");
                subnetwork.Send(messagesToSend.Pop());
            }
            else
            {
                subnetwork.cc.PathSet(new NetworkPackage(
                    RC_Name,
                    "CC_" + subnetwork.emulationNodeId,
                    Command.Path_Set,
                    connectionMessage
                    ));
                TimeStamp.WriteLine("{0} >> PATH SET to {1}", RC_Name, "CC_" + subnetwork.emulationNodeId);

            }
        }
    }
}
