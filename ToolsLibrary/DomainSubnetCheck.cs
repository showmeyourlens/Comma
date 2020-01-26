using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsLibrary
{
    public static class DomainSubnetCheck
    {
        public static bool IsSubnet(string contact)
        {
            string[] split = contact.Split('_');
            if (split.Length == 2)
            {
                if (String.Equals(split[0], "A") || String.Equals(split[0], "B"))
                {
                    if (String.Equals(split[1], "1") || String.Equals(split[1], "2"))
                    {

                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsDomain(string contact)
        {
            if (String.Equals(contact, "A") || String.Equals(contact, "B"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
