using System;
namespace ProvisionService.Models
{
	public class ProvisionRequest
    {
        public string Msisdn { get; set; }
        public int Product { get; set; }
        public bool ChargeSubscriber { get; set; }
        public string PayingMsisdn { get; set; }
        public string ClientTransactionId { get; set; }
        public bool IsPromo { get; set; }
        public bool IsJ4U { get; set; }
        public Payment Payment { get; set; }
        public string BeneficiaryId { get; set; }
    }

    public class Payment
    {
        public string Channel { get; set; }
        public string Currency { get; set; }
    }
}