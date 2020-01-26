using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class RCContact
    {
        public string contactName;
        public string subjectToAsk;

        public RCContact(string contactName, string subjectToAsk)
        {
            this.contactName = contactName;
            this.subjectToAsk = subjectToAsk;
        }
    }
}
