using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class Edge
    {   // ID lacza
        public int id;
        // pierwszy wierzcholek
        public string start;
        // drugi wierzcholek
        public string end;

        public bool isDirect;

        public int startPort;

        public int endPort;

        // waga lacza
        public int length;
     

        public Edge(int id, string begin, string end, int length)
        {
            this.start = begin;
            this.end = end;
            this.id = id;
            this.length = length;

        }

        public string GetStart()
        {
            return start;
        }

        public string GetEnd()
        {
            return end;
        }
     

    }
}
