using System.Collections.Generic;

namespace SharedLib.Models
{
    public class SuccessMessage
    {
        public long SuccessMessageId { get; set; }
        public string CategoryId { get; set; }
        public string MessageType { get; set; }
        public string MessageTemplate { get; set; }
        public string SubscriberType { get; set; }
        public IDictionary<string, string> PartialContents { get; set; }

    }
}
