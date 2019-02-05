using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class ADSite
    {
        public String Sitename { get; set; }
        public List<String> Subnets { get; set; }
        public ADSite()
        {
            Sitename = "";
            Subnets = new List<String>();
        }
    }
}
