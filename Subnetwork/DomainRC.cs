using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class DomainRC
    {
        public Domain domain;
        public string RC_Name;
        string connectionMessage;
        string foundPathMessage;
        string slotsToAllocate;
        Graph graph;
        Stack<NetworkPackage> messagesToSend;
        List<RCContact> contacts;
        public List<RCPath> paths;
        public DomainRC(Domain domain)
        {
            this.domain = domain;
            RC_Name = "RC_" + domain.emulationNodeId;
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
            //TimeStamp.WriteLine("{0} >> Received ROUTE TABLE QUERY REQUEST from {1}", RC_Name, networkPackage.sendingClientId);
            connectionMessage = networkPackage.message;
            /*
             * Big brain time - Wielkie sprawdzanie jak dojść do takiego klienta. Na razie oba są w domenie wiec do RC_A_1 i RC_A_2
             * Zapisać LRM połączenia między podsieciami
             */
            try
            {
                RCContact contact1 = contacts.Find(x => String.Equals(x.contactName, networkPackage.message.Split(' ')[0]));
                RCContact contact2 = contacts.Find(x => String.Equals(x.contactName, networkPackage.message.Split(' ')[1]));

                RCPath path = new RCPath();
                path.from = contact1.subjectToAsk;
                path.to = contact2.subjectToAsk;
                path.CCConnectionId = Int32.Parse(networkPackage.message.Split(' ')[4]);
                //Console.WriteLine(networkPackage.message);


                graph.Dijkstra(path.from, path.to, path);

                if (path.status == ConnectionStatus.InProgress)
                {
                    paths.Add(path);
                    Console.WriteLine("{0} {1} :: found path between {2} and {3}, length: {4}", TimeStamp.TAB, RC_Name, path.from, path.to, path.length);

                }
                else
                {
                    Console.WriteLine("{0} {1} did not found path between {2} and {3}", TimeStamp.TAB, RC_Name, path.from, path.to);
                }

                foundPathMessage = contact1.contactName + " " + contact2.subjectToAsk + " " + contact2.contactName + " " + contact1.subjectToAsk + " " + path.length;

                NetworkPackage response = new NetworkPackage(
                    "CC_" + domain.emulationNodeId,
                    "LRMs",
                    Command.Used_Slots_Request,
                    String.Join(" ", path.LRMids)
                    );
           
            domain.Send(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void UsedCracksResponse(NetworkPackage networkPackage)
        {
            //TimeStamp.WriteLine("{0} >> Received USED SLOTS RESPONSE from {1}", RC_Name, networkPackage.sendingClientId);

            RCPath path = paths.Find(x => x.status == ConnectionStatus.InProgress);
            NetworkPackage response = new NetworkPackage(
                RC_Name,
                domain.domainCC.CC_Name,
                Command.Path_Found,
                foundPathMessage +":" + networkPackage.message// klient_1 podsieć_klienta_2 klient_2 podsieć_klienta_1
                );
            TimeStamp.WriteLine("{0} >> ROUTE TABLE QUERY RESPONSE sent to {1}", RC_Name, "CC_" + networkPackage.receivingClientId);
            TimeStamp.WriteLine("{0} >> USED SLOTS REQUEST sent to {1}", "CC_" + domain.emulationNodeId, "LRMs");

            domain.domainCC.PathFound(response);
        }

        public void FindParams(NetworkPackage networkPackage)
        {

            //TimeStamp.WriteLine("{0} >> Received FIND PATH PARAMS from {1}", RC_Name, networkPackage.sendingClientId);

            /*
             * Big brain time liczenie parametrów ścieżki
             */
            string[] split = networkPackage.message.Split(' ');

            RCPath path = paths.Find(x => x.status == ConnectionStatus.InProgress);
            string signalParams = SignalParamFinder.FindParams(Int32.Parse(split[1]), Int32.Parse(split[0]), new List<int>());
            string[] splittedSignalParams = signalParams.Split(' ');
            slotsToAllocate = splittedSignalParams[1] + " " + splittedSignalParams[2];
            path.startCrack = splittedSignalParams[1];
            path.endCrack = splittedSignalParams[2];
            
            foreach(Subnetwork subnetwork in domain.subnetworks)
            {
                if(subnetwork.rc.paths.Find(x => x.CCConnectionId == path.CCConnectionId) != null)
                {
                    subnetwork.rc.paths.Find(x => x.CCConnectionId == path.CCConnectionId).startCrack = path.startCrack;
                    subnetwork.rc.paths.Find(x => x.CCConnectionId == path.CCConnectionId).endCrack = path.endCrack;
                    //Console.WriteLine("Path {0} updated", path.CCConnectionId);

                }
                else
                {
                    Console.WriteLine("Not found {0} {1}", path.CCConnectionId, subnetwork.rc.paths.First().CCConnectionId);

                }
            }

            NetworkPackage response = new NetworkPackage(
                RC_Name,
                domain.domainCC.CC_Name,
                Command.Path_Params_Found,
                signalParams
                );
            TimeStamp.WriteLine("{0} >> CALCULATE PARAMS RESPONSE sent to {1}", RC_Name,  networkPackage.receivingClientId);
            domain.domainCC.ParamsFound(response);
        }

        public void OXCSet(NetworkPackage networkPackage)
        {
           // TimeStamp.WriteLine("{0} >> Received OXC SET from {1}", RC_Name, networkPackage.sendingClientId);

            if (messagesToSend.Count() > 0)
            {
                Console.WriteLine(TimeStamp.TAB + " " + messagesToSend.Count() + " OXC confirmation needed");
                domain.Send(messagesToSend.Pop());
            }
            else
            {
                domain.domainCC.PathSet(new NetworkPackage(
                    RC_Name,
                    "CC_" + domain.emulationNodeId,
                    Command.Path_Set,
                    connectionMessage
                    ));
                TimeStamp.WriteLine("{0} >> PATH SET to {1}", RC_Name, "CC_" + domain.emulationNodeId);

            }
        }

        internal void SlotsAllocated(NetworkPackage networkPackage)
        {
            TimeStamp.WriteLine("{0} >> Received SLOTS ALLOCATED from {1}", RC_Name, networkPackage.sendingClientId);
        }

    }

    public class RCPath
    {
        public int length;
        public int CCConnectionId;
            
        public string from;
        public string to;
        public List<int> LRMids;
        public List<string> nodes;
        public ConnectionStatus status;
        public string startCrack;
        public string endCrack;
        public string lambda;

        public RCPath()
        {
            LRMids = new List<int>();
            nodes = new List<string>();
            CCConnectionId = -1;
        }

    }

    public class OXCconfig
    {
        public int linkId;
        public int portIn;
        public int portOut;

        public OXCconfig(int oxcid, int portIn, int portOut)
        {
            linkId = oxcid;
            this.portIn = portIn;
            this.portOut = portOut;
        }
    }
}
