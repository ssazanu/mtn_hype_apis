using SharedLib.Models;

namespace SharedLib.Interfaces
{
    public interface IMessageBuilder
    {
        SmsContents GenerateProductSuccessMessage(MessageOption messageItem);
    }
}