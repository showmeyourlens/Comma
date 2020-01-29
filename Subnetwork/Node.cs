using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class Node
    {
        /// <summary>
        /// ID wezla
        /// </summary>
        public string id;

        public List<Node> neighbors = new List<Node>();

        public Node(string id)
        {
            this.id = id;
        }

    }
}
