using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class NCC
    {
        public Domain subnetwork;
        public string NCC_Name;
        public NCC(Domain subnetwork)
        {
            this.subnetwork = subnetwork;
            NCC_Name = "NCC_" + subnetwork.emulationNodeId;
        }

        public void CallRequest(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CALL REQUEST from {1}", NCC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("NCC_1 >> DIRECTORY REQUEST sent to PC_1");
            TimeStamp.WriteLine("PC_1 >> received DIRECTORY REQUEST CONFIRMED from NCC_1");
            TimeStamp.WriteLine("NCC_1 >> POLICY OUT sent to PC_1");
            TimeStamp.WriteLine("PC_1 >> POLICY CONFIRMED sent to NCC_1");

            string destination = networkPackage.message.Split(' ')[0] + "_CPCC";
            subnetwork.Send(new NetworkPackage(
                NCC_Name,
                destination,
                Command.Call_Indication,
                String.Format("{0} {1}", networkPackage.sendingClientId.Split('_')[0], networkPackage.message)
                )) ;
            TimeStamp.WriteLine("{0} >> CALL INDICATION sent to {1}", NCC_Name, destination);
        }

        public void CallIndication(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CALL INDICATION CONFIRMED from {1}", NCC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> CONNECTION REQUEST sent to {1}", NCC_Name, "CC_" + subnetwork.emulationNodeId);
            subnetwork.cc.ConnectionRequest(new NetworkPackage(
                NCC_Name,
                "CC_" + subnetwork.emulationNodeId,
                Command.Connection_Request,
                networkPackage.message
                ));
           
        }

        public void ConnectionConfirmed(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CONNECTION CONFIRMED from {1}", NCC_Name, networkPackage.receivingClientId);
            string[] connectedClients = networkPackage.message.Split(' ');
            TimeStamp.WriteLine("{0} >> CALL CONFIRMED sent to {1}", NCC_Name, connectedClients[0] + "_CPCC");
            subnetwork.Send(new NetworkPackage(
                NCC_Name,
                connectedClients[0] + "_CPCC",
                Command.Call_Confirmed,
                connectedClients[1]
                ));
            TimeStamp.WriteLine("{0} >> CALL CONFIRMED sent to {1}", NCC_Name, connectedClients[1] + "_CPCC");
            subnetwork.Send(new NetworkPackage(
                NCC_Name,
                connectedClients[1] + "_CPCC",
                Command.Call_Confirmed,
                connectedClients[0]
                ));
        }
    }
}
