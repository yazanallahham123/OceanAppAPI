using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OceanApp.Models
{
    public class DeviceSettings
    {
        public string DeviceId { get; set; }
        public string BranchId { get; set; }
        public bool CanShowSales { get; set; }
        public bool CanMakeTransfers { get; set; }
        public bool CanReceiveTransfers { get; set; }        
        public string UserId { get; set; }
        public string Username { get; set; }
        public List<string> BranchesInSearch { get; set; }
        public List<string> BranchesToTransferTo { get; set; }
        public bool IsAdmin { get; set; }
        public string Fullname { get; set; }
    }
}
