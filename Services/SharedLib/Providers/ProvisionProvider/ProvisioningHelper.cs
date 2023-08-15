using helpers;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ucip_lib_v5.models;

namespace SharedLib.Providers.ProvisionProvider
{
    public static class ProvisioningHelper
    {
        private const string AMOUNT_FORMAT = "#,##0";
        private const string AMOUNT_FORMAT_WITH_DECIMAL = "#,##0.00";
        private static readonly string[] _binary_prefix = { "MB", "GB", "TB" };
        public static List<DaInfoWithOffer> PrepareDAUpdateInformation(List<DaInfo> dedicatedAccountInfos, double amountInCedi)
        {
            List<DaInfoWithOffer> dAUpdateInfos = new List<DaInfoWithOffer>();
            foreach (var da in dedicatedAccountInfos)
            {
                DaInfoWithOffer daUpdateInformation = ExtractDAInfoForProductAmount(amountInCedi, da);
                dAUpdateInfos.Add(daUpdateInformation);
            }
            return dAUpdateInfos;
        }

        private static DaInfoWithOffer ExtractDAInfoForProductAmount(double amountInCedi, DaInfo da)
        {
            double benefitInCedis = (double)(da.StaticValue > 0 ? da.StaticValue * da.ERM : (1 / da.ERM) * da.ERM * amountInCedi);
            var daUpdateInformation = new DaInfoWithOffer
            {
                DAId = da.DaId,
                DedicatedAccountUnitType = da.AccountUnitType,
                DAValueCedis = Math.Round(benefitInCedis, 2, MidpointRounding.AwayFromZero),
                ExpiryDate = DateTime.UtcNow.AddDays(da.ValidityDays),
                Erm = da.ERM
            };

            if (da.Offers != null && da.Offers.Any())
            {
                daUpdateInformation.Offers = new List<OfferInformation> { };
                for (int i = 0; i < da.Offers.Count; i++)
                {
                    daUpdateInformation.Offers.Add(new OfferInformation
                    {
                        OfferId = da.Offers[i].OfferId,
                        OfferType = da.Offers[i].OfferType,
                        Expiry = DateTime.UtcNow.AddDays(da.Offers[i].ValidityDays)
                    });
                }
            }
            return daUpdateInformation;
        }

        public static string SerializeBenefits(List<BenefitProcessingResponse> productBenefits)
        {
            List<string> serializedStrings = new List<string>();
            string separator = ",";
            foreach (var da in productBenefits)
            {
                if (da.Visible)
                {
                    if (da.Unit == "MB")
                    {
                        serializedStrings.Add($"{DataConverter(da.AmountInUnits, da.DisplayDecimals)}{(da.DisplayDescription ? $" {da.Description}" : "")}");
                    }
                    else
                    {
                        if (da.DisplayDecimals)
                        {
                            serializedStrings.Add($"{(Math.Floor(Convert.ToDouble(da.AmountInUnits) * 100) / 100).ToString(AMOUNT_FORMAT_WITH_DECIMAL)} {da.Unit}{(da.DisplayDescription ? $" {da.Description}" : "")}");
                        }
                        else
                        {
                            serializedStrings.Add($"{Math.Floor(Convert.ToDouble(da.AmountInUnits)).ToString(AMOUNT_FORMAT)} {da.Unit}{(da.DisplayDescription ? $" {da.Description}" : "")}");
                        }
                    }
                }

            }

            if (productBenefits.Count <= 1) return string.Join(",", serializedStrings);

            return string.Join($"{separator} ", serializedStrings.ToArray(), 0, serializedStrings.Count - 1) + ", & " + serializedStrings.LastOrDefault();
        }

        public static string DataConverter(double bytes, bool displayDecimals)
        {
            int counter = 0;
            double value = bytes;
            string text;
            do
            {
                if (displayDecimals)
                {
                    text = $"{(Math.Floor(Convert.ToDouble(value) * 100) / 100).ToString(AMOUNT_FORMAT_WITH_DECIMAL)} {_binary_prefix[counter]}";
                }
                else
                {
                    text = $"{Math.Floor(Convert.ToDouble(value)).ToString(AMOUNT_FORMAT)} {_binary_prefix[counter]}";
                }

                value /= 1024;
                counter++;
            }
            while (Math.Floor(value) > 0 && counter < _binary_prefix.Length);
            return text;
        }

        public static List<BenefitProcessingResponse> GetBenefits(Product product, double amountInCedis)
        {
            List<BenefitProcessingResponse> benefits = new List<BenefitProcessingResponse>();
            if (!Utility.IsEmpty(product.DaInfos))
            {
                for (int i = 0; i < product.DaInfos.Count; i++)
                {
                    var benefitUnit = product.DaInfos[i]?.StaticValue <= 0 ? 1 / product.DaInfos[i].ERM * amountInCedis : product.DaInfos[i]?.StaticValue ?? 0;

                    benefits.Add(new BenefitProcessingResponse
                    {
                        DAId = product.DaInfos[i].DaId,
                        AmountInUnits = benefitUnit,
                        ValidityDays = product.DaInfos[i].ValidityDays,
                        Unit = product.DaInfos[i].Unit,
                        AccountUnitType = product.DaInfos[i].AccountUnitType,
                        Description = product.DaInfos[i].Description,
                        DisplayDecimals = product.DaInfos[i].DisplayDecimals,
                        DisplayDescription = product.DaInfos[i].DisplayDescription,
                        Visible = product.DaInfos[i].Visible
                    });
                }
            }
            return benefits;
        }
    }
}
