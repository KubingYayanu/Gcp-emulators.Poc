using Google.Cloud.PubSub.V1;
using Google.Protobuf;

namespace Gcp.PubSub.Poc.Helpers
{
    public class PubSubPublisher : IPubSubPublisher
    {
        private readonly IPubSubPublisherPool _publisherPool;

        public PubSubPublisher(IPubSubPublisherPool publisherPool)
        {
            _publisherPool = publisherPool;
        }

        public async Task<string> PublishAsync(
            PubSubTaskConfig config,
            PubSubPayload payload,
            CancellationToken cancellationToken = default)
        {
            var publisher = await _publisherPool.GetPublisherAsync(
                projectId: config.ProjectId,
                topicId: config.TopicId);

            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(payload.Message)
            };

            foreach (var attr in payload.Attributes)
            {
                pubsubMessage.Attributes[attr.Key] = attr.Value;
            }

            var messageId = await publisher.PublishAsync(pubsubMessage);
            return messageId;
        }
    }
}