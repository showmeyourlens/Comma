using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{

    class Graph
    {
        public
        List<Node> nodes = new List<Node>();
        public
        List<Edge> edges = new List<Edge>();
        /// <summary>
        /// liczba wezlow
        /// </summary>
        public int numberNodes;
        /// <summary>
        /// liczba laczy
        /// </summary>
        public int numberEdges;

        public Graph(int e, int n)
        {
            numberNodes = n;
            numberEdges = e;
        }

        /*
        public void readNewTopologyFromLRM(List<LRMRow> values)
        {
            foreach (LRMRow row in values)
            {
                foreach (Edge Edge in edges)
                {
                    if (row.nodeID1 == Edge.GetStart() && row.nodeID2 == Edge.GetEnd())
                    {
                        Edge.channels.Clear();
                        foreach (int x in row.frequencySlots)
                            Edge.channels.Add(x);
                    }
                }
            }
        } */

        //zwraca wezel o okreslonym id
        public Node GetNode(int index)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if ( nodes[j].GetId() == index )
                {
                    return nodes[j];
                }
            }
            return null;
        }

        // zwraca link o wierzcholkach w okreslonych wezlach
        public Edge GetEdge(int id1, int id2)
        {
            for (int i = 0; i < this.edges.Count(); i++)
            {
                if ((this.edges[i].GetStart() == id1 && this.edges[i].GetEnd() == id2) || (this.edges[i].GetStart() == id2 && this.edges[i].GetEnd() == id1))
                { return this.edges[i]; }

            }
            Console.WriteLine("Nie istnieje link z takimi wierzcholkami!\n");
            return null;
        }

        public List<int> Dijkstra(int source, int destination, int necessarySlots)
        {
            int INF = 99999;
            Graph graphDijkstra = new Graph(0, 0);              // graf Dijkstry
            List<int> result = new List<int>();
            List<Edge> queue = new List<Edge>();                // kolejka
            List<int> road = new List<int>();                   // droga podanach w indeksach wezlow
            List<int> road_in_edges = new List<int>();          // droga podana w indeksach linkow
            List<int> prev = new List<int>();                   // lista poprzednikow
            List<int> cost = new List<int>(this.nodes.Count);   // lista kosztow sluzaca w algorytmie
            List<int> costs = new List<int>(this.nodes.Count);  // lista kosztow
            // id przypisywane wezlom zaczyna sie od 0
            //przypisanie kosztow i poprzednikow
            for (int i = 0; i < nodes.Count; i++)
            {
                prev.Add(-1);
                cost.Add(INF);
            }
            cost[0] = 0; // koszt pierwszego wezla = 0 
            costs[0] = 0; // koszt pierwszego wezla = 0
            // zbiory Q i S do algorytmu. Na poczatku wszystkie wezly w Q, a S jest pusty
            List<Node> S = new List<Node>();
            List<Node> Q = new List<Node>();
            for (int i = 0; i < this.nodes.Count(); i++)
            {
                Q.Add((GetNode(i)));
            }
            
            graphDijkstra.nodes.Add(nodes[source]);
            graphDijkstra.numberNodes++;

            //int previous = source;

            int smallest = INF;
            int present = 0; //id obecnie rozwazanego wezla
            int count = nodes.Count();  //licznik równy liczbie wezlow w grafie

            while (graphDijkstra.numberNodes != this.numberNodes)
            {
                int i;

                // szukamy wezla o najmniejszym koscie wsrod wszystkich jeszcze nie rozpatrzonych
                for (i = 0; i < cost.Count(); i++)
                {
                    if (cost[i] < smallest) { 
                        smallest = cost[i];
                        present = GetNode(i).GetId(); 
                    }
                }
                if (GetNode(present).GetId() == destination) // jesli rozwazany jest wezel docelowy to konczymy
                {
                    break;
                }

                count--;

                foreach (var neighbor in GetNode(present).neighbors)  // dla kazdego sasiada naszego rozwazanego wezla
                {
                    if (S.Contains(neighbor) == true) { continue; }

                    Edge toNeighbor = this.GetEdge(GetNode(present).GetId(), neighbor.GetId());
                    int alt = cost[GetNode(present).GetId()] + toNeighbor.GetWeight();   
                    if ( alt < cost[nodes[present].GetId()])
                    {
                        if (graphDijkstra.edges.Count() == 0)
                        {
                            if (toNeighbor.FindFreeChannels(necessarySlots) == true)
                            {
                                toNeighbor.useChannels(Edge.readyChannels);
                            }
                        }
                        else 
                        {
                            if (toNeighbor.AreSlotsFree(Edge.readyChannels) == true)
                            {
                                toNeighbor.useChannels(Edge.readyChannels);
                                cost[nodes[present].GetId()] = alt; //jesli alternetywny koszt dojscia do sasiada jest mniejszy od rozwazanego to zmieniamy jego koszt w tabeli
                                prev[neighbor.GetId()] = nodes[present].GetId();
                                graphDijkstra.nodes.Add(neighbor);
                                graphDijkstra.edges.Add(this.GetEdge(GetNode(present).GetId(), neighbor.GetId()));
                            }
                            else
                            {
                                this.edges.Remove(this.GetEdge(GetNode(present).GetId(), neighbor.GetId()));
                                GetNode(present).neighbors.Remove(neighbor);
                                break;
                            }
                        }
                    }
                }
                costs[present] = cost[present];
                cost[present] = INF;
            }






            


            return result;
        }


        /// <summary>
        /// sprawdzamy czy gotowe do zajecia szczeliny sa wolne na kazdym laczu
        /// </summary>
        /// <returns></returns>
        public bool AllLinksHaveTheseSlotsFree()
        {
            for(int i = 0; i< this.edges.Count(); i++)
            { 
                foreach(int index in Edge.readyChannels)
                {
                    if(edges[i].IsThisSlotFree(index) == false) { return false; }
                }
            }
            return true; ;
        }

        

    }
}
