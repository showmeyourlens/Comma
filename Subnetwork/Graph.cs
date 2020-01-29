using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{

    public class Graph
    {
        public List<Node> nodes = new List<Node>();
        public List<Edge> edges = new List<Edge>();

        List<Path> pathList = new List<Path>();

        public Graph(List<Edge> edges)
        {
            this.edges.AddRange(edges);

            foreach(Edge edge in edges)
            {
                if(this.nodes.Exists(x => String.Equals(x.id, edge.start)))
                {
                    if(!this.nodes.Find(x => String.Equals(x.id, edge.start)).neighbors.Exists(x => String.Equals(x.id, edge.end)))
                    {
                        this.nodes.Find(x => String.Equals(x.id, edge.start)).neighbors.Add(new Node(edge.end));
                    }
                }
                else
                {
                    this.nodes.Add(new Node(edge.start));
                    if (!this.nodes.Find(x => String.Equals(x.id, edge.start)).neighbors.Exists(x => String.Equals(x.id, edge.end)))
                    {
                        this.nodes.Find(x => String.Equals(x.id, edge.start)).neighbors.Add(new Node(edge.end));
                    }
                }
                if (this.nodes.Exists(x => String.Equals(x.id, edge.end)))
                {
                    if (!this.nodes.Find(x => String.Equals(x.id, edge.end)).neighbors.Exists(x => String.Equals(x.id, edge.start)))
                    {
                        this.nodes.Find(x => String.Equals(x.id, edge.end)).neighbors.Add(new Node(edge.start));
                    }
                }
                else
                {
                    this.nodes.Add(new Node(edge.end));
                    if (!this.nodes.Find(x => String.Equals(x.id, edge.end)).neighbors.Exists(x => String.Equals(x.id, edge.start)))
                    {
                        this.nodes.Find(x => String.Equals(x.id, edge.end)).neighbors.Add(new Node(edge.start));
                    }
                }
            }
        }

        // zwraca link o wierzcholkach w okreslonych wezlach
        public Edge GetEdge(string id1, string id2)
        {
            for (int i = 0; i < this.edges.Count(); i++)
            {
                if ((String.Equals(this.edges[i].start, id1) && String.Equals(edges[i].end, id2)) || (String.Equals(this.edges[i].start, id2) && String.Equals(this.edges[i].end, id1)))
                { return this.edges[i]; }

            }
            Console.WriteLine("Nie istnieje link z takimi wierzcholkami!\n");
            return null;
        }

        public void Dijkstra(string source, string destination, RCPath rcpath)
        {
            List<Path> dijkstraResult = new List<Path>();
            Path startingPath = new Path();
            startingPath.Nodes.Add(source);

            RecursiveDijkstra(dijkstraResult, startingPath, source, destination);

            // W dijkstraResults są wszystkie ścieżki które nie mają cykli i prowadzą z source do destination
            // Zajmowanie szczelin do ogarnięcia
            if (dijkstraResult.Count > 0)
            {

                Path best = dijkstraResult.OrderBy(x => x.Cost).First();


                best.Nodes.ForEach(x => rcpath.nodes.Add(x));
                for(int i=1; i<best.Nodes.Count; i++)
                {
                    rcpath.LRMids.Add(GetEdge(best.Nodes[i - 1], best.Nodes[i]).id);
                }

                rcpath.length = best.Cost;

                rcpath.status = ConnectionStatus.InProgress;

            }
            else
            {
                rcpath.status = ConnectionStatus.Rejected;
            }
        }

        public void RecursiveDijkstra(List<Path> dijkstraResult, Path pathToSource, string source, string destination)
        {
            Node startNode = nodes.Find(x => String.Equals(x.id, source));
            foreach (Node node in startNode.neighbors)
            {
                if (pathToSource.Nodes.Contains(node.id))
                {
                    // pętla. Ścieżka do wywalenia
                }
                else
                {
                    Path newPath = new Path(pathToSource);
                    newPath.AddNode(node.id, GetEdge(startNode.id, node.id).length);
                    if (node.id == destination)
                    {
                        dijkstraResult.Add(newPath);
                    }
                    else
                    {
                        RecursiveDijkstra(dijkstraResult, newPath, node.id, destination);
                    }
                }
            }
        }

    }

    public class Path
    {
        public List<string> Nodes;
        public int Cost;

        public Path()
        {
            Nodes = new List<string>();
            Cost = 0;
        }

        public Path(Path currentPath)
        {
            Nodes = new List<string>(currentPath.Nodes);
            Cost = currentPath.Cost;
        }

        public void AddNode(string nodeId, int cost)
        {
            Nodes.Add(nodeId);
            Cost += cost;
        }
    }
}
