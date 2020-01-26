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
        public Domain domain;
        public string NCC_Name;
        public string currentCall;
        public NCC(Domain domain)
        {
            this.domain = domain;
            NCC_Name = "NCC_" + domain.emulationNodeId;
        }

        public void CallRequest(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CALL REQUEST from {1}", NCC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine(":::::NCC_1 >> DIRECTORY REQUEST sent to PC_1");
            TimeStamp.WriteLine("::::PC_1 >> received DIRECTORY REQUEST CONFIRMED from NCC_1");
            TimeStamp.WriteLine("::::NCC_1 >> POLICY OUT sent to PC_1");
            TimeStamp.WriteLine("::::PC_1 >> POLICY CONFIRMED sent to NCC_1");

            string destination = "CPCC_A_1_" + networkPackage.message.Split(' ')[0];
            currentCall = String.Format("{0} {1}", networkPackage.sendingClientId.Split('_')[3], networkPackage.message);
            domain.Send(new NetworkPackage(
                NCC_Name,
                destination,
                Command.Call_Indication,
                currentCall
                )) ;
            TimeStamp.WriteLine("{0} >> CALL INDICATION sent to {1}", NCC_Name, destination);
        }

        public void CallIndication(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CALL INDICATION CONFIRMED from {1}", NCC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> CONNECTION REQUEST sent to {1}", NCC_Name, "CC_" + domain.emulationNodeId);
            domain.domainCC.ConnectionRequest(new NetworkPackage(
                NCC_Name,
                "CC_" + domain.emulationNodeId,
                Command.Connection_Request,
                networkPackage.message
                ));
           
        }

        public void ConnectionConfirmed(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CONNECTION CONFIRMED from {1}", NCC_Name, networkPackage.sendingClientId);
            string[] connectedClients = currentCall.Split(' ');
            TimeStamp.WriteLine("{0} >> CALL CONFIRMED sent to {1}", NCC_Name, "CPCC_A_1_" + connectedClients[0]);
            domain.Send(new NetworkPackage(
                NCC_Name,
                "CPCC_A_1_" + connectedClients[0],
                Command.Call_Confirmed,
                connectedClients[1]
                ));
            TimeStamp.WriteLine("{0} >> CALL CONFIRMED sent to {1}", NCC_Name, "CPCC_A_2_" + connectedClients[1]);
            domain.Send(new NetworkPackage(
                NCC_Name,
                "CPCC_A_1_" + connectedClients[1],
                Command.Call_Confirmed,
                connectedClients[0]
                ));
        }
    }
}
