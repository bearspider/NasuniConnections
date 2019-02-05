using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    class CifsLockInfo
    {
        public String Client { get; set; }
        public String Share { get; set; }
        public String Type { get; set; }
        public String FilePath { get; set; }

        public CifsLockInfo()
        {
            Client = "";
            Share = "";
            Type = "";
            FilePath = "";
        }
    }
}
