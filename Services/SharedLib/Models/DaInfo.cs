using System.Collections.Generic;
using ucip_lib_v5.models;

namespace SharedLib.Models
{
    public class DaInfo
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Description { get; set; }
        public int DaId { get; set; }
        public int ValidityDays { get; set; }
        public int AccountUnitType { get; set; }
        public double ERM { get; set; }
        public string Unit { get; set; }
        public double? StaticValue { get; set; }
        public bool Active { get; set; }
        public List<Offer> Offers { get; set; }
        public bool DisplayDecimals { get; set; }
        public bool Visible { get; set; }
        public bool DisplayDescription { get; set; }
        public List<PamInfo> PamInfos { get; set; }
    }
}
