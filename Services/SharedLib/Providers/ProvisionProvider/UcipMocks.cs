using System;
using System.IO;
using System.Xml;
using ucip_lib_v5.models;

namespace SharedLib.Providers.ProvisionProvider
{
    public class UcipMocks
    {
        private readonly XmlDocument _mocks;
        public UcipMocks()
        {
            _mocks = new XmlDocument();
            LoadMocks();
        }

        public CommandDetail GetMock(string name, string tag = null)
        {
            var response = _mocks.GetElementsByTagName(name)[0]?.InnerXml;
            return new CommandDetail { command = $"<<{(tag ?? name)}>>", response = $"<?xml version=\"1.0\" encoding=\"utf-8\" ?>{response}" };
        }

        private void LoadMocks()
        {
#if DEBUG
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ucip_mocks.xml");
            if (File.Exists(filePath))
            {
                var mocks_xml = File.ReadAllText(filePath);
                _mocks.LoadXml(mocks_xml);
            }
#endif
        }
    }
}
