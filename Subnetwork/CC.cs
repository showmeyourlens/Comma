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
        public Domain subnetwork;
        public string CC_Name;
        public CC(Domain subnetwork)
        {
            this.subnetwork = subnetwork;
            CC_Name = "CC_" + subnetwork.emulationNodeId;
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
            TimeStamp.WriteLine("{0} >> CONNECTION CONFIRMED sent to {1}", CC_Name, "NCC_" + subnetwork.emulationNodeId);
            networkPackage.sendingClientId = CC_Name;
            networkPackage.receivingClientId = "NCC_" + subnetwork.emulationNodeId;
            networkPackage.MMsgType = Command.Connection_Confirmed;
            subnetwork.ncc.ConnectionConfirmed(networkPackage);
        }
    }
}
