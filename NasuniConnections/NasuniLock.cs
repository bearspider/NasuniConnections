using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    class NasuniLock
    {
        public String Share { get; set; }
        public String FilePath { get; set; }
        public String Type { get; set; }
        public String User { get; set; }
        public String Client { get; set; }
        public String ClientName { get; set; }
        public NasuniLock()
        {
            Share = "";
            FilePath = "";
            Type = "";
            User = "";
            Client = "";
            ClientName = "";
        }

    }
}
