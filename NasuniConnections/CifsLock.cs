using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasuniConnections
{
    class CifsLock
    {
        public ObservableCollection<CifsLockInfo> CifsLockList { get; set; }
        public String Username { get; set; }
        public CifsLock()
        {
            CifsLockList = new ObservableCollection<CifsLockInfo>();
            Username = "";
        }
    }
}
