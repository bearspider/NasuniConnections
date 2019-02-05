using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class BasicInformation
    {
        public String Build { get; set; }
        public String Description { get; set; }
        public String Guid { get; set; }
        public String MgmtState { get; set; }
        public String SerialNo { get; set; }
        public String Timezone { get; set; }
        public BasicInformation()
        {
            Build = "";
            Description = "";
            Guid = "";
            MgmtState = "";
            SerialNo = "";
            Timezone = "";
        }
    }
}
