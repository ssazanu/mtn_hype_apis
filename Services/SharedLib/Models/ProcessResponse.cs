namespace SharedLib.Models
{
    public class ProcessResponse
    {
        public bool IsSuccessful { get; set; }
        public string ResponseMessage { get; set; }
        public string ResponseCode { get; set; }
        public int UssdSuccessMenuItem { get; set; }
    }
}
