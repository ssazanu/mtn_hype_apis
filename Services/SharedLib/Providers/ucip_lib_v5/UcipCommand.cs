using System;
using System.IO;
using System.Net.Http;
using ucip_lib_v5.models;

namespace ucip_lib_v5
{
    public class UcipCommand : IUcipCommand
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CommandTemplates _templates;

        public UcipCommand(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _templates = new CommandTemplates
            {
                DaStructure = ReadCommandFile("Da_Structure.txt"),
                DaValueStructure = ReadCommandFile("Da_Value_Structure.txt"),
                PamStructure = ReadCommandFile("PAM_Structure.txt"),
                PamValueStructure = ReadCommandFile("PAM_Value_Structure.txt"),
                OfferStructure = ReadCommandFile("Offer_Structure.txt"),
                OfferValueStructure = ReadCommandFile("Offer_Value_Structure.txt"),
                MemberStructure = ReadCommandFile("Member_Structure.txt"),
                GeneralUpdate = ReadCommandFile("General_Update.txt"),
                GeneralGet = ReadCommandFile("General_Get.txt"),
                TreeParameterStructure = ReadCommandFile("Tree_Parameter_Structure.txt"),
                RateParameterStructure = ReadCommandFile("Rate_Parameter_Structure.txt"),
            };
        }

        public UcipCommandBuilder CreateBuilder(AirServer airServer)
        {
            var builderTemplates = _templates;
            return new UcipCommandBuilder(_httpClientFactory, airServer, builderTemplates);
        }

        private static string ReadCommandFile(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Commands", "Ucip", "v5", fileName);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return "";
        }
    }
}
