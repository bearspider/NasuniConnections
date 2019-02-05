using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class NasuniCIFS
    {
        public String Share { get; set; }
        public String User { get; set; }
        public String Client { get; set; }
        public String Workstation { get; set; }
        public String ConnectedSite { get; set; }
        public NasuniCIFS()
        {
            Share = "";
            User = "";
            Client = "";
            Workstation = "";
            ConnectedSite = "";
        }
    }
}
