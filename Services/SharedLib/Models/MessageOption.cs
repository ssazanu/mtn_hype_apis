using helpers;
using System.Collections.Generic;

namespace SharedLib.Models
{

    public class MessageOption
    {
        public MessageOption()
        {
            BonusExpiry = "1";
        }
        public bool BuyForOthers { get; set; }
        public double Amount { get; set; }
        public string SenderMsisdn { get; set; }
        public string SenderMsisdnFormatted { get { return Utility.FormatNumber10Digits(SenderMsisdn); } }
        public string BeneficiayMsisdn { get; set; }
        public string BeneficiayMsisdnFormatted { get { return Utility.FormatNumber10Digits(BeneficiayMsisdn); } }
        public string Benefits { get; set; }
        public string BenefitsBonus { get; set; }
        public string BonusExpiry { get; set; }
        public string Template { get; set; }
        public List<Notificaition> Templates { get; set; }
        public bool AutoRenewed { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }
        public List<KeyValueConfiguration>? Configurations { get; set; }
    }
}
