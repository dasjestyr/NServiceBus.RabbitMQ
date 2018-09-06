using System;
using System.Collections.Generic;

namespace NServiceBus.Transport.RabbitMQ.Routing
{
    using System.Linq;
    using global::RabbitMQ.Client;

    class TopicRoutingTopology : IRoutingTopology
    {
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
            var routingKey = message.Headers.ContainsKey("routingKey") 
                ? message.Headers["routingKey"] 
                : null;

            // why do i need a type? Isn't it just supposed to be a destination?

            channel.BasicPublish(
                "TODO: TOPIC NAME", // may need to derive this somehow, or make an extension
                routingKey, 
                true, 
                properties,
                message.Body);
        }

        public void Send(IModel channel, string address, OutgoingMessage message, IBasicProperties properties)
        {
            var routingKey = message.Headers.ContainsKey("routingKey")
                ? message.Headers["routingKey"]
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
