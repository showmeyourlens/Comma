using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class Subnetwork
    {
        public string emulationNodeId;
        public Domain domain;
        public RC rc;
        public CC cc;

        public Subnetwork(Domain domain, string id)
        {
            this.domain = domain;
            this.emulationNodeId = domain.emulationNodeId + "_" + id;
            this.cc = new CC(this);
            this.rc = new RC(this);
        }
       
    }
}
