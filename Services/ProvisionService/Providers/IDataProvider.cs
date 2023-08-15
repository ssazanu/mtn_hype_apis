using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProvisionService.Models;
using SharedLib.Models;

namespace ProvisionService.Providers
{
    public interface IDataProvider
    {
        Task AddBulkHookRequest(List<HookRequest> reqData);
        Task<List<IntegratorAccount>> GetIntegrators();
        Task SaveFailedTransaction(string requesterMsisdn, string beneficiaryMsisdn, long productId, string productName, string categoryName, double amount, string benefits, string paymentMode, double maBalance, string reqReferralCode, DateTime reqProductExpiry, string reqTransactionId, string externalTransactionId, string channel);
        Task SaveSuccessfulTransaction(long productId, double amount, string requesterMsisdn, string beneficiaryMsisdn, string productName, string categoryName, string paymentMode, string benefits, string smsResponseMessage, string ussdResponseMessage, bool reqAutoRenew, string reqReferralCode, DateTime reqProductExpiry, string reqTransactionId, bool autoRenewed, string externalTransactionId, string channel);
    }
}