using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ucip_lib_v5
{
    public class AirServerHelper
    {
        private readonly string _protocol;
        private readonly string _ipAddress;
        private readonly int _port;
        private readonly string _path;
        private readonly string _authCredentials;
        private readonly IHttpClientFactory _httpClientFactory;

        public AirServerHelper(string protocol, string IPAddress, int port, string path, string authBase64Credentials, IHttpClientFactory httpClientFactory)
        {
            _protocol = protocol;
            _ipAddress = IPAddress;
            _port = port;
            _path = path;
            _authCredentials = authBase64Credentials;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> Execute(string payload)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                var ucipUrl = $"{_protocol}://{_ipAddress}:{_port}{_path}";

                var request = new HttpRequestMessage(HttpMethod.Post, ucipUrl);

                request.Headers.Host = $"{_ipAddress}:{_port}";
                request.Headers.TryAddWithoutValidation("User-Agent", "UGw Server/5.0/1.0");

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authCredentials);

                request.Content = new StringContent(payload, Encoding.UTF8, "text/xml");
                var response = await client.SendAsync(request);

                var textResponse = await response.Content.ReadAsStringAsync();


                return textResponse;
            }
            catch (Exception)
            {
                return "";
            }

        }
    }
}
