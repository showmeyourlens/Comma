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
            TimeStamp.WriteLine("{0} >> Received CONNECTION REQUEST from {1}", CC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> SET PATH sent to {1}", CC_Name, "RC_" + subnetwork.emulationNodeId);
            subnetwork.rc.SetPath(new NetworkPackage(
                CC_Name,
                "RC_" + subnetwork.emulationNodeId,
                Command.Set_Path,
                networkPackage.message
                ));
        }

        public void PathSet(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received PATH SET from {1}", CC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> CONNECTION CONFIRMED sent to {1}", CC_Name, "CC_" + subnetwork.domain.emulationNodeId);
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = "CC_" + subnetwork.domain.emulationNodeId;
            networkPackage.MMsgType = Command.Path_Set;
            subnetwork.domain.domainCC.PathSet(networkPackage);
        }
    }
}
