using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ucip_lib_v5.models;

namespace ucip_lib_v5
{
    public class UcipCommandBuilder
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AirServer _airServer;
        private readonly CommandTemplates _commandTemplates;
        private protected List<(string key, string value)> _parameters;
        private protected List<(string key, string value)> _tree_parameters;
        private protected List<(OfferInformation offer, List<DAUpdateInformation> daInfos)> _offers;
        private protected List<DAUpdateInformation> _daInfos;
        private protected List<PamInfo> _pamInfos;
        private readonly string _dateFormat = "yyyyMMddTHH:mm:ss+0000";


        public UcipCommandBuilder(IHttpClientFactory httpClientFactory, AirServer airServer, CommandTemplates commandTemplates)
        {
            this.httpClientFactory = httpClientFactory;
            _airServer = airServer;
            _commandTemplates = commandTemplates;
            _parameters = new List<(string key, string value)>();
            _daInfos = new List<DAUpdateInformation>();
            _pamInfos = new List<PamInfo>();
            _offers = new List<(OfferInformation offer, List<DAUpdateInformation> daInfos)>();
        }

        private string GetErmDecimals(double erm)
        {
            var ermString = erm.ToString();
            return ermString.Substring(ermString.IndexOf('.') + 1);
        }

        public UcipCommandBuilder SetMsisdnAndAdjustment(string msisdn, double amount)
        {
            _parameters.Add(("$$subscriberNumber$$", msisdn));
            _parameters.Add(("$$MA_Adjustment_Relative$$", $"{(int)(amount * 100.0)}"));
            return this;
        }
        public UcipCommandBuilder SetMsisdn(string msisdn)
        {
            _parameters.Add(("$$subscriberNumber$$", msisdn));
            return this;
        }
        public UcipCommandBuilder SetExternalDataInfo(params string[] externalData)
        {
            for (int i = 0; i < externalData.Length; i++)
            {
                _parameters.Add(($"$$externalData{i + 1}$$", externalData[i]));
            }

            return this;
        }
        public UcipCommandBuilder SetOriginInfo(string transactionId, string hostName, string nodeType, DateTime timestamp)
        {
            _parameters.Add(("$$originHostName$$", hostName));
            _parameters.Add(("$$originNodeType$$", nodeType));
            _parameters.Add(("$$originTransactionID$$", transactionId));
            _parameters.Add(("$$originTimeStamp$$", timestamp.ToString(_dateFormat)));

            return this;
        }

        public UcipCommandBuilder SetDaInfo(DaInfoWithOffer daInfo)
        {
            if (daInfo != null)
            {
                if (daInfo.Offers != null && daInfo.Offers.Any())
                {
                    for (int j = 0; j < daInfo.Offers.Count; j++)
                    {
                        if (!_offers.Any(_ => _.offer.OfferId == daInfo.Offers[j].OfferId))
                        {
                            (OfferInformation, List<DAUpdateInformation>) offer = (daInfo.Offers[j], new List<DAUpdateInformation> { });
                            if (j == 0)
                            {
                                DAUpdateInformation da = new DAUpdateInformation
                                {
                                    DAId = daInfo.DAId,
                                    DAValueCedis = (int)(daInfo.DAValueCedis * 100),
                                    Erm = daInfo.Erm,
                                    DedicatedAccountUnitType = daInfo.DedicatedAccountUnitType,
                                    ExpiryDate = daInfo.ExpiryDate,
                                };

                                offer.Item2.Add(da);
                            }

                            _offers.Add(offer);
                        }
                        else
                        {
                            _offers.FirstOrDefault(_ => _.offer.OfferId == daInfo.Offers[j].OfferId)
                                .daInfos.Add(new DAUpdateInformation
                                {
                                    DAId = daInfo.DAId,
                                    DAValueCedis = (int)(daInfo.DAValueCedis * 100),
                                    Erm = daInfo.Erm,
                                    ExpiryDate = daInfo.ExpiryDate,
                                    DedicatedAccountUnitType = daInfo.DedicatedAccountUnitType
                                });
                        }
                    }
                }
            }

            return this;
        }

        public UcipCommandBuilder SetDaInfo(List<DaInfoWithOffer> daInfos)
        {
            if (daInfos != null)
            {
                for (int i = 0; i < daInfos.Count; i++)
                {
                    SetDaInfo(daInfos[i]);
                }
            }

            return this;
        }

        public UcipCommandBuilder SetPamInfos(List<PamInfo> pamInfos)
        {
            if (pamInfos != null)
            {
                for (int i = 0; i < pamInfos.Count; i++)
                {
                    _pamInfos.Add(new PamInfo
                    {
                        ClassId = pamInfos[i].ClassId,
                        ServiceId = pamInfos[i].ServiceId,
                        ScheduleId = pamInfos[i].ScheduleId,
                    });
                }
            }

            return this;
        }

        public async Task<CommandDetail> GeneralUpdate()
        {
            AirServerHelper airServerHelper = new AirServerHelper(_airServer.Protocol, _airServer.IPAddress, _airServer.Port, "/Air", _airServer.Base64Credential, httpClientFactory);
            var mainTemplate = _commandTemplates.GeneralUpdate;
            StringBuilder commandStructure = new StringBuilder();
            for (int i = 0; i < _parameters.Count; i++)
            {
                mainTemplate = mainTemplate.Replace(_parameters[i].key, _parameters[i].value);
            }

            if (_offers.Any())
            {
                var offerTemplate = _commandTemplates.OfferStructure;
                StringBuilder offers = new StringBuilder();
                for (int i = 0; i < _offers.Count; i++)
                {
                    var structure = _commandTemplates.OfferValueStructure;
                    structure = structure
                        .Replace("$$OFFER_ID$$", $"{_offers[i].offer.OfferId}")
                        .Replace("$$OFFER_TYPE$$", $"{_offers[i].offer.OfferType}")
                        .Replace("$$EXPITY$$", $"{_offers[i].offer.Expiry.ToString(_dateFormat)}");
                    if (_offers[i].daInfos != null && _offers[i].daInfos.Any())
                    {
                        (string, int) rate_params = (null, 0);
                        var da_structure = _commandTemplates.DaStructure;
                        StringBuilder da_value_structure = new StringBuilder();

                        for (int j = 0; j < _offers[i].daInfos.Count; j++)
                        {
                            da_value_structure.Append(_commandTemplates.DaValueStructure
                                .Replace("$$DA_ID$$", _offers[i].daInfos[j].DAId.ToString())
                                .Replace("$$DA_VALUE$$", _offers[i].daInfos[j].DAValueCedis.ToString())
                                .Replace("$$ACCOUNT_UNIT_TYPE$$", _offers[i].daInfos[j].DedicatedAccountUnitType.ToString())
                                .Replace("$$EXPIRY$$", _offers[i].daInfos[j].ExpiryDate.ToString(_dateFormat))
                            );
                            if (string.IsNullOrWhiteSpace(rate_params.Item1))
                            {
                                var rateDecimal = GetErmDecimals(_offers[i].daInfos[j].Erm);
                                rate_params = (rateDecimal, rateDecimal.Length);
                            }
                        }
                        da_structure = da_structure.Replace("$$DA_VALUES_STRUCTURE$$", da_value_structure.ToString());
                        structure = structure.Replace("$$DA_STRUCTURE$$", da_structure);

                        var tree_params_structure = _commandTemplates.TreeParameterStructure;
                        var rate_parameters = _commandTemplates.RateParameterStructure
                                                .Replace("$$DECIMALS$$", rate_params.Item1)
                                                .Replace("$$DECIMALS_LENGTH$$", $"{rate_params.Item2}");

                        tree_params_structure = tree_params_structure.Replace("$$TREE_PARAMETER_MEMBERS$$", rate_parameters);
                        structure = structure.Replace("$$TREE_PARAMETERS_STRUCTURE$$", tree_params_structure);
                    }
                    else
                    {
                        structure = structure.Replace("$$DA_STRUCTURE$$", "");
                        structure = structure.Replace("$$TREE_PARAMETERS_STRUCTURE$$", "");
                        structure = structure.Replace("$$TREE_PARAMETER_MEMBERS$$", "");
                    }
                    offers.AppendLine(structure);
                }
                offerTemplate = offerTemplate.Replace("$$OFFER_VALUES_STRUCTURE$$", offers.ToString());
                commandStructure.AppendLine(offerTemplate);
            }

            if (_daInfos != null && _daInfos.Any())
            {
                var da_structure = _commandTemplates.DaStructure;
                StringBuilder da_value_structure = new StringBuilder();
                for (int i = 0; i < _daInfos.Count; i++)
                {
                    da_value_structure.Append(_commandTemplates.DaValueStructure
                                .Replace("$$DA_ID$$", _daInfos[i].DAId.ToString())
                                .Replace("$$DA_VALUE$$", _daInfos[i].DAValueCedis.ToString())
                                .Replace("$$ACCOUNT_UNIT_TYPE$$", _daInfos[i].DedicatedAccountUnitType.ToString())
                                .Replace("$$EXPIRY$$", _daInfos[i].ExpiryDate.ToString(_dateFormat))
                            );
                }
                da_structure = da_structure.Replace("$$DA_VALUES_STRUCTURE$$", da_value_structure.ToString());
                commandStructure.AppendLine(da_structure);
            }

            if (_pamInfos.Any())
            {
                var pam_structure = _commandTemplates.PamStructure;
                StringBuilder pam_value_structure = new StringBuilder();
                for (int i = 0; i < _pamInfos.Count; i++)
                {
                    pam_value_structure.Append(_commandTemplates.PamValueStructure
                                .Replace("$$PAM_CLASS_ID$$", _pamInfos[i].ClassId.ToString())
                                .Replace("$$PAM_SERVICE_ID$$", _pamInfos[i].ServiceId.ToString())
                                .Replace("$$PAM_SCHEDULE_ID$$", _pamInfos[i].ScheduleId.ToString())
                            );
                }
                pam_structure = pam_structure.Replace("$$PAM_VALUES_STRUCTURE$$", pam_value_structure.ToString());
                commandStructure.AppendLine(pam_structure);
            }

            mainTemplate = mainTemplate.Replace("$$UPDATE_MEMBER_STRUCTURES$$", commandStructure.ToString());
            string response = await airServerHelper.Execute(mainTemplate);
            return new CommandDetail { command = mainTemplate, response = response };
        }

        public async Task<CommandDetail> GeneralGet()
        {
            AirServerHelper airServerHelper = new AirServerHelper(_airServer.Protocol, _airServer.IPAddress, _airServer.Port, "/Air", _airServer.Base64Credential, httpClientFactory);
            var mainTemplate = _commandTemplates.GeneralGet;
            for (int i = 0; i < _parameters.Count; i++)
            {
                mainTemplate = mainTemplate.Replace(_parameters[i].key, _parameters[i].value);
            }

            string response = await airServerHelper.Execute(mainTemplate);
            return new CommandDetail { command = mainTemplate, response = response };
        }

    }
}
