using System;
using System.Collections.Generic;

namespace NServiceBus.Transport.RabbitMQ.Routing
{
    using System.Linq;
    using global::RabbitMQ.Client;

    class TopicRoutingTopology : IRoutingTopology
    {
        readonly bool useDurableExchanges;
        readonly ITopicManager topicManager;

        public TopicRoutingTopology(bool useDurableExchanges, ITopicManager topicManager)
        {
            this.useDurableExchanges = useDurableExchanges;
            this.topicManager = topicManager;
        }

        public void SetupSubscription(IModel channel, Type type, string subscriberName)
        {
            // at this stage, we are only interested in our own queue and exchange
            // this is subscribe by type which we dont need
            return;
        }

        public void TeardownSubscription(IModel channel, Type type, string subscriberName)
        {
            // we're not subscribed by type, so no need
            return;
        }

        public void Publish(IModel channel, Type type, OutgoingMessage message, IBasicProperties properties)
        {
            TryGetRoutingInfo(message, out var topicName, out var routingKey);
            channel.BasicPublish(topicName, routingKey, true, properties, message.Body);
        }

        static bool TryGetRoutingInfo(OutgoingMessage message, out string topicName, out string routingKey)
        {
            var hasTopic = message.Headers.TryGetValue(TopicRoutingInfo.DestinationTopicOptionsHeaderName, out var destination) && destination != null;
            var hasRoutingKey = message.Headers.TryGetValue(TopicRoutingInfo.RoutingKeyOptionsHeaderName, out var key) && key != null;

            topicName = destination;
            routingKey = key;

            return hasTopic && hasRoutingKey;
        }

        public void Publish(IModel channel, string topicName, string routingKey, OutgoingMessage message, IBasicProperties properties)
        {
            channel.BasicPublish(topicName, routingKey, true, properties, message.Body);
        }

        public void Send(IModel channel, string address, OutgoingMessage message, IBasicProperties properties)
        {
            channel.BasicPublish(address, string.Empty, true, properties, message.Body);
        }

        public void RawSendInCaseOfFailure(IModel channel, string address, byte[] body, IBasicProperties properties)
        {
            channel.BasicPublish(address, string.Empty, true, properties, body);
        }

        public void Initialize(IModel channel, IEnumerable<string> receivingAddresses, IEnumerable<string> sendingAddresses)
        {
            var addresses = receivingAddresses.Concat(sendingAddresses).ToList();
            
            // create our queue and our exchange
            foreach (var address in addresses)
            {
                channel.QueueDeclare(address, useDurableExchanges, false, false, null);
                channel.ExchangeDeclare(address, ExchangeType.Fanout, useDurableExchanges);
                channel.QueueBind(address, address, string.Empty);
            }

            topicManager.Initialize(channel, addresses);
        }

        public void BindToDelayInfrastructure(IModel channel, string address, string deliveryExchange, string routingKey)
        {
            channel.ExchangeBind(address, deliveryExchange, routingKey);
        }
    }
}
