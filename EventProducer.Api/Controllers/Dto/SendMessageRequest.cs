namespace EventProducer.Api.Controllers.Dto
{
    public record SendMessageRequest(
            string Message,
            string Topic,
            string Source,
            string Subject
        );
}
