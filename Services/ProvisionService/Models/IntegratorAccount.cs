using SharedLib.Models;
using System.Collections.Generic;
using models;

namespace ProvisionService.Models
{
    public class IntegratorAccount : Integrator
    {
        public List<PaymentChannels>? PaymentChannels { get; set; }
        public List<KeyValueConfiguration>? Configurations { get; set; }
    }
    public class PaymentChannels
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string VerificationApi { get; set; }
        public bool DisableVerification { get; set; }
        public string Channels { get; set; }
    }
}

