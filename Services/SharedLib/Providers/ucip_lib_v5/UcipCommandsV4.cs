using System;
using System.IO;

namespace ucip_lib_v5
{
    public class UcipCommandsV4
    {
        private string _GetAccountDetails;

        private string _CreditMA;

        private string _DebitMA;

        private string _UpdateBalance_MA_1DA;

        private string _UpdateBalance_MA_2DA;

        private string _UpdateBalance_MA_3DA;

        private string _UpdateBalance_MA_NDA;

        private string _UpdateBalance_DA_Structure;

        private string _GetAllOffers;

        private string _GetBalanceAndDate;

        private string _UpdateOffer;

        private string _UpdateConsumerOffer;

        private string _UpdateProviderOffer;

        private string _GetAccumulators;

        private string _UpdateServiceClass;

        private string _UpdateAccumulators;

        private string _DeleteOffer;

        private string _Refill;

        private string _AddPeriodicAccountManagementData;

        private string _RunPeriodicAccountManagement;

        private string _DeletePeriodicAccountManagementData;

        private string _UpdateUsageCounters;

        private string _UpdateUsageCounters_3UC;

        private string _UpdateUsageThresholds;

        private string _UpdateUsageThresholds_3UT;

        private string _GetUsageThresholdsAndCounters;

        private string _UpdateBalanceAndServiceDates;

        private string _SupervisionFragment;

        private string _ServiceFeeFragment;

        private string _UpdateBalanceAndServiceDates_NDA;

        private string _UpdateBalance_Absolute_DA_Structure;

        internal string GetAccountDetails => _GetAccountDetails;

        internal string CreditMA => _CreditMA;

        internal string DebitMA => _DebitMA;

        internal string UpdateBalance_MA_1DA => _UpdateBalance_MA_1DA;

        internal string UpdateBalance_MA_2DA => _UpdateBalance_MA_2DA;

        internal string UpdateBalance_MA_3DA => _UpdateBalance_MA_3DA;

        internal string UpdateBalance_MA_NDA => _UpdateBalance_MA_NDA;

        internal string UpdateBalance_DA_Structure => _UpdateBalance_DA_Structure;

        internal string GetAllOffers => _GetAllOffers;

        internal string GetBalanceAndDate => _GetBalanceAndDate;

        internal string UpdateOffer => _UpdateOffer;

        internal string UpdatConsumereOffer => _UpdateConsumerOffer;

        internal string UpdateProviderOffer => _UpdateProviderOffer;

        internal string GetAccumulators => _GetAccumulators;

        internal string UpdateServiceClass => _UpdateServiceClass;

        internal string UpdateAccumulators => _UpdateAccumulators;

        internal string DeleteOffer => _DeleteOffer;

        internal string Refill => _Refill;

        internal string AddPeriodicAccountManagementData => _AddPeriodicAccountManagementData;

        internal string RunPeriodicAccountManagement => _RunPeriodicAccountManagement;

        internal string DeletePeriodicAccountManagementData => _DeletePeriodicAccountManagementData;

        internal string UpdateUsageCounters => _UpdateUsageCounters;

        internal string UpdateUsageCounters_3UC => _UpdateUsageCounters_3UC;

        internal string UpdateUsageThresholds => _UpdateUsageThresholds;

        internal string UpdateUsageThresholds_3UT => _UpdateUsageThresholds_3UT;

        internal string GetUsageThresholdsAndCounters => _GetUsageThresholdsAndCounters;

        internal string UpdateBalanceAndServiceDates => _UpdateBalanceAndServiceDates;

        internal string SupervisionFragment => _SupervisionFragment;

        internal string ServiceFeeFragment => _ServiceFeeFragment;

        internal string UpdateBalanceAndServiceDates_NDA => _UpdateBalanceAndServiceDates_NDA;

        internal string UpdateBalance_Absolute_DA_Structure => _UpdateBalance_Absolute_DA_Structure;
        public UcipCommandsV4()
        {
            _GetAccountDetails = ReadCommandFile("GetAccountDetails.txt");
            _UpdateBalanceAndServiceDates = ReadCommandFile("UpdateBalanceAndServiceDates.txt");
            _UpdateBalanceAndServiceDates_NDA = ReadCommandFile("UpdateBalanceAndServiceDates_NDA.txt");
            _ServiceFeeFragment = ReadCommandFile("ServiceFeeFragment.txt");
            _SupervisionFragment = ReadCommandFile("SupervisionFragment.txt");
            _UpdateBalance_Absolute_DA_Structure = ReadCommandFile("UpdateBalance_Absolute_DA_Structure.txt");
            _UpdateBalance_MA_1DA = ReadCommandFile("UpdateBalance_MA_1DA.txt");
            _UpdateBalance_MA_2DA = ReadCommandFile("UpdateBalance_MA_2DA.txt");
            _UpdateBalance_MA_3DA = ReadCommandFile("UpdateBalance_MA_3DA.txt");
            _UpdateBalance_DA_Structure = ReadCommandFile("UpdateBalance_DA_Structure.txt");
            _UpdateBalance_MA_NDA = ReadCommandFile("UpdateBalance_MA_NDA.txt");
            _GetBalanceAndDate = ReadCommandFile("GetBalanceAndDate.txt");
            _CreditMA = ReadCommandFile("CreditBalanceMA.txt");
            _DebitMA = ReadCommandFile("DebitBalanceMA.txt");
            _GetAllOffers = ReadCommandFile("GetAllOffers.txt");
            _UpdateOffer = ReadCommandFile("UpdateOffer.txt");
            _DeleteOffer = ReadCommandFile("DeleteOffer.txt");
            _UpdateConsumerOffer = ReadCommandFile("UpdateConsumerOffer.txt");
            _UpdateProviderOffer = ReadCommandFile("UpdateProviderOffer.txt");
            _GetAccumulators = ReadCommandFile("GetAccumulators.txt");
            _UpdateAccumulators = ReadCommandFile("UpdateAccumulators.txt");
            _UpdateServiceClass = ReadCommandFile("UpdateServiceClass.txt");
            _AddPeriodicAccountManagementData = ReadCommandFile("AddPeriodicAccountManagementData.txt");
            _RunPeriodicAccountManagement = ReadCommandFile("RunPeriodicAccountManagement.txt");
            _DeletePeriodicAccountManagementData = ReadCommandFile("DeletePeriodicAccountManagementData.txt");
            _UpdateUsageCounters = ReadCommandFile("UpdateUsageCounters.txt");
            _UpdateUsageThresholds = ReadCommandFile("UpdateUsageThresholds.txt");
            _UpdateUsageCounters_3UC = ReadCommandFile("UpdateUsageCounters_3UC.txt");
            _UpdateUsageThresholds_3UT = ReadCommandFile("UpdateUsageThresholds_3UT.txt");
            _GetUsageThresholdsAndCounters = ReadCommandFile("GetUsageThresholdsAndCounters.txt");
            _Refill = ReadCommandFile("Refill.txt");
        }
        private static string ReadCommandFile(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Commands", "Ucip", fileName);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return "";
        }
    }
}
