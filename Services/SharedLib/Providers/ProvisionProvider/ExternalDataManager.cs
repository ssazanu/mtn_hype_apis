using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SharedLib.Interfaces;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharedLib.Providers.ProvisionProvider
{
    public class ExternalDataManager : IExternalDataManager
    {
        private protected List<ExternalData> _externalData;
        private readonly string _renewal_flag;
        public ExternalDataManager(IConfiguration config)
        {
            _renewal_flag = config.GetValue("CDR_RENEWAL_FLAG", "RENEWAL");
            _externalData = new List<ExternalData>();
            SetupInMemoryJSONExternalDataList();
        }

        public ExternalData GetExternalDataForProduct(string categoryId, double cost, string paymentMode, string payingMsisdn, string beneficiaryMsisdn, string productType, string referralCode, bool isRenewal)
        {
            if (_externalData == null || !_externalData.Any()) return null;
            var externalData = _externalData.FirstOrDefault(e => e.CategoryId == categoryId);
            if (externalData != null)
            {
                return new ExternalData
                {
                    CategoryId = externalData.CategoryId,
                    ExternalData1 = externalData.ExternalData1
                                    .Replace("$$REQUEST_TYPE$$", isRenewal ? "R" : $"S"),
                    OriginHostName = externalData.OriginHostName,
                    OriginNodeType = externalData.OriginNodeType,
                    ExternalData2 = externalData.ExternalData2.Replace("$$AMOUNT$$", $"{cost}")
                   .Replace("$$PAYMENT_MODE$$", paymentMode?.ToUpper())
                   .Replace("$$CATEGORY$$", categoryId?.ToUpper())
                   .Replace("$$PRODUCT_TYPE$$", productType.Replace(" ", "_"))

                   .Replace("$$_RENEWAL_FLAG$$", !isRenewal ? "" : $"-{_renewal_flag}")
                   .Replace("$$RENEWAL_FLAG$$", !isRenewal ? "" : $"{_renewal_flag}")

                   .Replace("$$_REFERRAL_CODE$$", string.IsNullOrEmpty(referralCode) ? "" : $"-{referralCode}")
                   .Replace("$$REFERRAL_CODE$$", string.IsNullOrEmpty(referralCode) ? "" : $"{referralCode}")

                   .Replace("$$_BENEFICIARY$$", string.IsNullOrEmpty(beneficiaryMsisdn) ? "" : $"-{beneficiaryMsisdn}")
                   .Replace("$$BENEFICIARY$$", string.IsNullOrEmpty(beneficiaryMsisdn) ? "" : $"{beneficiaryMsisdn}")

                   .Replace("$$_PAYING_MSISDN$$", string.IsNullOrEmpty(payingMsisdn) ? "" : $"-{payingMsisdn}")
                   .Replace("$$PAYING_MSISDN$$", string.IsNullOrEmpty(payingMsisdn) ? "" : $"{payingMsisdn}")
                };
            }
            return externalData;
        }


        protected void SetupInMemoryJSONExternalDataList()
        {
            string externalDataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ExternalData.json");

            using (StreamReader r = new StreamReader(externalDataFile))
            {
                string json = r.ReadToEnd();
                _externalData = JsonConvert.DeserializeObject<List<ExternalData>>(json);
            }

        }
    }
}
