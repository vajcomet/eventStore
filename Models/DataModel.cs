using System;

namespace eventStoreASP.Models
{
    public class DataModel
    {
        public string id { get; set; } = "0";
        public DateTime time { get; set; } = DateTime.Now;
        public string type { get; set; } = "none";
        public string value { get; set; } = "none";
    }
}
