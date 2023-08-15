using SharedLib.Models;
using System.Threading.Tasks;

namespace SharedLib.Interfaces
{
    public interface IUcipHelper
    {
        Task<AccountBalance> GetSubcriberInfo(string msisdn, string transactionId);
        Task<ProcessResponse> RunProvisionResourcesCommands(string requesterMsisdn, string msisdn, double maAmount, double packageAmount, ProvisionResourceItems resourceItems, string transactionId, ExternalData externalData);
        Task<ProcessResponse> UpdateMaBalance(string msisdn, double maAmount, string transactionId, ExternalData externalData);
    }
}