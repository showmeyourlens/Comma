using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Node
    {
        /// <summary>
        /// ID wezla
        /// </summary>
        public int id;
        /// <summary>
        /// wspolrzedne wierzcholka
        /// </summary>
        private int x;
        private int y;
        /// <summary>
        /// zmienna okreslajaca czy wezel jest podsiecia
        /// </summary>
        public bool isSubnetwork;
        /// <summary>
        /// lista sasiadow wezla
        /// </summary>
        public List<Node> neighbors = new List<Node>();

        public Node()
        {
            this.x = 0;
            this.y = 0;
            this.id = -1;
            this.isSubnetwork = false;
        }

        public Node(int nr, int a, int b)
        {
            this.x = 0;
            this.y = 0;
            this.id = nr;
            this.isSubnetwork = false;
        }

        public Node(int nr)
        {
            this.id = nr;
            this.isSubnetwork = false;
        }

        public int GetId()
        {
            return id;
        }


    }
}
