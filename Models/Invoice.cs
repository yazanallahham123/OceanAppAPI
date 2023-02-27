using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class Invoice
    {
        public string Blno { get; set; }
        public int TotalQty { get; set; }
        public double FinalValue { get; set; }
        public DateTime Date { get; set; }
    }
}
