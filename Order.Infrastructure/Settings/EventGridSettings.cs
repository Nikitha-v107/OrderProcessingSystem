namespace Order.Infrastructure.Settings
{
    public class EventGridSettings
    {
        public required string TopicEndpoint { get; set; }
        public required string TopicKey { get; set; }
    }
}
