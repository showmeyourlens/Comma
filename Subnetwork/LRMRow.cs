using System;
using System.Collections.Generic;
using System.Text;

namespace Subnetwork
{
    class LRMRow
    {
        public int nodeID1;
        public int nodeID2;
        public SortedSet<int> frequencySlots;
        public LRMRow(int routerID1, int routerID2, int MAXSLOTS)
        {
            frequencySlots = new SortedSet<int>();
            this.nodeID1 = routerID1;
            this.nodeID2 = routerID2;
            for (int i = 0; i < MAXSLOTS; i++)
            {
                frequencySlots.Add(i);
            }
        }
    }
}
