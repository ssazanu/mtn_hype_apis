using Serilog;
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Providers.ucip_lib_v5;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ucip_lib_v5.models;

namespace SharedLib.Providers.ProvisionProvider
{
    public class CommandResponseProcessor : ICommandResponseProcessor
    {

        private const string UNABLE_TO_COMPLETE_REQUEST_MESSAGE = "Sorry, Unable to complete request.";
        private const string INSUFFICIENT_BALANCE_MESSAGE = "You balance is not sufficient to purchase this package.";


        private readonly string[] insufficientBalanceCodes = new string[] { "124" };
        public CommandResponseProcessor()
        {

        }

        public AccountBalance GetBalanceInfoFromProcessingResponse(CommandDetail responsePayload, string[] successResponseCodes, string msisdn, string airServerIP)
        {
            if (responsePayload == null || string.IsNullOrWhiteSpace(responsePayload.response))
                throw new WarningException("Unable to get subscriber balance");

            var methodResponseString = responsePayload.response.Replace(@"<?xmlversion=""1.0""encoding=""utf-8""?>", @"<?xml version=""1.0"" encoding=""utf-8""?>");

            var responseCode = UcipResponseHelper.Instance.ExtractResponseCode(methodResponseString);
            Log.Information($"Response Code => GetBalance => {msisdn}: {responseCode}\n{responsePayload.command}\n{responsePayload.response}");

            if (successResponseCodes.Any(i => i == responseCode))
                return ExtractBalanceModel(methodResponseString);

            return null;
        }

        public ProcessResponse GetCommandProcessingResponse(CommandDetail responsePayload, string[] successResponseCodes, string msisdn, string airServerIP)
        {
            if (responsePayload == null || string.IsNullOrEmpty(responsePayload.response))
            {
                Log.Information($"Command: {responsePayload?.command}");
                Log.Information($"Response: empty");

                return new ProcessResponse { IsSuccessful = false };
            }
            var methodResponseString = responsePayload.response.Replace(@"<?xmlversion=""1.0""encoding=""utf-8""?>", @"<?xml version=""1.0"" encoding=""utf-8""?>");

            var responseCode = UcipResponseHelper.Instance.ExtractResponseCode(methodResponseString);
            Log.Information($"Command: {responsePayload.command}");
            Log.Information($"CommandResponse: {responsePayload.response}");
            Log.Information($"Response Code => {msisdn}: {responseCode}");

            int fatalResponseCode = -1;
            int.TryParse(responseCode, out fatalResponseCode);

            var processingResponse = new ProcessResponse
            {
                IsSuccessful = successResponseCodes.Any(i => i == responseCode),
                ResponseCode = responseCode,
            };

            if (insufficientBalanceCodes.Any(code => code == responseCode)) processingResponse.ResponseMessage = INSUFFICIENT_BALANCE_MESSAGE;
            else if (!processingResponse.IsSuccessful) processingResponse.ResponseMessage = UNABLE_TO_COMPLETE_REQUEST_MESSAGE;

            return processingResponse;
        }


        private AccountBalance ExtractBalanceModel(string response)
        {
            List<string[][]> daStructures = new List<string[][]> { };
            if (response.Contains(UcipStructures.Instance.DedicatedAccountInformationFieldName))
                daStructures = UcipResponseHelper.Instance.ExtractAllNested(response, UcipStructures.Instance.DedicatedAccountInformationFieldName,
                                                                                UcipStructures.Instance.DedicatedAccountStructure);

            return new AccountBalance
            {
                MainAccountBalance = ExtractMaBalance(response),
                DABalances = FormatDaInformation(daStructures),
                Offers = FormatOfferInformation(response),
                ServiceClass = Convert.ToInt32(UcipResponseHelper.Instance.Extract(response, "serviceClassCurrent", "<i4>"))
            };
        }
        private double ExtractMaBalance(string response)
        {
            return Convert.ToDouble(UcipResponseHelper.Instance.Extract(response, "accountValue1", "<string>")) / 100;
        }

        private List<DABalance> FormatDaInformation(List<string[][]> daStructures)
        {
            List<DABalance> balances = new List<DABalance>();

            foreach (var daItems in daStructures)
            {
                var daIdElement = daItems?.FirstOrDefault(element => element[0] == "dedicatedAccountID");
                if (daIdElement != null)
                {
                    var daId = Convert.ToInt32(daIdElement[1]);
                    var daIdValueElement = daItems.FirstOrDefault(element => element[0] == "dedicatedAccountValue1");
                    var daValue = Convert.ToDouble(daIdValueElement[1]);
                    var daIdExpiryDateElement1 = daItems.FirstOrDefault(element => element[0] == "expiryDate");
                    var daIdExpiryDateElement2 = daItems.FirstOrDefault(element => element[0] == "expiryDateTime");
                    DateTime daExpiryDate = ExtractDate(daIdExpiryDateElement1 ?? daIdExpiryDateElement2);

                    balances.Add(new DABalance
                    {
                        DAId = daId,
                        Balance = daValue / 100,
                        ExpiryDate = daExpiryDate
                    });

                }
            }

            return balances.GroupBy(x => x.DAId).Select(p => new DABalance
            {
                DAId = p.Key,
                Balance = p.Sum(x => x.Balance),
                ExpiryDate = p.Max(x => x.ExpiryDate),
            }).ToList();
        }

        private List<CustomerOfferInfo> FormatOfferInformation(string response)
        {
            List<CustomerOfferInfo> accountOffers = new List<CustomerOfferInfo>();
            if (!response.Contains("offerInformationList")) return accountOffers;

            var offerStructures = UcipResponseHelper.Instance.ExtractAllNested(response, "offerInformationList", UcipStructures.Instance.OfferStructure);

            foreach (var offerItems in offerStructures)
            {
                var offerElement = offerItems.FirstOrDefault(element => element[0] == "offerID");
                if (offerElement != null)
                {
                    var offerId = Convert.ToInt32(offerElement[1]);
                    var startDateElement1 = offerItems.FirstOrDefault(element => element[0] == "startDate");
                    var startDateElement2 = offerItems.FirstOrDefault(element => element[0] == "startDateTime");
                    var offerTypeValue = offerItems.FirstOrDefault(element => element[0] == "offerType");
                    var offerType = Convert.ToInt32(offerTypeValue);
                    var startDate = ExtractDate(startDateElement1 ?? startDateElement2);
                    var expiryDateElement1 = offerItems.FirstOrDefault(element => element[0] == "expiryDate");
                    var expiryDateElement2 = offerItems.FirstOrDefault(element => element[0] == "expiryDateTime");
                    var expiryDate = ExtractDate(expiryDateElement1 ?? expiryDateElement2);

                    accountOffers.Add(new CustomerOfferInfo
                    {
                        OfferId = offerId,
                        StartDate = startDate,
                        ExpiryDate = expiryDate,
                        OfferType = offerType
                    });
                }
            }

            return accountOffers;
        }

        private DateTime ExtractDate(string[] dateElement)
        {
            try
            {
                if (dateElement == null) return DateTime.MinValue;
                var expiryString = dateElement[1].Split('+')[0];

                return new DateTime(
                        Convert.ToInt32(expiryString.Substring(0, 4)),
                        Convert.ToInt32(expiryString.Substring(4, 2)),
                        Convert.ToInt32(expiryString.Substring(6, 2))
                        );
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

    }
}
