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
            // TimeStamp.WriteLine("{0} >> Received CONNECTION REQUEST from {1}", CC_Name, networkPackage.sendingClientId);

            Connection c = new Connection(connections.Count, 0);
            c.from = networkPackage.message.Split(' ')[0];
            c.to = networkPackage.message.Split(' ')[1];
            c.bandwidth = Int32.Parse(networkPackage.message.Split(' ')[2]);
            Console.WriteLine("{0} {1} :: Added connection: {2}", TimeStamp.TAB, CC_Name, Connection.WriteConnection(c));
            connections.Add(c);

            TimeStamp.WriteLine("{0} >> ROUTE TABLE QUERY REQUEST sent to {1}", CC_Name, "RC_" + domain.emulationNodeId);
            domain.domainRC.FindPath(new NetworkPackage(
                CC_Name,
                "RC_" + domain.emulationNodeId,
                Command.Find_Path,
                networkPackage.message + " " + c.id // From client to client
                ));

        }

        public void PathFound(NetworkPackage networkPackage)
        {
            // TimeStamp.WriteLine("{0} >> Received PATH FOUND from {1}", CC_Name, networkPackage.sendingClientId);
            string[] messageAndSlots = networkPackage.message.Split(':');
            string[] split = messageAndSlots[0].Split(' ');

            if (String.Equals(networkPackage.sendingClientId, domain.domainRC.RC_Name))
            {
                if (split.Length == 5)
                {
                    Connection c = connections.Find(x => x.status == ConnectionStatus.InProgress);
                    c.confirmationsNeeded += 2;
                    c.nodesInConnection.Add(split[3]);
                    c.nodesInConnection.Add(split[1]);
                    c.length += Int32.Parse(split[4]);

                    if (messageAndSlots.Length == 2)
                    {
                        c.ParseBusyCracks(messageAndSlots[1]);
                    }

                    if (DomainSubnetCheck.IsSubnet(split[3]))
                    {
                        NetworkPackage response = new NetworkPackage(
                            CC_Name,
                            domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[1])).cc.CC_Name,
                            Command.Connection_Request,
                            split[2] + " " + split[3] + " " + connections.Find(x => x.status == ConnectionStatus.InProgress).id
                            );
                        TimeStamp.WriteLine("{0} >> mini CONNECTION REQUEST REQUEST sent to {1}", CC_Name, response.receivingClientId);
                        domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[1])).cc.ConnectionRequest(response);
                    }
                    else
                    {

                    }

                    if (DomainSubnetCheck.IsSubnet(split[1]))
                    {
                        NetworkPackage response = new NetworkPackage(
                            CC_Name,
                            domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[3])).cc.CC_Name,
                            Command.Connection_Request,
                            split[0] + " " + split[1] + " " + connections.Find(x => x.status == ConnectionStatus.InProgress).id
                            );
                        TimeStamp.WriteLine("{0} >> mini CONNECTION REQUEST REQUEST sent to {1}", CC_Name, response.receivingClientId);
                        domain.subnetworks.Find(x => String.Equals(x.emulationNodeId, split[3])).cc.ConnectionRequest(response);
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                Connection c = connections.Find(x => x.status == ConnectionStatus.InProgress);
                c.confirmationsNeeded--;

                c.length += Int32.Parse(messageAndSlots[0]);
                if (messageAndSlots.Length == 2)
                {
                    c.ParseBusyCracks(messageAndSlots[1]);
                }

                int confirmations = connections.Find(x => x.status == ConnectionStatus.InProgress).confirmationsNeeded;
                if (confirmations > 0)
                {

                    Console.WriteLine("{0} {1} :: {2} mini CONNECTION REQUEST RESPONSE more needed to proceed", TimeStamp.TAB, CC_Name, confirmations);
                    /*
                     * Do tego czekanie na parametry z 2 domeny
                     */
                }
                else
                {
                    c.confirmationsNeeded = 2;
                    TimeStamp.WriteLine("{0} >> CALCULATE PARAMS REQUEST sent to {1}", CC_Name, domain.domainRC.RC_Name);
                    NetworkPackage response = new NetworkPackage(
                        CC_Name,
                        domain.domainRC.RC_Name,
                        Command.Find_Path_Params,
                        c.length.ToString() + " " + c.bandwidth
                        );
                    domain.domainRC.FindParams(response);
                }
            }
        }
        public void PathSet(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received PATH SET from {1}", CC_Name, networkPackage.sendingClientId);

            RCPath currentPath = domain.domainRC.paths.Find(x => x.status == ConnectionStatus.InProgress);
            Connection c = connections.Find(x => x.status == ConnectionStatus.InProgress);
            c.confirmationsNeeded--;
            if (c.confirmationsNeeded > 0)
            {
                Console.WriteLine("{0} {1} :: {2} more PATH SET needed", TimeStamp.TAB, CC_Name, c.confirmationsNeeded);
            }
            else
            {
                TimeStamp.WriteLine("{0} >> LINK CONNECTION REQUEST sent to {1}", CC_Name, "LRMs");
                c.confirmationsNeeded = 2;
                NetworkPackage message = new NetworkPackage(
                CC_Name,
                "LRMs",
                Command.Link_Connection_Request,
                String.Join(" ", currentPath.LRMids) + ":" + currentPath.startCrack + " " + currentPath.endCrack
                );
                domain.Send(message);
            }
          
        }

        public void LinkConnectionResponse(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received L CON RES from {1}", CC_Name, networkPackage.sendingClientId);
            RCPath currentPath = domain.domainRC.paths.Find(x => x.status == ConnectionStatus.InProgress);

            foreach (Subnetwork subnetwork in domain.subnetworks)
            {
                NetworkPackage message = new NetworkPackage(
                    CC_Name,
                    subnetwork.cc.CC_Name,
                    Command.Link_Connection_Request,
                    connections.Find(x => x.status == ConnectionStatus.InProgress).id + ":" + currentPath.startCrack + " " + currentPath.endCrack
                    );
                TimeStamp.WriteLine("{0} >> LINK CONNECTION REQUEST sent to {1}", CC_Name, message.receivingClientId);
                subnetwork.cc.LinkConnection(message);
            }
        }

        public void ParamsFound(NetworkPackage networkPackage)
        {
            // TimeStamp.WriteLine("{0} >> Received PATH PARAMS FOUND from {1}", CC_Name, networkPackage.sendingClientId);

            /*
             * Przekazanie parametrów do 2 domeny
             */
            Connection c = connections.Find(x => x.status == ConnectionStatus.InProgress);
            foreach (Subnetwork subnetwork in domain.subnetworks)
            {
                
                NetworkPackage response = new NetworkPackage(
                    CC_Name,
                    subnetwork.cc.CC_Name,
                    Command.Set_OXC,
                    networkPackage.message
                    );
                TimeStamp.WriteLine("{0} >> SET PATH REQUEST sent to {1}", CC_Name, response.receivingClientId);
                subnetwork.cc.SetPath(response);
            }

        }

        public void ConnectionConfirmed(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received CONNECTION CONFIRMED from {1}", CC_Name, networkPackage.sendingClientId);
            connections.Find(x => x.status == ConnectionStatus.InProgress).confirmationsNeeded--;
            int confirmations = connections.Find(x => x.status == ConnectionStatus.InProgress).confirmationsNeeded;
            if (confirmations > 0)
            {
                Console.WriteLine("{0} {1} :: {2} CONNECTION REQUEST RESPONSE more needed to proceed", TimeStamp.TAB, CC_Name, confirmations);
            }
            else
            {
                TimeStamp.WriteLine("{0} >> CONNECTION REQUEST RESPONSE sent to {1}", CC_Name, "NCC_" + domain.emulationNodeId);
                networkPackage.sendingClientId = CC_Name;
                networkPackage.receivingClientId = "NCC_" + domain.emulationNodeId;
                networkPackage.MMsgType = Command.Connection_Confirmed;
                Connection c = connections.Find(x => x.status == ConnectionStatus.InProgress);
                Console.WriteLine("{0} {1} :: {2}", TimeStamp.TAB, CC_Name, Connection.ChangeStatus(c, ConnectionStatus.Connected));
                string message = c.from + " " + c.to;
                domain.ncc.ConnectionConfirmed(networkPackage);
            }
        }

        internal void LinkDown(NetworkPackage networkPackage)
        {
            throw new NotImplementedException();
        }
    }
}
