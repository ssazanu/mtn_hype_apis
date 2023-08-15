namespace SharedLib.Models
{
    public class Accumulator
    {
        public string Id { get; set; }
        public int AccumulatorId { get; set; }
        public double Rate { get; set; }
        public double StaticValue { get; set; }
        public bool ForRequester { get; set; }
        public bool ForBeneficiary { get; set; }
    }
}
