using System.Collections.Generic;
using ucip_lib_v5.models;

namespace SharedLib.Models
{
    public class ProvisionResourceItems
    {
        public List<DaInfoWithOffer> DaInfoWithOffers { get; set; }
        public List<PamInfo> PamInfo { get; set; }
        public List<Accumulator> Accumulators { get; set; }
    }
}
