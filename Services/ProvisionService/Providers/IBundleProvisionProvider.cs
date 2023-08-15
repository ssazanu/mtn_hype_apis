using System.Threading.Tasks;
using models;
using ProvisionService.Models;

namespace ProvisionService.Providers
{
    public interface IBundleProvisionProvider
	{
		Task<ApiResponse<Data>> BundleProvision(
			string requesterMsisdn, string beneficiaryMsisdn, long productId,
            string currency, string paymentChannel, string paymentTransactionId,
			IntegratorAccount integratorAccount, string channel, bool chargeSubscriber);
	}
}

