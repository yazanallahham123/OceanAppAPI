using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class TransferMaster
    {
        public string Blno { get; set; }
        public string FromBranchName { get; set; }
        public int TotalQty { get; set; }
        public DateTime Date { get; set; }
    }
}
