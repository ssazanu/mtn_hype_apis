using System;
using helpers.Database.Models;
using ProvisionService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using helpers.Database;
using Serilog;
using SharedLib.Models;
using NpgsqlTypes;
using helpers;

namespace ProvisionService.Providers
{
    public class DataProvider : IDataProvider
    {
        private readonly IDBHelper _dbHelper;
        public DataProvider(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<IntegratorAccount>> GetIntegrators()
        {
            List<StoreProcedureParameter> parameters = new List<StoreProcedureParameter> { };

            return await _dbHelper.Fetch<IntegratorAccount>("GetIntegrators", parameters);
        }

        public async Task SaveSuccessfulTransaction(long productId, double amount, string requesterMsisdn, string beneficiaryMsisdn, string productName,
            string categoryName, string paymentMode, string benefits, string smsResponseMessage, string ussdResponseMessage,
            bool reqAutoRenew, string reqReferralCode, DateTime reqProductExpiry, string reqTransactionId, bool autoRenewed, string externalTransactionId, string channel)
        {
            List<StoreProcedureParameter> parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter { Name="reqProductId", Type = NpgsqlDbType.Bigint, Value =  productId},
                new StoreProcedureParameter { Name="reqAmount", Type = NpgsqlDbType.Double, Value = amount  },
                new StoreProcedureParameter { Name="reqRequesterMsisdn", Value = requesterMsisdn },
                new StoreProcedureParameter { Name="reqBeneficiaryMsisdn", Value = beneficiaryMsisdn },
                new StoreProcedureParameter { Name="reqProductName", Value =  productName},
                new StoreProcedureParameter { Name="reqCategoryName", Value = categoryName },
                new StoreProcedureParameter { Name="reqPaymentMode", Value = paymentMode },
                new StoreProcedureParameter { Name="reqBenefits", Value =  benefits},
                new StoreProcedureParameter { Name="reqSMSResponse", Value = smsResponseMessage },
                new StoreProcedureParameter { Name="reqUssdResponse", Value =  ussdResponseMessage},
                new StoreProcedureParameter { Name="reqAutoRenew", Type = NpgsqlDbType.Boolean, Value =  reqAutoRenew},
                new StoreProcedureParameter { Name="reqReferralCode", Value =  reqReferralCode},
                new StoreProcedureParameter { Name="reqProductExpiry", Type = NpgsqlDbType.Timestamp, Value =  reqProductExpiry},
                new StoreProcedureParameter { Name="reqTransactionId", Value =   $"{reqTransactionId}"},
                new StoreProcedureParameter { Name="reqAutoRenewed", Type = NpgsqlDbType.Boolean, Value =   autoRenewed},
                new StoreProcedureParameter { Name="reqExternalTransactionId", Value = externalTransactionId },
                new StoreProcedureParameter { Name="reqChannel", Value = channel },
            };

            await _dbHelper.ExecuteRaw("SaveSuccessfulTransaction", parameters);
        }

        public async Task SaveFailedTransaction(string requesterMsisdn, string beneficiaryMsisdn, long productId, string productName, string categoryName, double amount,
            string benefits, string paymentMode, double maBalance, string reqReferralCode, DateTime reqProductExpiry, string reqTransactionId, string externalTransactionId, string channel)
        {
            List<StoreProcedureParameter> parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter { Name="reqProductId", Type = NpgsqlDbType.Bigint, Value =  productId},
                new StoreProcedureParameter { Name="reqAmount", Type = NpgsqlDbType.Double, Value = amount  },
                new StoreProcedureParameter { Name="reqRequesterMsisdn", Value = requesterMsisdn },
                new StoreProcedureParameter { Name="reqBeneficiaryMsisdn", Value = beneficiaryMsisdn },
                new StoreProcedureParameter { Name="reqProductName", Value =  productName},
                new StoreProcedureParameter { Name="reqCategoryName", Value = categoryName },
                new StoreProcedureParameter { Name="reqPaymentMode", Value = paymentMode },
                new StoreProcedureParameter { Name="reqBenefits", Value =  benefits},
                new StoreProcedureParameter { Name="reqAccountBalance", Value =  $"{maBalance}"},
                new StoreProcedureParameter { Name="reqReferralCode", Value =  reqReferralCode},
                new StoreProcedureParameter { Name="reqProductExpiry", Type = NpgsqlDbType.Timestamp, Value =  reqProductExpiry},
                new StoreProcedureParameter { Name="reqTransactionId", Value = $"{reqTransactionId}" },
                new StoreProcedureParameter { Name="reqExternalTransactionId", Value = externalTransactionId },
                new StoreProcedureParameter { Name="reqChannel", Value = channel },
            };

            await _dbHelper.ExecuteRaw("SaveFailedTransaction", parameters);
        }

        public async Task AddBulkHookRequest(List<HookRequest> reqData)
        {
            List<StoreProcedureParameter> parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter { Name="reqData", Type = NpgsqlDbType.Json, Value =  reqData.Stringify()}
            };

            await _dbHelper.ExecuteRaw("AddBulkHookRequest", parameters);
        }
    }
}

