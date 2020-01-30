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
        public string currentBandwidth;
        string destinationCPCC;
        string requestingCPCC;
        public NCC(Domain domain)
        {
            this.domain = domain;
            NCC_Name = "NCC_" + domain.emulationNodeId;
        }

        public void CallRequest(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received CALL REQUEST REQUEST from {1}", NCC_Name, networkPackage.sendingClientId);
            if (true)
            {
                TimeStamp.WriteLine("NCC_" + domain.emulationNodeId + " >> DIRECTORY REQUEST sent to D_" + domain.emulationNodeId);
                TimeStamp.WriteLine("D_" + domain.emulationNodeId + " >> DIRECTORY RESPONSE sent to NCC_" + domain.emulationNodeId);
            }

            TimeStamp.WriteLine("NCC_" + domain.emulationNodeId + " >> CAC REQUEST sent to PC_" + domain.emulationNodeId);
            TimeStamp.WriteLine("PC_" + domain.emulationNodeId + " >> CAC RESPONSE sent to NCC_" + domain.emulationNodeId);

            destinationCPCC = "CPCC_A_1_" + networkPackage.message.Split(' ')[0];
            requestingCPCC = networkPackage.sendingClientId;
            currentBandwidth = networkPackage.message.Split(' ')[1];
            currentCall = String.Format("{0} {1}", networkPackage.sendingClientId.Split('_')[3], networkPackage.message);
            domain.Send(new NetworkPackage(
                NCC_Name,
                destinationCPCC,
                Command.Call_Accept_Request,
                currentCall
                )) ;
            TimeStamp.WriteLine("{0} >> CALL ACCEPT REQUEST sent to {1}", NCC_Name, destinationCPCC);
        }

        public void CallIndication(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received CALL ACCEPT RESPONSE from {1}", NCC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> CONNECTION REQUEST REQUEST sent to {1}", NCC_Name, "CC_" + domain.emulationNodeId);
            domain.domainCC.ConnectionRequest(new NetworkPackage(
                NCC_Name,
                "CC_" + domain.emulationNodeId,
                Command.Connection_Request,
                networkPackage.message + " " + currentBandwidth
                ));
           
        }

        public void ConnectionConfirmed(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received CONNECTION CONFIRMED from {1}", NCC_Name, networkPackage.sendingClientId);
            string[] connectedClients = currentCall.Split(' ');
            TimeStamp.WriteLine("{0} >> CALL REQUEST RESPONSE sent to {1}", NCC_Name, requestingCPCC);
            domain.Send(new NetworkPackage(
                NCC_Name,
                requestingCPCC,
                Command.Call_Confirmed,
                connectedClients[1]
                ));
        }
    }
}
