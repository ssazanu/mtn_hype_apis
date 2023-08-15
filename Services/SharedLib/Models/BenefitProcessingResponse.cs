namespace SharedLib.Models
{
    public class BenefitProcessingResponse
    {
        public int DAId { get; set; }
        public double AmountInUnits { get; set; }
        public string Unit { get; set; }
        public int ValidityDays { get; set; }
        public string Description { get; set; }
        public int AccountUnitType { get; set; }
        public bool Visible { get; set; }
        public bool DisplayDecimals { get; set; }
        public bool DisplayDescription { get; set; }
    }
}

