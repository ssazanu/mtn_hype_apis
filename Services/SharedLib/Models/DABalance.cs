using System;

namespace SharedLib.Models
{
    public class DABalance
    {
        public int DAId { get; set; }
        public double Balance { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}