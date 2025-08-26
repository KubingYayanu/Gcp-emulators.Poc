namespace Gcp.PubSub.Poc.Domain.Queues.Options
{
    public class WorkerQueueOptions
    {
        public const string SectionName = "Queues:Worker";

        public QueueSection PublisherA { get; set; }
    }
}