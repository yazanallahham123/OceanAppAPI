using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class InvoiceDetail
    {
        public string ComputerNo { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Barcode { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
    }
}
