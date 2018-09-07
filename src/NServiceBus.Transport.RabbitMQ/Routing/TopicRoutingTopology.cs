using System;
using System.Collections.Generic;

namespace NServiceBus.Transport.RabbitMQ.Routing
{
    using System.Linq;
    using global::RabbitMQ.Client;

    class TopicRoutingTopology : IRoutingTopology
    {
        const string RoutingKeyHeaderName = "routingKey";
        readonly bool useDurableExchanges;

        public TopicRoutingTopology(bool useDurableExchanges)
        {
            this.useDurableExchanges = useDurableExchanges;
        }

        public void SetupSubscription(IModel channel, Type type, string subscriberName)
        {
            // at this stage, we are only interested in our own queue and exchange
            channel.ExchangeBind(subscriberName, subscriberName, string.Empty);
        }

        public void TeardownSubscription(IModel channel, Type type, string subscriberName)
        {
            channel.ExchangeUnbind(subscriberName, subscriberName, string.Empty, null);
        }

        public void Publish(IModel channel, Type type, OutgoingMessage message, IBasicProperties properties)
        {
            if (ContainsTopicRoutingInformation(message, out var topicName, out var routingKey))
            {
                channel.BasicPublish(topicName, routingKey, true, properties, message.Body);
            }
            else
            {
                channel.BasicPublish(
                    ExchangeName(type),
                    routingKey,
                    true,
                    properties,
                    message.Body);
            }
        }

        static bool ContainsTopicRoutingInformation(OutgoingMessage message, out string topicName, out string routingKey)
        {
            var hasTopic = message.Headers.TryGetValue(TopicRoutingInfo.DestinationTopicOptionsHeaderName, out var destination) && destination != null;
            var hasRoutingKey = message.Headers.TryGetValue(TopicRoutingInfo.RoutingKeyOptionsHeaderName, out var key) && key != null;

            topicName = destination;
            routingKey = key;

            return hasTopic && hasRoutingKey;
        }

        static string ExchangeName(Type type) => type.Namespace + ":" + type.Name;

        public void Publish(IModel channel, string topicName, string routingKey, OutgoingMessage message, IBasicProperties properties)
        {
            channel.BasicPublish(topicName, routingKey, true, properties, message.Body);
        }

        public void Send(IModel channel, string address, OutgoingMessage message, IBasicProperties properties)
        {
            var routingKey = message.Headers.ContainsKey(RoutingKeyHeaderName)
                ? message.Headers[RoutingKeyHeaderName]
                : null;

            channel.BasicPublish(address, routingKey, true, properties, message.Body);
        }

        public void RawSendInCaseOfFailure(IModel channel, string address, byte[] body, IBasicProperties properties)
        {
            channel.BasicPublish(address, string.Empty, true, properties, body);
        }

        public void Initialize(IModel channel, IEnumerable<string> receivingAddresses, IEnumerable<string> sendingAddresses)
        {
            // create our queue and our exchange
            foreach (var address in receivingAddresses.Concat(sendingAddresses))
            {
                channel.QueueDeclare(address, useDurableExchanges, false, false, null);
                channel.ExchangeDeclare(address, ExchangeType.Fanout, useDurableExchanges);
                channel.QueueBind(address, address, string.Empty);
            }
        }

        public void BindToDelayInfrastructure(IModel channel, string address, string deliveryExchange, string routingKey)
        {
            channel.ExchangeBind(address, deliveryExchange, routingKey);
        }
    }
}
