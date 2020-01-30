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
        public Subnetwork subnetwork;
        public string RC_Name;
        string connectionMessage;
        Stack<NetworkPackage> messagesToSend;
        List<RCContact> contacts;
        public Graph graph;
        public List<RCPath> paths;
        public RC(Subnetwork subnetwork)
        {
            this.subnetwork = subnetwork;
            RC_Name = "RC_" + subnetwork.emulationNodeId;
            messagesToSend = new Stack<NetworkPackage>();
            contacts = new List<RCContact>();
            paths = new List<RCPath>();
            RCParser.ParseConfig(RC_Name, contacts, out graph);

            //foreach (RCContact c in contacts)
            //{
            //    Console.WriteLine("{0} {1}", c.contactName, c.subjectToAsk);
            //}
            //foreach (Edge e in graph.edges)
            //{
            //    Console.WriteLine("{0} {1} {2} {3}", e.id, e.start, e.end, e.length);
            //}
        }
        public void FindPath(NetworkPackage networkPackage)
        {

            RCPath path = new RCPath();
            path.from = networkPackage.message.Split(' ')[0];
            path.to = networkPackage.message.Split(' ')[1];
            path.CCConnectionId = Int32.Parse(networkPackage.message.Split(' ')[2]);
            //Console.WriteLine(networkPackage.message);


            graph.Dijkstra(networkPackage.message.Split(' ')[0], networkPackage.message.Split(' ')[1], path);

            if (path.status == ConnectionStatus.InProgress)
            {
                paths.Add(path);
                Console.WriteLine("{0} {1} :: found path between {2} and {3}, length: {4}", TimeStamp.TAB, RC_Name, path.from, path.to, path.length);
            }
            else
            {
                Console.WriteLine("{0} {1} did not found path between {2} and {3}", TimeStamp.TAB, RC_Name, path.from, path.to);
            }


            TimeStamp.WriteLine("{0} >> ROUTE TABLE QUERY RESPONSE sent {1}", RC_Name, networkPackage.sendingClientId);
            networkPackage.receivingClientId = networkPackage.sendingClientId;
            networkPackage.sendingClientId = RC_Name;
            /*
             * Dodać zajęte szczeliny
             */
            TimeStamp.WriteLine("{0} >> USED SLOTS REQUEST sent to {1}", "CC_" + subnetwork.emulationNodeId, "LRMs");
            NetworkPackage response = new NetworkPackage(
                "CC_" + subnetwork.emulationNodeId,
                "LRMs",
                Command.Used_Slots_Request,
                String.Join(" ", path.LRMids)
                );

            subnetwork.domain.Send(response);
        }

        internal void CheckLink(NetworkPackage networkPackage)
        {
           // TimeStamp.WriteLine("{0} >> Received CHECK LINK REQUEST from {1}", RC_Name, networkPackage.sendingClientId);
           // Console.WriteLine(networkPackage.message);
            Edge affectedEdge = graph.edges.Find(x => x.id == Int32.Parse(networkPackage.message));
            RCPath affectedPath = paths.Find(x => x.LRMids.Contains(affectedEdge.id));
            affectedPath.status = ConnectionStatus.InProgress;
           // Console.WriteLine("Edge: {0} {1} {2}", affectedEdge.id, affectedEdge.start, affectedEdge.end);
            Node node1 = graph.nodes.Find(x => String.Equals(x.id, affectedEdge.start));
            Node node2 = graph.nodes.Find(x => String.Equals(x.id, affectedEdge.end));

            Node node11 = node1.neighbors.Find(x => String.Equals(x.id, node2.id));
            Node node22 = node2.neighbors.Find(x => String.Equals(x.id, node1.id));

            node1.neighbors.Remove(node11);
            node2.neighbors.Remove(node22);

            graph.edges.Remove(affectedEdge);

            TimeStamp.WriteLine("{0} >> CHECK LINK RESPONSE sent to {1}", RC_Name, subnetwork.cc.CC_Name);
            subnetwork.cc.connections.Find(x => x.id == affectedPath.CCConnectionId).status = ConnectionStatus.InProgress;
            NetworkPackage message = new NetworkPackage(
                subnetwork.cc.CC_Name,
                "LRMs",
                Command.Remove_Link_Connection_Request,
                String.Join(" ", affectedPath.LRMids)+ ":" + affectedPath.startCrack + " " + affectedPath.endCrack
                );
            TimeStamp.WriteLine("{0} >> REMOVE LINK CONNECTION REQUEST sent to {1}", message.sendingClientId, message.receivingClientId);
            subnetwork.domain.Send(message);
                  
        }

        public void RestorePath(NetworkPackage networkPackage)
        {
            RCPath path = paths.Find(x => x.CCConnectionId == Int32.Parse(networkPackage.message));
            path.LRMids.Clear();
            path.nodes.Clear();

            graph.Dijkstra(path.from, path.to, path);

            if (path.status == ConnectionStatus.InProgress)
            {
                paths.Add(path);
                Console.WriteLine("{0} {1} found path between {2} and {3}, length: {4}", TimeStamp.TAB, RC_Name, path.from, path.to, path.length);
            }
            else
            {
                Console.WriteLine("{0} {1} did not found path between {2} and {3}", TimeStamp.TAB, RC_Name, path.from, path.to);
            }


            TimeStamp.WriteLine("{0} >> RESTORE PATH RESPONSE sent {1}", RC_Name, networkPackage.sendingClientId);
            networkPackage.receivingClientId = networkPackage.sendingClientId;
            networkPackage.sendingClientId = RC_Name;

            messagesToSend.Clear();
            connectionMessage = networkPackage.message;
            string lambda = path.lambda;

            for (int i = 1; i < path.LRMids.Count; i++)
            {
                if (path.nodes[i].Contains("OXC"))
                {
                    string portConfig = GetPorts(path.LRMids[i - 1], path.nodes[i], path.LRMids[i]);
                    if (portConfig.Length > 0)
                    {
                        
                        messagesToSend.Push(new NetworkPackage(
                            RC_Name,
                            path.nodes[i],
                            Command.Set_OXC,
                            portConfig + " " + lambda + " " + "d"
                            ));
                        subnetwork.domain.Send(messagesToSend.Pop());
                        Thread.Sleep(10);
                    }
                    else
                    {
                        Console.WriteLine("PORT CONFIG ERROR");
                    }
                }
            }
            

            TimeStamp.WriteLine("{0} >> LINK CONNECTION REQUEST sent to {1}", "CC_" + subnetwork.emulationNodeId, "LRMs");
            NetworkPackage response = new NetworkPackage(
                "CC_" + subnetwork.emulationNodeId,
                "LRMs",
                Command.Link_Connection_Request,
                String.Join(" ", path.LRMids) + ":" + path.startCrack + " " + path.endCrack + ":false" 
                );
            subnetwork.domain.Send(response);
        }

        public void UsedSlotsResponse(NetworkPackage networkPackage)
        {
            //path found
            string busyCracks = networkPackage.message;
            networkPackage.message = paths.Find(x => x.status == ConnectionStatus.InProgress).length.ToString() + ":" + busyCracks;
            //Console.WriteLine(networkPackage.message);
            subnetwork.cc.PathFound(networkPackage);
        }
        public void SetPath(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received SET PATH from {1}", RC_Name, networkPackage.sendingClientId);

            messagesToSend.Clear();
            connectionMessage = networkPackage.message;
            string lambda = connectionMessage.Split(' ')[0];
            RCPath path = paths.Find(x => x.status == ConnectionStatus.InProgress);
            path.lambda = lambda;
            for(int i=1; i<path.LRMids.Count; i++)
            {
                if (path.nodes[i].Contains("OXC"))
                {
                    string portConfig = GetPorts(path.LRMids[i - 1], path.nodes[i], path.LRMids[i]);
                    if (portConfig.Length > 0)
                    {                   
                    messagesToSend.Push(new NetworkPackage(
                        RC_Name,
                        path.nodes[i],
                        Command.Set_OXC,
                        portConfig + " " + lambda
                        ));
                    }
                    else
                    {
                        Console.WriteLine("PORT CONFIG ERROR");
                    }
                }
            }
            string communicate = String.Join(" ", messagesToSend.ToList().Select(x => x.receivingClientId));
            Console.WriteLine("{0} {1} SET PATH will be send to: {2}", TimeStamp.TAB, RC_Name, communicate);
            subnetwork.domain.Send(messagesToSend.Pop());
        }

        public string GetPorts(int link1, string node, int link2)
        {
            Edge edge1 = graph.edges.Find(x => x.id == link1);
            Edge edge2 = graph.edges.Find(x => x.id == link2);
            string port1 = "";
            string port2 = "";

            if (String.Equals(edge1.start, node))
            {
                port1 = edge1.startPort.ToString();
            }
            if (String.Equals(edge1.end, node))
            {
                port1 = edge1.endPort.ToString();

            }
            if (String.Equals(edge2.start, node))
            {
                port2 = edge2.startPort.ToString();

            }
            if (String.Equals(edge2.end, node))
            {
                port2 = edge2.endPort.ToString();

            }
            if (port1.Length > 0 && port2.Length > 0)
            {
                return port1 + " " + port2;
            }
            else
            {
                return "";
            }

        }

        public void OXCSet(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received OXC SET from {1}", RC_Name, networkPackage.sendingClientId);

            if (messagesToSend.Count() > 0)
            {
                Console.WriteLine(TimeStamp.TAB + " " + RC_Name +" :: " + messagesToSend.Count() + " OXC confirmation needed");
                subnetwork.domain.Send(messagesToSend.Pop());
            }
            else
            {
                //paths.Find(x => x.status == ConnectionStatus.InProgress).status = ConnectionStatus.Connected;
                TimeStamp.WriteLine("{0} >> SET PATH RESPONSE to {1}", RC_Name, "CC_" + subnetwork.emulationNodeId);

                subnetwork.cc.PathSet(new NetworkPackage(
                    RC_Name,
                    "CC_" + subnetwork.emulationNodeId,
                    Command.Path_Set
                    ));

            }
        }
    }
}
