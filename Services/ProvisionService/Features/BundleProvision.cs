using System.Threading.Tasks;
using helpers;
using helpers.Atttibutes;
using helpers.Interfaces;
using helpers.Engine;
using Microsoft.Extensions.Configuration;
using models;
using ProvisionService.Models;
using ProvisionService.Providers;
using System.Linq;

namespace ProvisionService.Features
{
    [Feature(Name = "BundleProvision")]
    public class BundleProvision : BaseServiceFeature
	{
        private readonly string _channel;
        private readonly IIntegratorStorage _integratorStorage;
        private readonly IBundleProvisionProvider _bundleProvisionProvider;

		public BundleProvision(IConfiguration configuration, IIntegratorStorage integratorStorage, IBundleProvisionProvider bundleProvisionProvider)
		{
            _channel = configuration.GetValue<string>("Channel");
            _integratorStorage = integratorStorage;
            _bundleProvisionProvider = bundleProvisionProvider;
        }

        [Entry(Method = "POST")]
        public async Task<ProvisionResponse> Entry([FromJsonBody] ProvisionRequest payload)
        {
            var validationResult = ValidatePayload(payload);
            if (validationResult != null) return new ProvisionResponse { Message = validationResult };

            var integrators = await _integratorStorage.GetIntegrators();
            if (integrators == null) return new ProvisionResponse { Message = "Internal server error."};

            var integrator = integrators.FirstOrDefault(x => x.IntegratorCode == _channel);
            if (integrator == null) return new ProvisionResponse { Message = "Channel not allowed." };

            IntegratorAccount integratorAccount = integrator.Data as IntegratorAccount;

            var result = await _bundleProvisionProvider.BundleProvision(payload.PayingMsisdn, payload.Msisdn,
                payload.Product, payload.Payment.Currency, payload.Payment.Channel,
                payload.ClientTransactionId, integratorAccount, _channel, payload.ChargeSubscriber);

            return new ProvisionResponse
            {
                Success = result.Success,
                Message = result.ResponseMessage,
                Data = result.Data,
                TransactionId = result?.Data?.Product?.TransactionId
            };
        }

        private string ValidatePayload(ProvisionRequest payload)
        {
            if (string.IsNullOrWhiteSpace(payload.Msisdn)) return "Msisdn is required";
            if (string.IsNullOrWhiteSpace(payload.PayingMsisdn)) return "Paying Msisdn is required";

            if (payload.Payment == null) return "Payment details are required";
            if (payload.Payment.Channel == null) return "Payment Channel is required";
            if (payload.Payment.Currency == null) return "Payment Currency is required";

            if (payload.Product <= 0) return "A valid product id is required";

            return null;
        }
    }
}

