using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class DomainCC
    {
        public Domain domain;
        public string CC_Name;
        public List<Connection> connections;
        public DomainCC(Domain domain)
        {
            this.domain = domain;
            CC_Name = "CC_" + domain.emulationNodeId;
            connections = new List<Connection>();
        }
        public void ConnectionRequest(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received CONNECTION REQUEST from {1}", CC_Name, networkPackage.sendingClientId);
            TimeStamp.WriteLine("{0} >> SET PATH sent to {1}", CC_Name, "RC_" + domain.emulationNodeId);

            connections.Add(new Connection(connections.Count, 0));

            domain.domainRC.SetPath(new NetworkPackage(
                CC_Name,
                "RC_" + domain.emulationNodeId,
                Command.Set_Path,
                networkPackage.message
                ));

        }

        public void PathSet(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received PATH SET from {1}", CC_Name, networkPackage.sendingClientId);
            if (String.Equals(networkPackage.sendingClientId, domain.domainRC.RC_Name))
            {
                string[] split = networkPackage.message.Split(' ');

                if (split.Length == 4)
                {
                    connections.Find(x => x.status == ConnectionStatus.InProgress).confirmationsNeeded += 2;
                    if (DomainSubnetCheck.IsSubnet(split[1]))
                    {
                        NetworkPackage response = new NetworkPackage(
                            CC_Name,
                            domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[1])).cc.CC_Name,
                            Command.Connection_Request,
                            split[0] + " " + split[1]
                            );
                        domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[1])).cc.ConnectionRequest(response);
                    }
                    else
                    {

                    }

                    if (DomainSubnetCheck.IsSubnet(split[3]))
                    {
                        NetworkPackage response = new NetworkPackage(
                            CC_Name,
                            domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[3])).cc.CC_Name,
                            Command.Connection_Request,
                            split[2] + " " + split[3]
                            );
                        domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[3])).cc.ConnectionRequest(response);
                    }
                    else
                    {

                    }

                }
            }
            else
            {
                connections.Find(x => x.status == ConnectionStatus.InProgress).confirmationsNeeded--;
                int confirmations = connections.Find(x => x.status == ConnectionStatus.InProgress).confirmationsNeeded;
                if (confirmations > 0)
                {
                    Console.WriteLine(TimeStamp.TAB + " {0} PATH SET more needed to proceed", confirmations);
                }
                else
                {
                    TimeStamp.WriteLine("{0} >> CONNECTION CONFIRMED sent to {1}", CC_Name, "NCC_" + domain.emulationNodeId);
                    networkPackage.sendingClientId = CC_Name;
                    networkPackage.receivingClientId = "NCC_" + domain.emulationNodeId;
                    networkPackage.MMsgType = Command.Connection_Confirmed;
                    domain.ncc.ConnectionConfirmed(networkPackage);
                }
            }

        }
    }
}
