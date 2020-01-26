using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsLibrary
{
    public enum Command
    {
        Client_To_Client,
        HELLO,
        Call_Request,
        Call_Indication,
        Call_Indication_Confirmed,
        Connection_Request,
        Set_Path,
        Path_Set,
        Set_OXC,
        OXC_Set,
        Connection_Confirmed,
        Call_Confirmed
    }
}
