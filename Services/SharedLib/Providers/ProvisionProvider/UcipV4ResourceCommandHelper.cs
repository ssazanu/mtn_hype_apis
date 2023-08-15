using helpers;
using Serilog;
using SharedLib.Interfaces;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ucip_lib_v5;
using ucip_lib_v5.models;

namespace SharedLib.Providers.ProvisionProvider
{
    public class UcipV4ResourceCommandHelper : IUcipV4ResourceCommandHelper
    {
        private readonly UcipMocks _ucipMocks;
        private readonly IAirServerManager _airServerManager;
        private readonly ICommandResponseProcessor _responseProcessor;
        private readonly IUcipV4CommandBuilder _ucipV4CommandBuilder;
        private readonly string[] successCodes = new string[] { "0" };
        public UcipV4ResourceCommandHelper(UcipMocks ucipMocks, IAirServerManager airServerManager, ICommandResponseProcessor responseProcessor, IUcipV4CommandBuilder ucipV4CommandBuilder)
        {
            _ucipMocks = ucipMocks;
            _airServerManager = airServerManager;
            _responseProcessor = responseProcessor;
            _ucipV4CommandBuilder = ucipV4CommandBuilder;
        }

        public async Task<ProcessResponse> RunCommands(string requesterMsisdn, string msisdn, double maAmount, double packageAmount, ProvisionResourceItems resourceItems, string transactionId, ExternalData externalData)
        {
            // Run Das
            Log.Information($"DaInfoWithOffers[{requesterMsisdn}|{msisdn}]: {resourceItems.Stringify()}");

            var airServer = _airServerManager.GetRandomActiveAir();
            var daResponse = await RunDaInfoUpdate(airServer, msisdn, transactionId, externalData, maAmount, resourceItems);
            if (daResponse != null && daResponse.IsSuccessful)
            {
                // Run Offers
                if (resourceItems.DaInfoWithOffers.Any(_ => _.Offers != null && _.Offers.Any()))
                    await RunAttachOffer(airServer, msisdn, transactionId, resourceItems);

                // Run PAM
                if (resourceItems.PamInfo != null && resourceItems.PamInfo.Any())
                    await AssignAndRunPam(airServer, msisdn, transactionId, resourceItems);

                // Run Accumulators for beneficiary
                if (resourceItems.Accumulators != null && resourceItems.Accumulators.Any(x => x.ForBeneficiary))
                    _ = RunAccumulators(airServer, msisdn, packageAmount, transactionId, resourceItems);

                // Run Accumulators for requester
                if (resourceItems.Accumulators != null && requesterMsisdn != msisdn && resourceItems.Accumulators.Any(x => x.ForRequester))
                    _ = RunAccumulators(airServer, requesterMsisdn, packageAmount, transactionId, resourceItems);
            }
            return daResponse;
        }

        public async Task<AccountBalance> GetSubcriberInfo(string msisdn, string transactionId)
        {
            var airServer = _airServerManager.GetRandomActiveAir();
            Log.Information($"AIRSERVER: {airServer.Stringify()}");
            
            var response = await _ucipV4CommandBuilder.GetBalanceAndDate(
                    msisdn,
                    transactionId,
                    DateTime.Now,
                    airServer.DefaultOriginHostName,
                    airServer.DefaultOriginNodeType,
                    airServer.IPAddress,
                    airServer.Port,
                    airServer.Base64Credential,
                    airServer.Protocol
                );

            return _responseProcessor.GetBalanceInfoFromProcessingResponse(
                            new CommandDetail { command = response.command, response = response.response }
                            , successCodes, msisdn, airServer.IPAddress);/**/
        }

        private async Task<ProcessResponse> RunDaInfoUpdate(AirServer airServer, string msisdn, string transactionId, ExternalData externalData, double maAmount, ProvisionResourceItems resourceItems)
        {
            List<DAUpdateInformation> daInfos = resourceItems
                                                 .DaInfoWithOffers.Select(_ => new DAUpdateInformation { DAId = _.DAId, DAValueCedis = _.DAValueCedis, ExpiryDate = _.ExpiryDate }).ToList();

            var response = await _ucipV4CommandBuilder.UpdateBalance_MA_NDA(msisdn, maAmount, daInfos,
                      transactionId,
                      DateTime.Now,
                      externalData == null ? airServer.DefaultExternalData1 : externalData.ExternalData1,
                      externalData == null ? airServer.DefaultExternalData2 : externalData.ExternalData2,
                      externalData == null ? airServer.DefaultOriginHostName : externalData.OriginHostName,
                      externalData == null ? airServer.DefaultOriginNodeType : externalData.OriginNodeType,
                      airServer.IPAddress,
                      airServer.Port,
                      airServer.Base64Credential,
                      airServer.Protocol
                  );
            return _responseProcessor.GetCommandProcessingResponse(
                new CommandDetail { command = response.command, response = response.response },
                successCodes, msisdn, airServer.IPAddress);
        }

        public async Task<ProcessResponse> UpdateMaBalance(string msisdn, string transactionId, ExternalData externalData, double maAmount)
        {

            var airServer = _airServerManager.GetRandomActiveAir();
            Log.Information($"AIRSERVER: {airServer.Stringify()}");

            var response = await _ucipV4CommandBuilder.CreditBalanceMA(msisdn, maAmount,
                      transactionId,
                      DateTime.Now,
                      externalData == null ? airServer.DefaultExternalData1 : externalData.ExternalData1,
                      externalData == null ? airServer.DefaultExternalData2 : externalData.ExternalData2,
                      externalData == null ? airServer.DefaultOriginHostName : externalData.OriginHostName,
                      externalData == null ? airServer.DefaultOriginNodeType : externalData.OriginNodeType,
                      airServer.IPAddress,
                      airServer.Port,
                      airServer.Base64Credential,
                      airServer.Protocol
                  );
            return _responseProcessor.GetCommandProcessingResponse(
                new CommandDetail { command = response.command, response = response.response },
                successCodes, msisdn, airServer.IPAddress);
        }
        private async Task RunAttachOffer(AirServer airServer, string msisdn, string transactionId, ProvisionResourceItems resourceItems)
        {
            List<OfferInformation> daOfferInfos = new List<OfferInformation>();
            if (resourceItems.DaInfoWithOffers != null && resourceItems.DaInfoWithOffers.Any(x => x.Offers != null))
                daOfferInfos = resourceItems?.DaInfoWithOffers
                                    ?.Where(x => x.Offers != null && x.Offers.Any())
                                    ?.SelectMany(_ => _?.Offers)?.ToList();

            if (daOfferInfos != null && daOfferInfos.Any())
            {
                for (int i = 0; i < daOfferInfos.Count; i++)
                {
                    var response = await _ucipV4CommandBuilder.UpdateOffer(
                                                    msisdn,
                                                    daOfferInfos[i].OfferId,
                                                    daOfferInfos[i].Expiry,
                                                    transactionId,
                                                    DateTime.Now,
                                                    airServer.DefaultOriginHostName,
                                                    airServer.DefaultOriginNodeType,
                                                    airServer.IPAddress,
                                                    airServer.Port,
                                                    airServer.Base64Credential,
                                                    airServer.Protocol
                                                );
                    _responseProcessor.GetCommandProcessingResponse(
                        new CommandDetail { command = response.command, response = response.response },
                        successCodes, msisdn, airServer.IPAddress);


                }
            }



        }

        private async Task RunAccumulators(AirServer airServer, string msisdn, double amount, string transactionId, ProvisionResourceItems resourceItems)
        {
            for (int i = 0; i < resourceItems?.Accumulators?.Count; i++)
            {
                var accumulatorValue = resourceItems.Accumulators[i].StaticValue <= 0 ? (int)(resourceItems.Accumulators[i].Rate * amount) : (int)resourceItems.Accumulators[i].StaticValue;

                try
                {
                    var responsePayload = await _ucipV4CommandBuilder.UpdateAccumulators(
                           msisdn,
                           resourceItems.Accumulators[i].AccumulatorId,
                           accumulatorValue,
                           transactionId,
                           DateTime.Now,
                           airServer.DefaultOriginHostName,
                           airServer.DefaultOriginNodeType,
                           airServer.IPAddress,
                           airServer.Port,
                           airServer.Base64Credential,
                           airServer.Protocol
                       );
                    _responseProcessor.GetCommandProcessingResponse(
                        new CommandDetail { command = responsePayload.command, response = responsePayload.response },
                        successCodes, msisdn, airServer.IPAddress);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                    Log.Warning($"Accumulator: msisdn:{msisdn}, accumulatorValue: {accumulatorValue}, amount: {amount}, rate:{resourceItems.Accumulators[i].Rate}, static val: {resourceItems.Accumulators[i].StaticValue}");
                }

            }
        }
        private async Task AssignAndRunPam(AirServer airServer, string msisdn, string transactionId, ProvisionResourceItems resourceItems)
        {
            //var account = await GetSubcriberInfo(msisdn);

            List<OfferInformation> daOfferInfos = new List<OfferInformation>();
            if (resourceItems.DaInfoWithOffers != null && resourceItems.DaInfoWithOffers.Any(x => x.Offers != null))
                daOfferInfos = resourceItems?.DaInfoWithOffers?.Where(x => x.Offers != null && x.Offers.Any())?.SelectMany(_ => _?.Offers)?.ToList();

            List<PamInfo> pamInfos = resourceItems?.PamInfo;

            if (pamInfos != null && pamInfos.Any())
            {
                //var nonExistingPams = pamInfos.Where(x => x.Offers.Any(o => account.Offers != null && account.Offers.Any(c => c.OfferId != o.OfferId)))?.ToList();
                //Log.Information($"Add PAM for {msisdn} => {nonExistingPams.Stringify()}");

                for (int i = 0; i < pamInfos.Count; i++)
                {
                    /*#if DEBUG
                                        _responseProcessor.GetCommandProcessingResponse(_ucipMocks.GetMock("SuccessResponse", "AddPeriodicAccountManagementData"), successCodes, msisdn, airServer.IPAddress);
                                        continue;
                    #endif*/
                    var response = await _ucipV4CommandBuilder.AddPeriodicAccountManagementData(
                                       msisdn,
                                       pamInfos[i].ClassId,
                                       pamInfos[i].ServiceId,
                                       pamInfos[i].ScheduleId,
                                       transactionId,
                                       DateTime.Now,
                                       airServer.DefaultOriginHostName,
                                       airServer.DefaultOriginNodeType,
                                       airServer.IPAddress,
                                       airServer.Port,
                                       airServer.Base64Credential,
                                       airServer.Protocol
                                   );
                    _responseProcessor.GetCommandProcessingResponse(
                        new CommandDetail { command = response.command, response = response.response },
                        successCodes, msisdn, airServer.IPAddress);

                    await RunPam(airServer, pamInfos[i].Indicator, pamInfos[i].ServiceId, msisdn, transactionId);
                }

            }

        }

        private async Task RunPam(AirServer airServer, int indicator, int serviceId, string msisdn, string transactionId)
        {
            /*#if DEBUG
                        _responseProcessor.GetCommandProcessingResponse(_ucipMocks.GetMock("SuccessResponse", "RunPeriodicAccountManagement"), successCodes, msisdn, airServer.IPAddress);
                        return;
            #endif*/
            var response = await _ucipV4CommandBuilder.RunPeriodicAccountManagement(
                                                    msisdn,
                                                    indicator,
                                                    serviceId,
                                                    transactionId,
                                                    DateTime.Now,
                                                    airServer.DefaultOriginHostName,
                                                    airServer.DefaultOriginNodeType,
                                                    airServer.IPAddress,
                                                    airServer.Port,
                                                    airServer.Base64Credential,
                                                    airServer.Protocol
                                                );
            _responseProcessor.GetCommandProcessingResponse(
                new CommandDetail { command = response.command, response = response.response },
                successCodes, msisdn, airServer.IPAddress);
        }


    }
}
