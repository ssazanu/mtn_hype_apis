using helpers;
using SharedLib.Interfaces;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharedLib.Providers.ProvisionProvider
{
    public class MessageBuilder : IMessageBuilder
    {
        private readonly string MSG_TEMPLATE = "Your bundle purchase was successful";
        protected List<SuccessMessage> _successMessages { get; }
        public MessageBuilder()
        {
            _successMessages = SetupInMemorySuccessMessageList();
        }
        public virtual SmsContents GenerateProductSuccessMessage(MessageOption messageItem)
        {
            var smsContents = new SmsContents();

            string smsTemplate = messageItem.Templates.FirstOrDefault(x => x.NotificationType?.ToUpper() == "SMS")?.Content ?? MSG_TEMPLATE;
            string responseTemplate = messageItem.Templates.FirstOrDefault(x => x.NotificationType?.ToUpper() == "RESPONSE")?.Content ?? MSG_TEMPLATE;
            string renewalFailedTemplate = messageItem.Templates.FirstOrDefault(x => x.NotificationType?.ToUpper() == "RENEWAL_FAILED_SMS")?.Content ?? MSG_TEMPLATE;
            string renewalPromptTemplate = messageItem.Templates.FirstOrDefault(x => x.NotificationType?.ToUpper() == "RENEWAL_PROMPT")?.Content ?? MSG_TEMPLATE;

            string ussdMessage = "";
            string buyerMessage = "";
            string beneficiaryMessage = "";
            string renewalFailed = "";
            string renewalPrompt = "";

            if (messageItem.Configurations != null && messageItem.Configurations.Count > 0)
            {
                var responseFooter = messageItem.Configurations.FirstOrDefault(x => x.Key.ToUpper() == "RESPONSE_FOOTER" && x.Active);
                if (responseFooter != null && !string.IsNullOrWhiteSpace(responseFooter.Value))
                {
                    responseTemplate = $"{responseTemplate} {responseFooter.Value}";
                }

                var responseHeader = messageItem.Configurations.FirstOrDefault(x => x.Key.ToUpper() == "RESPONSE_HEADER" && x.Active);
                if (responseHeader != null && !string.IsNullOrWhiteSpace(responseHeader.Value))
                {
                    responseTemplate = $"{responseHeader.Value} {responseTemplate}";
                }

                var smsFooter = messageItem.Configurations.FirstOrDefault(x => x.Key.ToUpper() == "SMS_FOOTER" && x.Active);
                if (smsFooter != null && !string.IsNullOrWhiteSpace(smsFooter.Value))
                {
                    smsTemplate = $"{smsTemplate} {smsFooter.Value}";
                }

                var smsHeader = messageItem.Configurations.FirstOrDefault(x => x.Key.ToUpper() == "SMS_HEADER" && x.Active);
                if (smsHeader != null && !string.IsNullOrWhiteSpace(smsHeader.Value))
                {
                    smsTemplate = $"{smsHeader.Value} {smsTemplate}";
                }
            }


            smsTemplate = ReplacePlaceholders(smsTemplate, messageItem);
            responseTemplate = ReplacePlaceholders(responseTemplate, messageItem);
            renewalFailed = ReplacePlaceholders(renewalFailedTemplate, messageItem);
            renewalPrompt = ReplacePlaceholders(renewalPromptTemplate, messageItem);


            if (messageItem.BuyForOthers)
            {
                ussdMessage = responseTemplate
                    .Replace("$$PURCHASED_RECEIVED$$", messageItem.AutoRenewed ? "renewed" : "purchased")
                    .Replace("$$PRODUCT_AMOUNT$$", $"{messageItem.Amount} for {messageItem.BeneficiayMsisdnFormatted}");

                buyerMessage = smsTemplate
                    .Replace("$$PURCHASED_RECEIVED$$", messageItem.AutoRenewed ? "renewed" : "purchased")
                    .Replace("$$PRODUCT_AMOUNT$$", $"{messageItem.Amount} for {messageItem.BeneficiayMsisdnFormatted}");

                beneficiaryMessage = smsTemplate
                    .Replace("$$PURCHASED_RECEIVED$$", messageItem.AutoRenewed ? "renewed" : "received")
                    .Replace("$$PRODUCT_AMOUNT$$", $"{messageItem.Amount} from {messageItem.SenderMsisdnFormatted}"); ;

            }
            else
            {
                ussdMessage = responseTemplate
                    .Replace("$$PURCHASED_RECEIVED$$", messageItem.AutoRenewed ? "renewed" : "purchased")
                    .Replace("$$PRODUCT_AMOUNT$$", $"{messageItem.Amount}")
                    .Replace("$$PRODUCT_AMOUNT_ONLY$$", $"{messageItem.Amount}");

                buyerMessage = smsTemplate
                    .Replace("$$PURCHASED_RECEIVED$$", messageItem.AutoRenewed ? "renewed" : "purchased")
                    .Replace("$$PRODUCT_AMOUNT$$", $"{messageItem.Amount}")
                    .Replace("$$PRODUCT_AMOUNT_ONLY$$", $"{messageItem.Amount}");

            }

            smsContents.Response = ussdMessage;
            smsContents.Reciepient = beneficiaryMessage;
            smsContents.Self = buyerMessage;
            smsContents.RenewalFailed = renewalFailed;
            smsContents.RenewalPrompt = renewalPrompt;

            return smsContents;

        }

        private string ReplacePlaceholders(string template, MessageOption messageItem)
        {
            return template
                        .Replace("$$BENEFITS$$", messageItem.Benefits)
                        .Replace("$$BONUS_EXPIRY$$", messageItem.BonusExpiry)
                        .Replace("$$CATEGORY_NAME$$", messageItem.Category.CategoryName)
                        .Replace("$$PRODUCT_NAME$$", messageItem.Product.ProductName)
                        .Replace("$$BONUS_BENEFITS$$", messageItem.BenefitsBonus)
                        .Replace("$$MSISDN$$", messageItem.SenderMsisdnFormatted)
                        .Replace("$$BENEFICIARY$$", messageItem.BeneficiayMsisdnFormatted)
                        .Replace("$$PRODUCT_AMOUNT_ONLY$$", $"{messageItem.Amount}");
        }

        protected List<SuccessMessage> SetupInMemorySuccessMessageList()
        {
            string succesMessageFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "SuccessMessages.json");
            using (StreamReader r = new StreamReader(succesMessageFile))
            {
                return r.ReadToEnd().ParseObject<List<SuccessMessage>>();
            }
        }
    }
}
