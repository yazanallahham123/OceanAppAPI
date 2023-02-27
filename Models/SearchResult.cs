using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class SearchResult
    {
        public List<Item> Items { get; set; }
        public List<ItemColor> Colors { get; set; }
        public List<ItemSize> Sizes { get; set; }
        public List<Branch> Branches { get; set; }
    }
}
