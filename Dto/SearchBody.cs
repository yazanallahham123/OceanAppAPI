using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Dto
{
    public class SearchBody
    {
        public bool byComputerNo { get; set; }
        public string searchText { get; set; }
        public string? colorId { get; set; }
        public string? sizeId { get; set; }

        public string? branchId { get; set; }
    }
}
