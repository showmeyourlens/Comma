using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsLibrary;

namespace Subnetwork
{
    public class Connection
    {
        public int id;
        public int confirmationsNeeded;
        public string from;
        public string to;
        public int length;
        public int bandwidth;
        public List<int> busyCracks;
        public ConnectionStatus status;
        public List<string> nodesInConnection;

        public Connection(int id, int confirmationsNeeded)
        {
            this.id = id;
            this.confirmationsNeeded = confirmationsNeeded;
            this.status = ConnectionStatus.InProgress;
            nodesInConnection = new List<string>();
            busyCracks = new List<int>();
        }

        public static string WriteConnection(Connection connection)
        {
            return String.Format("From {0} to {1}, ID: {2}, status: {3}", connection.from, connection.to, connection.id, connection.status.ToString());
        }

        public static string ChangeStatus(Connection connection, ConnectionStatus status)
        {
            string result = String.Format("Connection {0}: status changed from: {1} to: {2} ", connection.id, connection.status.ToString(), status.ToString());
            connection.status = status;
            return result;
        }

        public static string StringifyNodes(Connection connection)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string node in connection.nodesInConnection)
            {
                if (sb.Length != 0)
                {
                    sb.Append(" ");
                }
                sb.Append(node);
            }
            return sb.ToString();
        }

        public void ParseBusyCracks(string message)
        {
            if (message != null && message != " " && message != "")
            {
                string[] split = message.Split(' ');
                for (int i = 0; i < split.Length; i++)
                {
                    this.busyCracks.Add(Int32.Parse(split[0]));
                }
                Console.WriteLine(TimeStamp.TAB + " Updated connection " + this.id + " with " + split.Length + " busy slots");
            }
        }
    }

    public enum ConnectionStatus
    {
        InProgress,
        Connected,
        Disconnected,
        Rejected
    }
}
