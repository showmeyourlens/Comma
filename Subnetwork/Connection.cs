using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class Connection
    {
        public int id;
        public int confirmationsNeeded;
        public ConnectionStatus status;

        public Connection(int id, int confirmationsNeeded)
        {
            this.id = id;
            this.confirmationsNeeded = confirmationsNeeded;
            this.status = ConnectionStatus.InProgress;
        }
    }

    public enum ConnectionStatus
    {
        InProgress,
        Connected,
        Disconnected
    }
}
