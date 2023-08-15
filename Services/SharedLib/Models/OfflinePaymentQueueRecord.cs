using System;

namespace SharedLib.Models
{
    public class OfflinePaymentQueueRecord
    {
        public long QueueId { get; set; }
        public string TransactionId { get; set; }
        public string PayingMsisdn { get; set; }
        public int ProductId { get; set; }
        public string PaymentMode { get; set; }
        public string Channel { get; set; }
        public double Amount { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string BeneficiaryMsisdn { get; set; }
        public string RequesterMsisdn { get; set; }
        public string ReferralCode { get; set; }
    }
}
