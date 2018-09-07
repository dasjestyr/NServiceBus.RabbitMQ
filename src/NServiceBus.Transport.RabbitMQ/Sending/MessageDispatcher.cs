namespace NServiceBus.Transport.RabbitMQ
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Extensibility;

    class MessageDispatcher : IDispatchMessages
    {
        readonly ChannelProvider channelProvider;

        public MessageDispatcher(ChannelProvider channelProvider)
        {
            this.channelProvider = channelProvider;
        }

        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, ContextBag context)
        {
            var channel = channelProvider.GetPublishChannel();

            try
            {
                var unicastTransportOperations = outgoingMessages.UnicastTransportOperations;
                var multicastTransportOperations = outgoingMessages.MulticastTransportOperations;

                var tasks = new List<Task>(unicastTransportOperations.Count + multicastTransportOperations.Count);

                foreach (var operation in unicastTransportOperations)
                {
                    tasks.Add(SendMessage(operation, channel));
                }

                foreach (var operation in multicastTransportOperations)
                {
                    tasks.Add(PublishMessage(operation, channel));
                }

                return tasks.Count == 1 ? tasks[0] : Task.WhenAll(tasks);
            }
            finally
            {
                channelProvider.ReturnPublishChannel(channel);
            }
        }

        Task SendMessage(UnicastTransportOperation transportOperation, ConfirmsAwareChannel channel)
        {
            var message = transportOperation.Message;

            var properties = channel.CreateBasicProperties();
            properties.Fill(message, transportOperation.DeliveryConstraints, out var destination);

            return channel.SendMessage(destination ?? transportOperation.Destination, message, properties);
        }

        Task PublishMessage(MulticastTransportOperation transportOperation, ConfirmsAwareChannel channel)
        {
            var message = transportOperation.Message;
            
            var properties = channel.CreateBasicProperties();
            properties.Fill(message, transportOperation.DeliveryConstraints, out _);

            // choose overload based on some other information

            // PROBLEM ... how can we create an extention that overloads the Publish(message) interface with something like Publish(topic, key, message)???

            return channel.PublishMessage(transportOperation.MessageType, message, properties);
        }
    }

    /// <summary>
    /// Syntactic suger for <see cref="T:NServiceBus.IPipelineContext"/>
    /// </summary>
    public static class IPipelineContextTopicExtentions
    {
        /// <summary>
        /// Publishes a message to the specified topic with the provided routing key.
        /// </summary>
        /// <param name="context">Object being extended</param>
        /// <param name="message">The message that will be sent to the topic.</param>
        /// <param name="routingInfo">The routing info.</param>
        public static async Task Publish(this IPipelineContext context, object message, TopicRoutingInfo routingInfo)
        {
            var options = new PublishOptions();

            // The ContextBag seems to be dropped by the core library, so we need to use headers to get these values all the way to the publish implementation
            options.SetHeader(TopicRoutingInfo.DestinationTopicOptionsHeaderName, routingInfo.Destination);
            options.SetHeader(TopicRoutingInfo.RoutingKeyOptionsHeaderName, routingInfo.RoutingKey);
            
            await context.Publish(message, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Instantiates a message of type T and sends it to the specified topic using the provided routing key.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="context">Object being extended</param>
        /// <param name="messageConstructor">An action that will initialize the properties of the message.</param>
        /// <param name="routingInfo">The routing info.</param>
        public static async Task Publish<T>(this IPipelineContext context, Action<T> messageConstructor, TopicRoutingInfo routingInfo)
        {
            var options = new PublishOptions();

            // The ContextBag seems to be dropped by the core library, so we need to use headers to get these values all the way to the publish implementation
            options.SetHeader(TopicRoutingInfo.DestinationTopicOptionsHeaderName, routingInfo.Destination);
            options.SetHeader(TopicRoutingInfo.RoutingKeyOptionsHeaderName, routingInfo.RoutingKey);

            await context.Publish(messageConstructor, options).ConfigureAwait(false); // do overloads
        }
    }

    /// <summary>
    /// Provides routing information for topic-bound messages.
    /// </summary>
    public class TopicRoutingInfo
    {
        /// <summary>
        /// The options header name for the routing key
        /// </summary>
        public const string RoutingKeyOptionsHeaderName = "routing-key";
        /// <summary>
        /// The options header name for the destination topic
        /// </summary>
        public const string DestinationTopicOptionsHeaderName = "destination-topic";

        /// <summary>
        /// The destination topic name.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// The routing key to be used.
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TopicRoutingInfo"/>
        /// </summary>
        public TopicRoutingInfo(){}

        /// <summary>
        /// Initializes a new instance of <see cref="TopicRoutingInfo"/>
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="routingKey"></param>
        public TopicRoutingInfo(string destination, string routingKey)
        {
            Destination = destination;
            RoutingKey = routingKey;
        }
    }
}
