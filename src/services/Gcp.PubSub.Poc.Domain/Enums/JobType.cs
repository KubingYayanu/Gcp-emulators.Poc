namespace Gcp.PubSub.Poc.Domain.Enums
{
    public enum JobType
    {
        // one to one
        PublisherOneToOne = 1,
        SubscriberOneToOne = 2,
        // one to many
        PublisherOneToMany = 3,
        SubscriberOneToMany1 = 4,
        SubscriberOneToMany2 = 5,
        // filter by attribute
        PublisherFilterByAttribute = 6,
        SubscriberFilterByAttributeDog = 7,
        SubscriberFilterByAttributeCat = 8
    }
}