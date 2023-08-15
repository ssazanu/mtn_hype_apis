using SharedLib.Models;
using System.Threading.Tasks;

namespace SharedLib.Interfaces
{
    public interface IUcipV4ResourceCommandHelper
    {
        Task<AccountBalance> GetSubcriberInfo(string msisdn, string transactionId);
        Task<ProcessResponse> RunCommands(string requesterMsisdn, string msisdn, double maAmount, double packageAmount, ProvisionResourceItems resourceItems, string transactionId, ExternalData externalData);
        Task<ProcessResponse> UpdateMaBalance(string msisdn, string transactionId, ExternalData externalData, double maAmount);
    }
}