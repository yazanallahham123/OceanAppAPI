using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class TransferBody
    {
        public Currency Currency { get; set; }
        public Branch FromBranch { get; set; }
        public Branch ToBranch { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public List<Item> Items { get; set; }

    }
}
