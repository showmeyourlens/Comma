using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class CC
    {
        public Subnetwork subnetwork;
        public string CC_Name;
        public List<Connection> connections;
        public CC(Subnetwork subnetwork)
        {
            this.subnetwork = subnetwork;
            CC_Name = "CC_" + subnetwork.emulationNodeId;
            connections = new List<Connection>();
        }
        public void ConnectionRequest(NetworkPackage networkPackage)
        {
            Connection c = new Connection(Int32.Parse(networkPackage.message.Split(' ')[2]), 0);
            c.from = networkPackage.message.Split(' ')[0];
            c.to = networkPackage.message.Split(' ')[1];
            Console.WriteLine("{0} {1} :: Added connection: {2}", TimeStamp.TAB, CC_Name, Connection.WriteConnection(c));
            connections.Add(c);

            TimeStamp.WriteLine("{0} >> ROUTE TABLE QUERY REQUEST sent to {1}", CC_Name, "RC_" + subnetwork.emulationNodeId);
            subnetwork.rc.FindPath(new NetworkPackage(
                CC_Name,
                "RC_" + subnetwork.emulationNodeId,
                Command.Find_Path,
                networkPackage.message
                ));
        }

        public void PathSet(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received PATH SET from {1}", CC_Name, networkPackage.sendingClientId);
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = subnetwork.domain.domainCC.CC_Name;
            TimeStamp.WriteLine("{0} >> SET PATH RESPONSE sent to {1}", CC_Name, "CC_" + subnetwork.domain.emulationNodeId);
            subnetwork.domain.domainCC.PathSet(networkPackage);
            //NetworkPackage response = new NetworkPackage(
            //    CC_Name,
            //    "LRMs",

            //    );
            //TimeStamp.WriteLine("{0} >> SET PATH RESPONSE sent to {1}", CC_Name, "CC_" + subnetwork.domain.emulationNodeId);
            //subnetwork.domain.domainCC.ConnectionConfirmed(networkPackage);
        }

        public void LinkConnection(NetworkPackage networkPackage)
        {
           // TimeStamp.WriteLine("{0} >> Received L CON REQ from {1}", CC_Name, networkPackage.sendingClientId);
            RCPath path = subnetwork.rc.paths.Find(x => x.status == ConnectionStatus.InProgress);

            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = "LRMs";
            networkPackage.MMsgType = Command.Link_Connection_Request;

            networkPackage.message = String.Join(" ", path.LRMids) + ":" + path.startCrack + " " + path.endCrack;
            TimeStamp.WriteLine("{0} >> LINK CONNECTION REQUEST sent to {1}", CC_Name, networkPackage.receivingClientId);

            subnetwork.domain.Send(networkPackage);
        }

        public void LinkConnectionResponse(NetworkPackage networkPackage)
        {
            // rc status connected
            //TimeStamp.WriteLine("{0} >> Received L CON RESP from {1}", CC_Name, networkPackage.sendingClientId);
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = "CC_" + subnetwork.domain.emulationNodeId;
            networkPackage.MMsgType = Command.Connection_Confirmed;
            Connection c = connections.Find(x => x.status == ConnectionStatus.InProgress);
            Console.WriteLine("{0} {1} :: {2}", TimeStamp.TAB, CC_Name, Connection.ChangeStatus(c, ConnectionStatus.Connected));
            TimeStamp.WriteLine("{0} >> LINK CONNECTION RESPONSE sent to {1}", CC_Name, "CC_" + subnetwork.domain.emulationNodeId);
            subnetwork.domain.domainCC.ConnectionConfirmed(networkPackage);
        }

        public void PathFound(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received PATH FOUND from {1}", CC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> mini CONNECTION REQUEST RESPONSE sent to {1}", CC_Name, "CC_" + subnetwork.domain.emulationNodeId);
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = "CC_" + subnetwork.domain.emulationNodeId;
            networkPackage.MMsgType = Command.Path_Found;
            subnetwork.domain.domainCC.PathFound(networkPackage);
        }

        internal void SetPath(NetworkPackage networkPackage)
        {
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = subnetwork.rc.RC_Name;
            TimeStamp.WriteLine("{0} >> SET PATH REQUEST sent to {1}", CC_Name, networkPackage.receivingClientId);
            subnetwork.rc.SetPath(networkPackage);
        }

        internal void LinkDown(NetworkPackage networkPackage)
        {
            Console.WriteLine("----------------");
            TimeStamp.WriteLine("{0} >> Received LINK DOWN from {1}", CC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> CHECK LINK REQUEST sent to {1}", CC_Name, subnetwork.rc.RC_Name);
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = subnetwork.rc.RC_Name;
            networkPackage.MMsgType = Command.Check_Link_Request;
            subnetwork.rc.CheckLink(networkPackage);

        }

        public void RemoveLinkConnection(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received RM L CON RSP from {1}", "LRMs", networkPackage.sendingClientId, networkPackage.message);

            TimeStamp.WriteLine("{0} >> RESTORE PATH sent to {1}", CC_Name, subnetwork.rc.RC_Name);

            NetworkPackage message = new NetworkPackage(
                CC_Name,
                subnetwork.rc.RC_Name,
                Command.Restore_Path_Request,
                connections.Find(x => x.status == ConnectionStatus.InProgress).id.ToString()
                );
            subnetwork.rc.RestorePath(message);
        }

    }
}
