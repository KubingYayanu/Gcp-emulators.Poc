namespace Gcp.PubSub.Poc.Domain.Queues.Options
{
    public class WorkerQueueOptions
    {
        public const string SectionName = "Queues:Worker";

        // one to one
        public QueueSection PublisherOneToOne { get; set; }

        public QueueSection SubscriberOneToOne { get; set; }

        // one to many
        public QueueSection PublisherOneToMany { get; set; }

        public QueueSection SubscriberOneToMany1 { get; set; }

        public QueueSection SubscriberOneToMany2 { get; set; }
        
        // filter by attribute
        public QueueSection PublisherFilterByAttribute { get; set; }

        public QueueSection SubscriberFilterByAttributeDog { get; set; }
        
        public QueueSection SubscriberFilterByAttributeCat { get; set; }
    }
}