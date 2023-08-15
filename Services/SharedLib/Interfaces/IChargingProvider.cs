using SharedLib.Models;
using System.Threading.Tasks;

namespace SharedLib.Interfaces
{
    public interface IChargingProvider
    {
        Task<AccountBalance> GetBalance(string msisdn, string transactionId);
        Task<ProcessResponse> ProvisionResources(string transactionId, string requesterMsisdn, string payingMsisdn, string paymentMode, string beneficiaryMsisdn, double amount, Product product, Category category, string referralCode, bool isExternalPayment, bool isRenewal, bool chargeSubscriber);
    }
}