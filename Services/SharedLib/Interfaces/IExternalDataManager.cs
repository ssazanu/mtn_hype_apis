using SharedLib.Models;

namespace SharedLib.Interfaces
{
    public interface IExternalDataManager
    {
        ExternalData GetExternalDataForProduct(string categoryId, double cost, string paymentMode, string payingMsisdn, string beneficiaryMsisdn, string productType, string referralCode, bool isRenewal);
    }
}