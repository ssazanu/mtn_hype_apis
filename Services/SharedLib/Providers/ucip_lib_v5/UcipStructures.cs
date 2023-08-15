namespace SharedLib.Providers.ucip_lib_v5
{
    public class UcipStructures
    {
        public static readonly UcipStructures Instance = new UcipStructures();

        private UcipStructures()
        {
        }

        public readonly string DedicatedAccountInformationFieldName = "dedicatedAccountInformation";

        public readonly string OfferInformationFieldName = "offerInformation";

        public readonly string AccumulatorInformationFieldName = "accumulatorInformation";

        public readonly string UsageCounterUsageThresholdInformationFieldName = "usageCounterUsageThresholdInformation";

        public readonly string UsageThresholdInformationFieldName = "usageCounterUsageThresholdInformation";

        private readonly string[][] _dedicatedAccountStructure = new string[4][]
        {
        new string[2] { "dedicatedAccountID", "<i4>" },
        new string[2] { "dedicatedAccountValue1", "<string>" },
        new string[2] { "expiryDate", "<dateTime.iso8601>" },
        new string[2] { "startDate", "<dateTime.iso8601>" }
        };

        private readonly string[][] _offerStructure = new string[3][]
        {
        new string[2] { "offerID", "<i4>" },
        new string[2] { "endDate", "<dateTime.iso8601>" },
        new string[2] { "startDate", "<dateTime.iso8601>" }
        };

        private readonly string[][] _accumulatorStructure = new string[4][]
        {
        new string[2] { "accumulatorID", "<i4>" },
        new string[2] { "accumulatorValue", "<i4>" },
        new string[2] { "endDate", "<dateTime.iso8601>" },
        new string[2] { "startDate", "<dateTime.iso8601>" }
        };

        private readonly string[][] _UsageCounterStructure = new string[5][]
        {
        new string[2] { "usageCounterID", "<i4>" },
        new string[2] { "usageCounterMonetaryValue1", "<string>" },
        new string[2] { "usageCounterValue", "<string>" },
        new string[2] { "associatedPartyID", "<string>" },
        new string[2] { "usageThresholdInformation", "<struct>" }
        };

        private readonly string[][] _UsageThresholdStructure = new string[3][]
        {
        new string[2] { "usageThresholdID", "<i4>" },
        new string[2] { "usageThresholdMonetaryValue1", "<string>" },
        new string[2] { "usageThresholdSource", "<i4>" }
        };

        public string[][] DedicatedAccountStructure => _dedicatedAccountStructure;

        public string[][] OfferStructure => _offerStructure;

        public string[][] AccumulatorStructure => _accumulatorStructure;

        public string[][] UsageCounterStructure => _UsageCounterStructure;

        public string[][] UsageThresholdStructure => _UsageThresholdStructure;

    }

}
