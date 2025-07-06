namespace EventProducer.Controllers.Dto
{
    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
