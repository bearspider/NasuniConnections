using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class Status
    {
        public String Offline { get; set; }
        public String Version { get; set; }
        public String Platform { get; set; }
        public String CacheSize { get; set; }
        public String CacheUsed { get; set; }
        public String CacheFree { get; set; }
        public String PercentFree { get; set; }
        public String CpuCores { get; set;}
        public String CpuModel { get; set; }
        public String CpuFreq { get; set; }
        public String Sockets { get; set; }
        public String Memory { get; set; }
        public String UpdateAvail { get; set; }
        public String CurrentVersion { get; set;}
        public String NewVersion { get; set; }
        public String Uptime { get; set; }

        public Status()
        {
            Offline = "";
            Version = "";
            Platform = "";
            CacheSize = "";
            CacheUsed = "";
            CacheFree = "";
            PercentFree = "";
            CpuCores = "";
            CpuModel = "";
            CpuFreq = "";
            Sockets = "";
            Memory = "";
            UpdateAvail = "";
            CurrentVersion = "";
            NewVersion = "";
            Uptime = "";
        }
    }
}
