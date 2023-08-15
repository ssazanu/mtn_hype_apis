using System;
using System.Collections.Generic;

namespace ucip_lib_v5.models
{
    public class DaInfoWithOffer
    {
        public int DAId { get; set; }
        public int DedicatedAccountUnitType { get; set; }
        public double DAValueCedis { get; set; }
        public double Erm { get; set; }
        public DateTime ExpiryDate { get; set; }
        public List<OfferInformation> Offers { get; set; }
    }
}
