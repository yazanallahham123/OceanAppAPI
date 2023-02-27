using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class Item
    {
        public string Code { get; set; }
        public string ComputerNo { get; set; }
        public string Price { get; set; }
        public string Cost { get; set; }
        public string Qty { get; set; }
        public string Barcode { get; set; }
        public string Branch { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }

        public string Sale { get; set; }
        public string BranchId { get; set; }

    }
}
