using System;
using System.Collections.Generic;

namespace LUISSample.ViewModel
{
    public class ReservationInformation
    {
        public string Room { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string Size { get; set; }
        public string Day { get; set; }
        public string UserId { get; set; }
        public IEnumerable<String> Sizes = new string[] { "Big", "Medium", "Small" };
    }
}