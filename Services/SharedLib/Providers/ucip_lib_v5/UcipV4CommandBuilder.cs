using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ucip_lib_v5.models;

namespace ucip_lib_v5
{
    public class UcipV4CommandBuilder : IUcipV4CommandBuilder
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UcipCommandsV4 _ucipCommandsTemplate;
        private readonly string _dateFormat = "yyyyMMddTHH:mm:ss+0000";


        public UcipV4CommandBuilder(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _ucipCommandsTemplate = new UcipCommandsV4();
        }

        public async Task<CommandDetail> UpdateBalance_MA_NDA(string phoneNumber, double maAmount, List<DAUpdateInformation> daUpdateList, string originTransactionId, DateTime originTimestamp, string externalData1, string externalData2, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string updateBalance_MA_NDA = _ucipCommandsTemplate.UpdateBalance_MA_NDA;

            updateBalance_MA_NDA = updateBalance_MA_NDA.Replace("$$base64Credential$$", base64Credential)
                                                        .Replace("$$originHostName$$", originHostName)
                                                        .Replace("$$originNodeType$$", originNodeType)
                                                        .Replace("$$originTimeStamp$$", originTimestamp.ToString(_dateFormat))
                                                        .Replace("$$originTransactionID$$", originTransactionId.ToString())
                                                        .Replace("$$subscriberNumber$$", phoneNumber)
                                                        .Replace("$$externalData1$$", externalData1)
                                                        .Replace("$$externalData2$$", externalData2)
                                                        .Replace("$$MA_Adjustment_Relative$$", ((int)(maAmount * 100.0)).ToString());

            StringBuilder stringBuilder = new StringBuilder();
            foreach (DAUpdateInformation daUpdate in daUpdateList)
            {
                string updateBalance_DA_Structure = _ucipCommandsTemplate.UpdateBalance_DA_Structure;
                updateBalance_DA_Structure = updateBalance_DA_Structure.Replace("$$DA1_Adjustment_Relative$$", ((int)(daUpdate.DAValueCedis * 100.0)).ToString());
                updateBalance_DA_Structure = updateBalance_DA_Structure.Replace("$$DA1_ID$$", daUpdate.DAId.ToString());
                updateBalance_DA_Structure = updateBalance_DA_Structure.Replace("$$DA1_Expiry$$", daUpdate.ExpiryDate.ToString(_dateFormat));
                updateBalance_DA_Structure = updateBalance_DA_Structure.Replace("$$DA1_Account_Unit_Type$$", daUpdate.DedicatedAccountUnitType.ToString());
                stringBuilder.Append(updateBalance_DA_Structure);
            }
            updateBalance_MA_NDA = updateBalance_MA_NDA.Replace("$$DA_Structure$$", stringBuilder.ToString());

            string response = await airServerHelper.Execute(updateBalance_MA_NDA);
            return new CommandDetail { command = updateBalance_MA_NDA, response = response };
        }

        public async Task<CommandDetail> CreditBalanceMA(string phoneNumber, double maAmount, string originTransactionId, DateTime originTimestamp, string externalData1, string externalData2, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string creditMA = _ucipCommandsTemplate.CreditMA;

            creditMA = creditMA.Replace("$$base64Credential$$", base64Credential);
            creditMA = creditMA.Replace("$$originHostName$$", originHostName);
            creditMA = creditMA.Replace("$$originNodeType$$", originNodeType);
            creditMA = creditMA.Replace("$$originTimeStamp$$", originTimestamp.ToString("yyyyMMddTHH:mm:ss+0000"));
            creditMA = creditMA.Replace("$$originTransactionID$$", originTransactionId.ToString());
            creditMA = creditMA.Replace("$$subscriberNumber$$", phoneNumber);
            creditMA = creditMA.Replace("$$externalData1$$", externalData1);
            creditMA = creditMA.Replace("$$externalData2$$", externalData2);
            creditMA = creditMA.Replace("$$MA_Adjustment_Relative$$", ((int)(maAmount * 100.0)).ToString());

            string response = await airServerHelper.Execute(creditMA);
            return new CommandDetail { command = creditMA, response = response };
        }

        public async Task<CommandDetail> GetBalanceAndDate(string phoneNumber, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string getBalanceAndDate = _ucipCommandsTemplate.GetBalanceAndDate
                                                            .Replace("$$base64Credential$$", base64Credential)
                                                            .Replace("$$originHostName$$", originHostName)
                                                            .Replace("$$originNodeType$$", originNodeType)
                                                            .Replace("$$originTimeStamp$$", originTimestamp.ToString("yyyyMMddTHH:mm:ss+0000"))
                                                            .Replace("$$originTransactionID$$", originTransactionId.ToString())
                                                            .Replace("$$subscriberNumber$$", phoneNumber);

            string text = await airServerHelper.Execute(getBalanceAndDate);
            return new CommandDetail { command = getBalanceAndDate, response = text };
        }

        public async Task<CommandDetail> UpdateOffer(string phoneNumber, int offerId, DateTime expiryDate, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string updateOffer = _ucipCommandsTemplate.UpdateOffer;

            updateOffer = updateOffer.Replace("$$base64Credential$$", base64Credential);
            updateOffer = updateOffer.Replace("$$originHostName$$", originHostName);
            updateOffer = updateOffer.Replace("$$originNodeType$$", originNodeType);
            updateOffer = updateOffer.Replace("$$originTimeStamp$$", originTimestamp.ToString("yyyyMMddTHH:mm:ss+0000"));
            updateOffer = updateOffer.Replace("$$originTransactionID$$", originTransactionId.ToString());
            updateOffer = updateOffer.Replace("$$subscriberNumber$$", phoneNumber);
            updateOffer = updateOffer.Replace("$$offerID$$", offerId.ToString());
            updateOffer = updateOffer.Replace("$$expiryDate$$", expiryDate.ToString("yyyyMMddTHH:mm:ss+0000"));

            string response = await airServerHelper.Execute(updateOffer);
            return new CommandDetail { command = updateOffer, response = response };
        }

        public async Task<CommandDetail> UpdateAccumulators(string phoneNumber, int accumulatorId, int accumulatorValue, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string updateAccumulators = _ucipCommandsTemplate.UpdateAccumulators;

            updateAccumulators = updateAccumulators.Replace("$$base64Credential$$", base64Credential);
            updateAccumulators = updateAccumulators.Replace("$$originHostName$$", originHostName);
            updateAccumulators = updateAccumulators.Replace("$$originNodeType$$", originNodeType);
            updateAccumulators = updateAccumulators.Replace("$$originTimeStamp$$", originTimestamp.ToString("yyyyMMddTHH:mm:ss+0000"));
            updateAccumulators = updateAccumulators.Replace("$$originTransactionID$$", originTransactionId.ToString());
            updateAccumulators = updateAccumulators.Replace("$$subscriberNumber$$", phoneNumber);
            updateAccumulators = updateAccumulators.Replace("$$accumulatorID$$", accumulatorId.ToString());
            updateAccumulators = updateAccumulators.Replace("$$accumulatorValue$$", accumulatorValue.ToString());

            string response = await airServerHelper.Execute(updateAccumulators.Substring(updateAccumulators.IndexOf('.') + 1));
            return new CommandDetail { command = updateAccumulators, response = response };
        }

        public async Task<CommandDetail> AddPeriodicAccountManagementData(string phoneNumber, int pamClassId, int pamServiceId, int scheduleId, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string addPeriodicAccountManagementData = _ucipCommandsTemplate.AddPeriodicAccountManagementData;

            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$base64Credential$$", base64Credential);
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$originHostName$$", originHostName);
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$originNodeType$$", originNodeType);
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$originTimeStamp$$", originTimestamp.ToString("yyyyMMddTHH:mm:ss+0000"));
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$originTransactionID$$", originTransactionId.ToString());
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$subscriberNumber$$", phoneNumber);
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$pamClassID$$", pamClassId.ToString());
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$pamServiceID$$", pamServiceId.ToString());
            addPeriodicAccountManagementData = addPeriodicAccountManagementData.Replace("$$scheduleID$$", scheduleId.ToString());

            string response = await airServerHelper.Execute(addPeriodicAccountManagementData);
            return new CommandDetail { command = addPeriodicAccountManagementData, response = response };

        }

        public async Task<CommandDetail> RunPeriodicAccountManagement(string phoneNumber, int pamIndicator, int pamServiceId, string originTransactionId, DateTime originTimestamp, string originHostName, string originNodeType, string airServerIp, int airServerPort, string base64Credential, string airServerProtocol)
        {
            AirServerHelper airServerHelper = new AirServerHelper(airServerProtocol ?? "http", airServerIp, airServerPort, "/Air", base64Credential, _httpClientFactory);
            string runPeriodicAccountManagement = _ucipCommandsTemplate.RunPeriodicAccountManagement;

            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$base64Credential$$", base64Credential);
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$originHostName$$", originHostName);
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$originNodeType$$", originNodeType);
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$originTimeStamp$$", originTimestamp.ToString("yyyyMMddTHH:mm:ss+0000"));
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$originTransactionID$$", originTransactionId.ToString());
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$subscriberNumber$$", phoneNumber);
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$pamIndicator$$", pamIndicator.ToString());
            runPeriodicAccountManagement = runPeriodicAccountManagement.Replace("$$pamServiceID$$", pamServiceId.ToString());


            string response = await airServerHelper.Execute(runPeriodicAccountManagement);
            return new CommandDetail { command = runPeriodicAccountManagement, response = response };

        }
    }
}
