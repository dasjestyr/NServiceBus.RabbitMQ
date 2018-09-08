
namespace NServiceBus.Transport.RabbitMQ.TransportTests
{
    class DeleteMe
    {
        public void Usage()
        {
            var config = new EndpointConfiguration("");
            var transport = config.UseTransport<RabbitMQTransport>();
            transport.UseTopicRoutingTopology()
                .CreateTopic("stats-1")
                .CreateTopic("stats-2")
                .Subscribe("topic-1", "topic.1")
                .Subscribe("topic-2", "#");

        }
    }
}
