using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace ClientNodeNS
{
    //Calling Party Call Controller
    public class CPCC
    {
        string CPCC_Name;
        ClientNode clientNode;
        public CPCC(ClientNode clientNode)
        {
            this.clientNode = clientNode;
            CPCC_Name = "CPCC_" + clientNode.domainId + "_" + clientNode.cloudCommunicator.emulationNodeId;
        }

        public void CallRequest(string toNode, string bandwidth)
        {
            NetworkPackage networkPackage = new NetworkPackage(
                CPCC_Name,
                "NCC_" + clientNode.domainId,
                Command.Call_Request,
                toNode + " " + bandwidth
                );
            TimeStamp.WriteLine("{0} >> CALL REQUEST sent to {1}", CPCC_Name, networkPackage.receivingClientId);
            clientNode.cloudCommunicator.Send(networkPackage);
        }

        public void CallIndication(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> received CALL INDICATION from {1}", CPCC_Name, networkPackage.sendingClientId);
            clientNode.cloudCommunicator.Send(new NetworkPackage(
                CPCC_Name,
                networkPackage.sendingClientId,
                Command.Call_Indication_Confirmed,
                networkPackage.message
                ));
            TimeStamp.WriteLine("{0} >> CALL INDICATION CONFIRMED sent to {1}", CPCC_Name, networkPackage.sendingClientId);

        }

        internal void CallConfirmed(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> received CALL CONFIRMED from {1}", CPCC_Name, networkPackage.sendingClientId);
            clientNode.contactList.Find(x => x.receiverId == networkPackage.message).isConnection = true;
        }
    }
}
