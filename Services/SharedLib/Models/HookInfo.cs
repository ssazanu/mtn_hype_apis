using System;

namespace SharedLib.Models
{
    public class HookInfo
    {
        public long Id { get; set; }
        public string Url { get; set; }
    }

    public class HookRequest
    {
        public long HookId { get; set; }
        public string TransactionId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public double Amount { get; set; }
        public string RequesterMsisdn { get; set; }
        public string BeneficiaryMsisdn { get; set; }
        public string PaymentMode { get; set; }
        public string Benefits { get; set; }
        public SmsContents SmsMessages { get; set; }
        public string ReferralCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ExternalTransactionId { get; set; }
        public string Channel { get; set; }
        public DateTime ProductExpiry { get; set; }
    }
}
