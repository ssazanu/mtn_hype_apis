using SharedLib.Models;
using System.Collections.Generic;

namespace ucip_lib_v5.models
{
    public class PamInfo
    {
        public int ClassId { get; set; }
        public int ServiceId { get; set; }
        public int ScheduleId { get; set; }
        public int Indicator { get; internal set; }
        public List<Offer> Offers { get; internal set; }
    }
}
