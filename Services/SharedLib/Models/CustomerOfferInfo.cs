using System;

namespace SharedLib.Models
{
    public class CustomerOfferInfo
    {
        public int OfferId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime StartDate { get; set; }
        public int OfferType { get; set; }
    }
}
