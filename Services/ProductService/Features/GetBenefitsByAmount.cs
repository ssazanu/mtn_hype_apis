using helpers;
using helpers.Atttibutes;
using helpers.Interfaces;
using Microsoft.Extensions.Configuration;
using models;
using System.Threading.Tasks;

namespace ProvisionService.Features
{
    [Feature(Name = "GetBenefitsByAmount")]
    public class GetBenefitsByAmount : BaseServiceFeature
    {
        private readonly string _channel; 

        public GetBenefitsByAmount(IConfiguration configuration)
        {

        }

        [Entry(Method = "POST")]
        public async Task<ApiResponse> Entry([FromJsonBody] dynamic payload)
        {

            return new ApiResponse { };
        }

        private string ValidatePayload(dynamic payload)
        {

            return null;
        }
    }
}

