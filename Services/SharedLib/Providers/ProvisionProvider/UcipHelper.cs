using helpers;
using Microsoft.Extensions.Configuration;
using SharedLib.Interfaces;
using SharedLib.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using ucip_lib_v5;

namespace SharedLib.Providers.ProvisionProvider
{
    public class UcipHelper : IUcipHelper
    {
        private readonly IAirServerManager _airServerManager;
        private readonly IUcipCommand _ucipCommand;
        private readonly IUcipV4ResourceCommandHelper _ucipV4ResourceCommandHelper;
        private readonly ICommandResponseProcessor _responseProcessor;
        private readonly string _ucipVersion = "v5";

        private readonly string[] updateBalanceSuccessCodes = new string[] { "0", "1", "2" };
        private readonly string[] updateOfferSuccessCodes = new string[] { "0", "1", "2" };
        private readonly string[] accountBlockedResponseCodes = new string[] { "104" };
        private readonly string[] balanceSuccessCodes = new string[] { "0" };

        public UcipHelper(IAirServerManager airServerManager, IUcipCommand ucipCommand, IConfiguration config,
            IUcipV4ResourceCommandHelper ucipV4ResourceCommandHelper, ICommandResponseProcessor responseProcessor)
        {
            _airServerManager = airServerManager;
            _ucipCommand = ucipCommand;
            _ucipV4ResourceCommandHelper = ucipV4ResourceCommandHelper;
            _responseProcessor = responseProcessor;
            _ucipVersion = config["UCIP_VERSION"] ?? "v5";
        }
        public async Task<AccountBalance> GetSubcriberInfo(string msisdn, string transactionId)
        {
            if (_ucipVersion == "v4") return await _ucipV4ResourceCommandHelper.GetSubcriberInfo(msisdn, transactionId);

            var airServer = _airServerManager.GetRandomActiveAir();
            var commandBuilder = _ucipCommand.CreateBuilder(airServer);
            commandBuilder
                .SetMsisdn(Utility.FormatNumber9Digits(msisdn))
                .SetOriginInfo("101", airServer.DefaultOriginHostName, airServer.DefaultOriginNodeType, DateTime.UtcNow)
                ;
            var responsePayload = await commandBuilder.GeneralGet();


            return _responseProcessor.GetBalanceInfoFromProcessingResponse(responsePayload, balanceSuccessCodes, msisdn, airServer.IPAddress);

        }

        public async Task<ProcessResponse> RunProvisionResourcesCommands(string requesterMsisdn, string msisdn, double maAmount, double packageAmount, ProvisionResourceItems resourceItems, string transactionId, ExternalData externalData)
        {
            if (_ucipVersion == "v4") return await _ucipV4ResourceCommandHelper.RunCommands(requesterMsisdn, msisdn, maAmount, packageAmount, resourceItems, transactionId, externalData);

            var airServer = _airServerManager.GetRandomActiveAir();
            var commandBuilder = _ucipCommand.CreateBuilder(airServer);
            commandBuilder
               .SetMsisdnAndAdjustment(Utility.FormatNumber12Digits(msisdn), maAmount)
               .SetOriginInfo(
                    $"{transactionId}",
                    externalData == null ? airServer.DefaultOriginHostName : externalData.OriginHostName,
                    externalData == null ? airServer.DefaultOriginNodeType : externalData.OriginNodeType,
                    DateTime.UtcNow
                )
               .SetExternalDataInfo(
                    externalData == null ? airServer.DefaultExternalData1 : externalData.ExternalData1,
                    externalData == null ? airServer.DefaultExternalData2 : externalData.ExternalData2
                );

            if (resourceItems.PamInfo.Any()) commandBuilder.SetPamInfos(resourceItems.PamInfo);
            if (resourceItems.PamInfo.Any()) commandBuilder.SetDaInfo(resourceItems.DaInfoWithOffers);

            var responsePayload = await commandBuilder.GeneralUpdate();



            return _responseProcessor.GetCommandProcessingResponse(responsePayload, balanceSuccessCodes, msisdn, airServer.IPAddress);

        }

        public async Task<ProcessResponse> UpdateMaBalance(string msisdn, double maAmount, string transactionId, ExternalData externalData)
        {
            if (_ucipVersion == "v4") return await _ucipV4ResourceCommandHelper.UpdateMaBalance(msisdn, transactionId, externalData, maAmount);

            var airServer = _airServerManager.GetRandomActiveAir();
            var commandBuilder = _ucipCommand.CreateBuilder(airServer);
            commandBuilder
               .SetMsisdnAndAdjustment(Utility.FormatNumber12Digits(msisdn), maAmount)
               .SetOriginInfo(
                    $"{transactionId}",
                    externalData == null ? airServer.DefaultOriginHostName : externalData.OriginHostName,
                    externalData == null ? airServer.DefaultOriginNodeType : externalData.OriginNodeType,
                    DateTime.UtcNow
                )
               .SetExternalDataInfo(
                    externalData == null ? airServer.DefaultExternalData1 : externalData.ExternalData1,
                    externalData == null ? airServer.DefaultExternalData2 : externalData.ExternalData2
                );

            var responsePayload = await commandBuilder.GeneralUpdate();



            return _responseProcessor.GetCommandProcessingResponse(responsePayload, balanceSuccessCodes, msisdn, airServer.IPAddress);

        }

    }
}
