using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CableCloud
{
    public class ReceiverState
    {
        public Socket WorkSocket { get; set; } = null;

        public const int BufferSize = 1024;
        public byte[] Buffer { get; set; } = new byte[BufferSize];        
    }
}
