using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class NasuniFiler
    {
        public String Name{ get; set; }
        public String Guid { get; set; }
        public String Serialno { get; set; }
        public String Build { get; set; }
        public Boolean Offline { get; set; }
        public Double PercentUsed { get; set; }
        public BasicInformation BasicInfo { get; set; }
        public CIFSSettings CifsInfo { get; set; }
        public Status FilerStatus { get; set; }
        public NasuniFiler()
        {
            Name = "";
            Guid = "";
            Serialno = "";
            Build = "";
            Offline = true;
            PercentUsed = 0.0;
            BasicInfo = new BasicInformation();
            CifsInfo = new CIFSSettings();
            FilerStatus = new Status();
        }
    }
}
