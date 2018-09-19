namespace NServiceBus.Transport.RabbitMQ.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::RabbitMQ.Client;
    
    /// <summary>
    /// Provides a means to manage topics on RMQ.
    /// </summary>
    public class TopicManager
    {
        bool isInitialized;
        List<Action<IModel>> createTopics = new List<Action<IModel>>();
        List<Action<IModel, IEnumerable<string>>> createSubscriptions = new List<Action<IModel, IEnumerable<string>>>();

        /// <summary>
        /// Initializes the topic manager to setup topic exchanges.
        /// </summary>
        /// <param name="channel">The <see cref="T:RabbitMQ.Client.IModel" /></param>
        /// <param name="addresses">List of bound addresses.</param>
        internal void Initialize(IModel channel, IEnumerable<string> addresses)
        {
            if (isInitialized)
                throw new Exception("Topic manager is already initialized.");

            var addressList = addresses.ToList();
            foreach (var topic in createTopics)
                topic(channel);

            foreach (var subscription in createSubscriptions)
                subscription(channel, addressList);

            // just to get these lists of function pointers out of memory
            createTopics = null;
            createSubscriptions = null;
            isInitialized = true;
        }

        /// <summary>
        /// Creates a topic but does not subscribe to it. Use this to create a topic that you will be publishing to but not consuming.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="arguments">Arguments</param>
        /// <returns></returns>
        public TopicManager DeclareTopic(string topicName, IDictionary<string, object> arguments = null)
        {
            createTopics.Add(channel => channel.ExchangeDeclare(topicName, ExchangeType.Topic, true, false, arguments));
            return this;
        }

        /// <summary>
        /// Ensures that a topic exists and that you are subscribed to it. A subscription will bind your dedicated fanout exchange 
        /// to the topic exchange using the provided routing key pattern.
        /// </summary>
        /// <param name="topicName">The name of the topic.</param>
        /// <param name="routingKeyPattern">The routing key pattern used to receive specific messages from the topic.</param>
        /// <param name="arguments">Arguments</param>
        /// <returns></returns>
        public TopicManager Subscribe(string topicName, string routingKeyPattern, IDictionary<string, object> arguments = null)
        {
            // ensure the topic exists and then bind our exchange to that topic exchange using the routing key pattern
            // by convention, all addresses will have their own fanout exchange and corresponding queue with the same name

            createSubscriptions.Add((channel, addresses) =>
            {
                channel.ExchangeDeclare(topicName, ExchangeType.Topic, true, false, arguments);
                foreach (var address in addresses)
                {
                    channel.ExchangeBind(address, topicName, routingKeyPattern, arguments);
                }
            });

            return this;
        }
    }
}