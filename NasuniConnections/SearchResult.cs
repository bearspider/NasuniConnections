using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class SearchResult
    {
        public String Share { get; set; }
        public String User { get; set; }
        public String Client { get; set; }
        public String ClientName { get; set; }
        public String ADSite { get; set; }
        public String Filer { get; set; }
        public SearchResult()
        {
            Share = "";
            User = "";
            Client = "";
            ClientName = "";
            ADSite = "";
            Filer = "";
        }
    }
}
