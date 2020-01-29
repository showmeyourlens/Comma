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
        Call_Request_Request,
        Call_Accept_Request,
        Call_Accept_Confirmed,
        Connection_Request,
        Set_Path,
        Path_Set,
        Set_OXC,
        OXC_Set,
        Connection_Confirmed,
        Call_Confirmed,
        Find_Path,
        Path_Found,
        Find_Path_Params,
        Path_Params_Found,
        Used_Slots_Request,
        Used_Cracks_Response,
        Link_Connection_Request,
        Link_Connection_Response,
        Slots_Allocated,
        Break_The_Link,
        Link_Down,
        Check_Link_Request,
        Check_Link_Response,
        Remove_Link_Connection_Request,
        Remove_Link_Connection_Response,
        Restore_Path_Request,
        Restore_Path_Response


    }
}
