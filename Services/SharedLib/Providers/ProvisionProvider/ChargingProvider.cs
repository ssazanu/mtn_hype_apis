using SharedLib.Interfaces;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ucip_lib_v5.models;

namespace SharedLib.Providers.ProvisionProvider
{
    public class ChargingProvider : IChargingProvider
    {
        private const string BENEFICIARY_AIRTIME_PAY = "BENEFICIARY_AIRTIME_PAY";
        private const string INITIATOR_AIRTIME_PAY = "INITIATOR_AIRTIME_PAY";
        private const string AIRTIME_PAYMENT_KEY = "AIRTIME";
        private readonly IUcipHelper _ucipHelper;
        private readonly IExternalDataManager _externalDataManager;

        public IExternalDataManager ExternalDataManager => _externalDataManager;

        public ChargingProvider(IUcipHelper ucipHelper, IExternalDataManager externalDataManager)
        {
            _ucipHelper = ucipHelper;
            _externalDataManager = externalDataManager;
        }

        public async Task<AccountBalance> GetBalance(string msisdn, string transactionId) => await _ucipHelper.GetSubcriberInfo(msisdn, transactionId);

        public async Task<ProcessResponse> ProvisionResources(string transactionId, string requesterMsisdn, string payingMsisdn, string paymentMode, string beneficiaryMsisdn,
            double amount, Product product, Category category, string referralCode, bool isExternalPayment, bool isRenewal, bool chargeSubscriber)
        {
            ExternalData payingExternalData = ExternalDataManager.GetExternalDataForProduct(category.CategoryId, -amount, paymentMode, payingMsisdn, beneficiaryMsisdn, product.ProductType, referralCode, isRenewal);
            ExternalData payingExternalDataRefund = ExternalDataManager.GetExternalDataForProduct(category.CategoryId, amount, paymentMode, payingMsisdn, beneficiaryMsisdn, product.ProductType, referralCode, isRenewal);
            ExternalData beneficiaryExternalData = ExternalDataManager.GetExternalDataForProduct(category.CategoryId, amount, paymentMode, payingMsisdn, beneficiaryMsisdn, product.ProductType, referralCode, isRenewal);

            //Deduct payingMsisd 
            var resourceItems = new ProvisionResourceItems
            {
                DaInfoWithOffers = new List<DaInfoWithOffer> { },
                PamInfo = new List<PamInfo> { },
                Accumulators = new List<Accumulator> { }
            };
            bool isDeducted = false;
            double deductionAmount = -amount;
            double packageAmount = amount;
            ProcessResponse deductionResponse = new ProcessResponse { };

            if (isExternalPayment)
            {
                deductionAmount = 0;
                deductionResponse.IsSuccessful = true;
            }
            //else if (paymentMode == AIRTIME_PAYMENT_KEY && payingMsisdn != beneficiaryMsisdn)
            else if (chargeSubscriber)
            {
                deductionResponse = await _ucipHelper.RunProvisionResourcesCommands(requesterMsisdn, payingMsisdn, -amount, packageAmount, resourceItems, transactionId, payingExternalData);
                isDeducted = deductionResponse.IsSuccessful;
                deductionAmount = 0;
            }
            else if (paymentMode == AIRTIME_PAYMENT_KEY && payingMsisdn == beneficiaryMsisdn)
            {
                deductionResponse.IsSuccessful = true;
            }


            if (deductionResponse == null) return new ProcessResponse { IsSuccessful = false, ResponseMessage = "Request failed. Please try again later" };
            if (deductionResponse.IsSuccessful)
            {
                resourceItems.DaInfoWithOffers = ProvisioningHelper.PrepareDAUpdateInformation(product.DaInfos, amount);
                //Credit beneficiary
                if (product.PamInfos?.Count > 0) resourceItems.PamInfo?.AddRange(product.PamInfos?.GroupBy(x => x.ClassId).Select(y => y?.First()));

                if (product.DaInfos != null && product.DaInfos?.Count > 0 && product.DaInfos.Any(_ => _.PamInfos != null && _.PamInfos.Count > 0))
                {
                    var daPamInfos = product.DaInfos?.SelectMany(_ => _?.PamInfos)?.ToList();
                    resourceItems.PamInfo.AddRange(daPamInfos.GroupBy(x => x.ClassId).Select(y => y.First()));
                }

                if (product.Accumulators != null && product.Accumulators?.Count > 0)
                    resourceItems.Accumulators.AddRange(product.Accumulators);


                resourceItems.DaInfoWithOffers.ForEach(x =>
                {
                    if (x.Offers != null && x.Offers.Any())
                        x.Offers = x.Offers.GroupBy(x => x.OfferId).Select(y => y.First()).ToList();
                });

                ProcessResponse creditResponse = await _ucipHelper.RunProvisionResourcesCommands(requesterMsisdn, beneficiaryMsisdn, deductionAmount, packageAmount, resourceItems, transactionId, beneficiaryExternalData);

                if (!creditResponse.IsSuccessful)
                {
                    //Refund paying Msisdn
                    if (isDeducted && (new string[] { INITIATOR_AIRTIME_PAY, BENEFICIARY_AIRTIME_PAY, AIRTIME_PAYMENT_KEY }).Contains(paymentMode.ToUpper()))
                        await _ucipHelper.RunProvisionResourcesCommands(requesterMsisdn, payingMsisdn, amount, packageAmount, new ProvisionResourceItems { }, transactionId, payingExternalDataRefund);

                    return new ProcessResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Unable to complete request. Please try again later"
                    };
                }

                return new ProcessResponse { IsSuccessful = true };
            }
            return new ProcessResponse
            {
                IsSuccessful = false,
                ResponseMessage = !string.IsNullOrWhiteSpace(deductionResponse.ResponseMessage) ? deductionResponse.ResponseMessage : "Bundle purchase failed. Please try again later",
            };
        }


    }
}
