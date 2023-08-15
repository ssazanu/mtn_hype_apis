namespace SharedLib.Models
{
    public class Offer
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long DaInfoId { get; set; }
        public int OfferId { get; set; }
        public int ValidityDays { get; set; }
        public bool Active { get; set; }
        public int OfferType { get; set; }
    }

}
