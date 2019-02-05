using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    public class CIFSSettings
    {
        public String SMB { get; set; }
        public String VetoFiles { get; set; }
        public String AioSupport { get; set; }
        public String SmbEncrypt { get; set; }
        public String FruitSupport { get; set; }
        public String DenyAccess { get; set; }
        public String ProtoLevel { get; set; }
        public String UnixExt { get; set; }
        public String Anonymous { get; set; }
        public CIFSSettings ()
        {
            SMB = "";
            VetoFiles = "";
            AioSupport = "";
            SmbEncrypt = "";
            FruitSupport = "";
            DenyAccess = "";
            ProtoLevel = "";
            UnixExt = "";
            Anonymous = "";
        }
    }
}
