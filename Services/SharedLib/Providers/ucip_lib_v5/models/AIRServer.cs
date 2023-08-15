namespace ucip_lib_v5.models
{
    public class AirServer
    {
        public string Protocol { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Base64Credential { get; set; }
        public bool Active { get; set; }
        public string DefaultOriginHostName { get; set; }
        public string DefaultOriginNodeType { get; set; }
        public string DefaultExternalData1 { get; set; }
        public string DefaultExternalData2 { get; set; }
    }
}
