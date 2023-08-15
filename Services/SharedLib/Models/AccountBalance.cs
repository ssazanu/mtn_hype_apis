using System.Collections.Generic;

namespace SharedLib.Models
{
    public class AccountBalance
    {
        public double MainAccountBalance { get; set; }
        public int ServiceClass { get; set; }
        public List<DABalance> DABalances { get; set; }
        public List<CustomerOfferInfo> Offers { get; set; }
    }
}
