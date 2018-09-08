namespace NServiceBus.Transport.RabbitMQ.Routing
{
    using System.Collections.Generic;
    using global::RabbitMQ.Client;

    /// <summary>
    /// When implemented, provides a means to manage topics on RMQ.
    /// </summary>
    public interface ITopicManager
    {
        /// <summary>
        /// Creates a topic.
        /// </summary>
        /// <param name="topicName">Name of the topic</param>
        /// <param name="arguments">Arguments</param>
        ITopicManager CreateTopic(string topicName, IDictionary<string, object> arguments = null);

        /// <summary>
        /// Subscribes to a topic.
        /// </summary>
        /// <param name="topicName">The topic name</param>
        /// <param name="routingKeyPattern">The routing key pattern for the exchange to exchange binding.</param>
        /// <param name="arguments">Arguments</param>
        /// <returns></returns>
        ITopicManager Subscribe(string topicName, string routingKeyPattern, IDictionary<string, object> arguments = null);

        /// <summary>
        /// Initializes the topic manager to setup topic exchanges.
        /// </summary>
        /// <param name="channel">The <see cref="IModel"/></param>
        /// <param name="addresses">List of bound addresses.</param>
        void Initialize(IModel channel, IEnumerable<string> addresses);
    }
}