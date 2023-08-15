using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ucip_lib_v5.models;

namespace ucip_lib_v5
{
    public interface IUcipV4CommandBuilder
    {
        Task<CommandDetail> GetBalanceAndDate(string phoneNumber, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
        Task<CommandDetail> UpdateBalance_MA_NDA(string phoneNumber, double maAmount, List<DAUpdateInformation> daUpdateList, string originTransactionId, DateTime originTimestamp, string externalData1, string externalData2, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
        public Task<CommandDetail> UpdateOffer(string phoneNumber, int offerId, DateTime expiryDate, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
        public Task<CommandDetail> UpdateAccumulators(string phoneNumber, int accumulatorId, int accumulatorValue, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
        public Task<CommandDetail> AddPeriodicAccountManagementData(string phoneNumber, int pamClassId, int pamServiceId, int scheduleId, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
        public Task<CommandDetail> RunPeriodicAccountManagement(string phoneNumber, int pamIndicator, int pamServiceId, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
        Task<CommandDetail> CreditBalanceMA(string phoneNumber, double maAmount, string originTransactionId, DateTime originTimestamp, string externalData1, string externalData2, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol);
    }
}