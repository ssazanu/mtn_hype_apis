using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using helpers;
using helpers.Notifications;
using models;
using ProvisionService.Models;
using Serilog;
using SharedLib.Interfaces;
using SharedLib.Models;
using SharedLib.Providers.CategoryProvider;
using SharedLib.Providers.ProvisionProvider;

namespace ProvisionService.Providers
{
	public class BundleProvisionProvider : IBundleProvisionProvider
	{
        private readonly BaseConfigurationProvider _baseConfigurationProvider;
        private readonly IChargingProvider _chargingProvider;
        private readonly ISmsNotification _smsNotification;
        private readonly IMessageBuilder _messageBuilder;
        private readonly IDataProvider _dataProvider;

        public BundleProvisionProvider(BaseConfigurationProvider baseConfigurationProvider, IChargingProvider chargingProvider,
            ISmsNotification smsNotification, IMessageBuilder messageBuilder, IDataProvider dataProvider)
		{
            _baseConfigurationProvider = baseConfigurationProvider;
            _chargingProvider = chargingProvider;
            _smsNotification = smsNotification;
            _messageBuilder = messageBuilder;
            _dataProvider = dataProvider;
        }

        public async Task<ApiResponse<Data>> BundleProvision(string requesterMsisdn, string beneficiaryMsisdn,
            long productId, string currency, string paymentChannel, string paymentTransactionId,
            IntegratorAccount integratorAccount, string channel, bool chargeSubscriber)
        {
            var product = await _baseConfigurationProvider.GetProductById(productId);

            Category category = await _baseConfigurationProvider.GetCategoryByProductId(productId);

            var amount = product.MaxAmount;
            //if (product.MaxAmount == product.MinAmount) amount = product.MaxAmount; for future use

            return await ProvisionCategory(requesterMsisdn, beneficiaryMsisdn, requesterMsisdn, product, category,
                amount, paymentChannel, paymentTransactionId, integratorAccount,
                channel, chargeSubscriber);
        }

        private async Task<ApiResponse<Data>> ProvisionCategory(string requesterMsisdn, string beneficiaryMsisdn,
            string payingMsisdn, SharedLib.Models.Product product, Category category, double amount,
            string paymentChannel, string paymentTransactionId, IntegratorAccount integratorAccount, string channel, bool chargeSubscriber)
        {
            if (product.DaInfos == null || !product.DaInfos.Any())
                return new ApiResponse<Data> { ResponseMessage = "No dedicated accounts available for the selected product" };

            requesterMsisdn = Utility.FormatNumber12Digits(requesterMsisdn);
            beneficiaryMsisdn = Utility.FormatNumber12Digits(beneficiaryMsisdn);
            payingMsisdn = Utility.FormatNumber12Digits(payingMsisdn);

            AccountBalance balance = new AccountBalance { MainAccountBalance = -99999 };
            AccountBalance beneficiaryBalance = new AccountBalance { MainAccountBalance = -99999 };

            if (!string.IsNullOrWhiteSpace(product.AllowedServiceClass))
            {
                beneficiaryBalance = await _chargingProvider.GetBalance(beneficiaryMsisdn, paymentTransactionId);
                Log.Information($"Beneficiary Balance [{beneficiaryMsisdn}] => MainAccountBalance: {beneficiaryBalance.MainAccountBalance}, ServiceClass: {beneficiaryBalance.ServiceClass}");

                if (beneficiaryBalance == null)
                    return new ApiResponse<Data> { ResponseMessage = "Unable to complete beneficiary qualification check. Kindly try again later." };

                if (!product.AllowedServiceClass.Split(',').Contains($"{beneficiaryBalance.ServiceClass}"))
                    return new ApiResponse<Data> { ResponseMessage = $"Sorry! {beneficiaryMsisdn} is not allowed for this service" };
            }

            if (chargeSubscriber)
            {
                if (payingMsisdn == beneficiaryMsisdn && beneficiaryBalance.MainAccountBalance != -99999)
                {
                    balance = beneficiaryBalance;
                }
                else
                {
                    balance = await _chargingProvider.GetBalance(payingMsisdn, paymentTransactionId);
                    Log.Information($"Paying Balance [{payingMsisdn}] => MainAccountBalance: {balance}, ServiceClass: {balance.ServiceClass}");
                }

                if (balance == null) return new ApiResponse<Data> { ResponseMessage = "Unable to complete balance check. Kindly try again." };
                if (balance.MainAccountBalance < amount) new ApiResponse<Data> { ResponseMessage = $"Insufficient balance for {Utility.FormatNumber10Digits(payingMsisdn)}" };
            }

            var productBenefits = ProvisioningHelper.GetBenefits(product, amount);
            var benefit = ProvisioningHelper.SerializeBenefits(productBenefits);
            var productValidity = product.DaInfos.Select(x => x.ValidityDays).Max();
            var productExpiry = DateTime.Now.AddDays(productValidity);

            var transactionId = !string.IsNullOrWhiteSpace(paymentTransactionId) ? paymentTransactionId : GenerateTransactionId(payingMsisdn);

            try
            {
                //Provision Resources
                var provisionResponse = await _chargingProvider.ProvisionResources(transactionId, requesterMsisdn, payingMsisdn, paymentChannel,
                    beneficiaryMsisdn, amount, product, category, null, !chargeSubscriber, false, chargeSubscriber);

                if (!provisionResponse.IsSuccessful)
                {
                    await SaveFailedTransaction(requesterMsisdn, beneficiaryMsisdn, product, category, amount, benefit, paymentChannel,
                        balance, null, productExpiry, transactionId, paymentTransactionId, channel, false);

                    return new ApiResponse<Data> { ResponseMessage = provisionResponse.ResponseMessage}; 
                }

                //Prepare SMS and USSD
                SmsContents successMessage = await PrepareSmsAndUssdResponses(requesterMsisdn, beneficiaryMsisdn, product, category, amount, benefit, false, integratorAccount);

                _ = SaveSuccessfulTransaction(product, amount, requesterMsisdn, beneficiaryMsisdn, category, paymentChannel, benefit,
                    successMessage, false, null, productExpiry, transactionId, paymentTransactionId, false, channel);

                _ = SendNotifications(requesterMsisdn, beneficiaryMsisdn, successMessage);

                _ = QueueHooks(category, product, transactionId, amount, requesterMsisdn, beneficiaryMsisdn, paymentChannel, benefit, successMessage, null, paymentTransactionId, channel, productExpiry);

                Data provisionInfo = PopulateProvisionInfo(product, category, amount, integratorAccount, productExpiry, transactionId);

                return new ApiResponse<Data> { Success = true, ResponseMessage = successMessage.Response, Data = provisionInfo };
            }
            catch (Exception ex)
            {
                await SaveFailedTransaction(requesterMsisdn, beneficiaryMsisdn, product, category, amount, benefit, paymentChannel,
                        balance, null, productExpiry, transactionId, paymentTransactionId, channel, false);

                Log.Error($"{ex}");
                throw;
             
            }
        }

        private static Data PopulateProvisionInfo(SharedLib.Models.Product product, Category category, double amount, IntegratorAccount integratorAccount, DateTime productExpiry, string transactionId)
        {
            if (integratorAccount.Configurations != null && integratorAccount.Configurations.Any(x => x.Key == "RETURN_PROVISION_DETAILS" && x.Value == "Y"))
            {
                var validity = productExpiry - DateTime.Now;
                return new Data
                {
                    Product = new Models.Product
                    {
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        CategoryName = category.CategoryName,
                        Cost = amount,
                        CategoryId = category.ConfigId,
                        Description = product.ProductName,
                        ValidityDays = validity.Days,
                        Type = product.ProductType,
                        ValidityHours = validity.Hours,
                        CostCurrency = new CostCurrency { },
                        TransactionId = transactionId
                    }
                };
            }



            return null;
        }

        private string GenerateTransactionId(string msisdn)
        {
            var ticks = DateTime.Now.Ticks.ToString();
            return $"{Utility.FormatNumber9Digits(msisdn)}{new Random().Next(1, 9999)}{ticks.Substring(ticks.Length - 4)}";
        }

        private async Task SendNotifications(string requesterMsisdn, string beneficiaryMsisdn, SmsContents successMessage)
        {
            var messages = new List<(string recipient, string message)>
            {
                (requesterMsisdn, successMessage.Self)
            };

            if (beneficiaryMsisdn != requesterMsisdn)
                messages.Add((beneficiaryMsisdn, successMessage.Reciepient));

            var reqSms = await _smsNotification.Dispatch(messages);
            Log.Information($"{reqSms}");
        }

        private async Task<SmsContents> PrepareSmsAndUssdResponses(string requesterMsisdn, string beneficiaryMsisdn, SharedLib.Models.Product product,
            Category category, double amountInCedi, string benefits, bool isRenewalRequest, IntegratorAccount integratorAccount)
        {
            MessageOption messageItem = new MessageOption
            {
                Templates = product?.Notification,
                Amount = amountInCedi,
                SenderMsisdn = requesterMsisdn,
                BeneficiayMsisdn = beneficiaryMsisdn,
                Benefits = benefits,
                BenefitsBonus = null,
                BuyForOthers = requesterMsisdn != beneficiaryMsisdn,
                AutoRenewed = isRenewalRequest,
                Product = product,
                Category = category,
                Configurations = integratorAccount.Configurations,
            };

            SmsContents successMessage = _messageBuilder.GenerateProductSuccessMessage(messageItem);

            return successMessage;
        }

        private async Task SaveSuccessfulTransaction(SharedLib.Models.Product product, double amountInCedi, string requesterMsisdn, string beneficiaryMsisdn, Category category,
            string paymentMode, string benefits, SmsContents successMessage, bool autoRenew, string referralCode, DateTime productExpiry, string transactionId,
            string externalTransactionId, bool renewed, string channel)
        {
            try
            {
                await _dataProvider.SaveSuccessfulTransaction(product.Id, amountInCedi, requesterMsisdn, beneficiaryMsisdn, product.ProductName,
                   category.CategoryName, paymentMode, benefits, successMessage.Self, successMessage.Response, autoRenew, referralCode ?? "", productExpiry, transactionId, renewed, externalTransactionId ?? "", channel ?? "");

            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                var parameters = new
                {
                    productId = product.Id,
                    amountInCedi,
                    requesterMsisdn,
                    beneficiaryMsisdn,
                    product.ProductName,
                    category.CategoryName,
                    paymentMode,
                    benefits,
                    successMessage,
                    autoRenew,
                    referralCode,
                    productExpiry,
                    transactionId,
                    externalTransactionId,
                    channel
                };
                Log.Warning($"Failed => SaveSuccessfulTransaction => {parameters.Stringify()}");
            }
        }



        private async Task SaveFailedTransaction(string requesterMsisdn, string beneficiaryMsisdn, SharedLib.Models.Product product, Category category, double amountInCedi, string benefits, string paymentMode,
                                    AccountBalance balance, string referralCode, DateTime productExpiry, string transactionId, string externalTransactionId, string channel, bool isRenewalRequest)
        {
            try
            {
                await _dataProvider.SaveFailedTransaction(requesterMsisdn, beneficiaryMsisdn, product.Id, product.ProductName, category.CategoryName, amountInCedi, benefits,
                   paymentMode, balance.MainAccountBalance, referralCode ?? "", productExpiry, transactionId, externalTransactionId ?? "", channel ?? "");
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                var parameters = new
                {
                    requesterMsisdn,
                    beneficiaryMsisdn,
                    productId = product.Id,
                    product.ProductName,
                    category.CategoryName,
                    amountInCedi,
                    benefits,
                    paymentMode,
                    balance.MainAccountBalance,
                    referralCode,
                    productExpiry,
                    transactionId,
                    externalTransactionId,
                    channel
                };
                Log.Warning($"Failed => SaveFailedTransaction => {parameters.Stringify()}");
            }
        }

        private async Task QueueHooks(Category category, SharedLib.Models.Product product, string transactionId, double amount, string requesterMsisdn, string beneficiaryMsisdn, string paymentMode, string benefits, SmsContents smsMessage, string referralCode, string externalTransactionId, string channel, DateTime productExpiry)
        {
            List<HookRequest> data = new List<HookRequest> { };
            try
            {
                if (product.Hooks != null && product.Hooks.Any())
                {
                    data = product.Hooks.Select(x => new HookRequest
                    {
                        HookId = x.Id,
                        TransactionId = transactionId,
                        ExternalTransactionId = externalTransactionId,
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        CategoryName = category.CategoryName,
                        Amount = amount,
                        RequesterMsisdn = requesterMsisdn,
                        BeneficiaryMsisdn = beneficiaryMsisdn,
                        PaymentMode = paymentMode,
                        Benefits = benefits,
                        SmsMessages = smsMessage,
                        ReferralCode = referralCode,
                        TransactionDate = DateTime.Now,
                        Channel = channel,
                        ProductExpiry = productExpiry
                    }).ToList();



                    await _dataProvider.AddBulkHookRequest(data);
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                Log.Warning($"Failed => AddBulkHookRequest => {data.Stringify()}");
            }
        }
    }
}

