using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace NetworkCallController
{
    class Configuration
    {
        public string NetworkCallControlerName { get; set; }

        public IPAddress NetworkCallControlerAddress { get; set; }

        public ushort NetworkCallControlerPort { get; set; }

        public static Configuration ParseConfig (string FileName)
        {

            List<string> content = File.ReadAllLines(FileName).ToList();
            var config = new Configuration();
            config.NetworkCallControlerName = GetProperty(content, "NETWORKCALLCONTROLERNAME");
            config.NetworkCallControlerAddress = IPAddress.Parse(GetProperty(content, "NETWORKCALLCONTROLERADDRESS"));
            config.NetworkCallControlerPort = ushort.Parse(GetProperty(content, "NETWORKCALLCONTROLERPORT"));
            return config;
        }

        private static string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }
    }
}
