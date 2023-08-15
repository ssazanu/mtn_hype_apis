using SharedLib.Models;
using ucip_lib_v5.models;

namespace SharedLib.Interfaces
{
    public interface ICommandResponseProcessor
    {
        AccountBalance GetBalanceInfoFromProcessingResponse(CommandDetail responsePayload, string[] successResponseCodes, string msisdn, string airServerIP);
        ProcessResponse GetCommandProcessingResponse(CommandDetail responsePayload, string[] successResponseCodes, string msisdn, string airServerIP);
    }
}