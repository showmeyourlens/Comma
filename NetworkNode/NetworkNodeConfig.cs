using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace NetworkNode
{
    public class NetworkNodeConfig
    {
        public IPAddress SubnetworkAddress { get; set; }

        public ushort SubnetworkPort { get; set; }

        public string NodeName { get; set; }

        public IPAddress CloudAddress { get; set; }

        public IPAddress NodeAddress { get; set; }

        public ushort CloudPort { get; set; }

        public static NetworkNodeConfig ParseConfig(string FileName)
        {
            var content = File.ReadAllLines(FileName).ToList();
            var config = new NetworkNodeConfig();

            
            config.NodeName = GetProperty(content, "NODENAME");
            config.CloudAddress = IPAddress.Parse(GetProperty(content, "CLOUDADDRESS"));
            config.NodeAddress = IPAddress.Parse(GetProperty(content, "NODEADDRESS"));
            config.CloudPort = ushort.Parse(GetProperty(content, "CLOUDPORT"));
            config.SubnetworkAddress = IPAddress.Parse(GetProperty(content, "SUBNETWORKADDRESS"));
            config.SubnetworkPort = ushort.Parse(GetProperty(content, "SUBNETWORKPORT"));
            return config;
        }

        private static string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }
    }
}