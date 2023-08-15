using System.Collections.Generic;
using ucip_lib_v5.models;

namespace SharedLib.Models
{
    public class Product
    {
        public long Id { get; set; }
        public string CategoryId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public List<Notificaition> Notification { get; set; }
        public double MinAmount { get; set; }
        public double MaxAmount { get; set; }
        public List<DaInfo> DaInfos { get; set; }
        public List<PamInfo> PamInfos { get; set; }
        public List<Accumulator> Accumulators { get; set; }
        public List<HookInfo> Hooks { get; set; }
        public string AllowedServiceClass { get; set; }
        public int UssdSuccessMenuItem { get; set; }
    }
}
