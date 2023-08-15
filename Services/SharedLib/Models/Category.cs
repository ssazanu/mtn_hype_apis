using System.Collections.Generic;

namespace SharedLib.Models
{
    public class Category
    {
        public long ConfigId { get; set; }
        public string CategoryId { get; set; }
        public double MinAmount { get; set; }
        public double MaxAmount { get; set; }
        public bool Active { get; set; }
        public bool IsSpecial { get; set; }
        public List<Product> Products { get; set; }
        public string CategoryName { get; set; }
        public string Notification { get; set; }
    }
}
