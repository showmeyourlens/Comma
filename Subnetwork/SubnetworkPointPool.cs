using System;
using System.Collections.Generic;
using System.Text;

namespace Subnetwork
{
    class SubnetworkPointPool
    {
        public List<SubnetworkPoint> snps;

        public SubnetworkPointPool()
        {
            snps = new List<SubnetworkPoint>();
        }

        /// <summary>
        /// Tworzy liste SNP i dodaje do niej pojedynczy SNP
        /// </summary>
        /// <param name="point"></param>
        public SubnetworkPointPool(SubnetworkPoint point) : this()
        {
            snps.Add(point);
        }

        /// <summary>
        /// Dodaje SubNetworkPoint do listy
        /// </summary>
        /// <param name="SNP"></param>
        public void Add(SubnetworkPoint SNP)
        {
            snps.Add(SNP);
        }

    }
}
