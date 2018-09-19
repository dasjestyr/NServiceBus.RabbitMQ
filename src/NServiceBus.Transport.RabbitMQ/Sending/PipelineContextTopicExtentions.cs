namespace NServiceBus.Transport.RabbitMQ
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Syntactic suger for <see cref="T:NServiceBus.IPipelineContext"/>
    /// </summary>
    public static class PipelineContextTopicExtentions
    {
        /// <summary>
        /// Publishes a message to the specified topic with the provided routing key.
        /// </summary>
        /// <param name="context">Object being extended</param>
        /// <param name="message">The message that will be sent to the topic.</param>
        /// <param name="routingInfo">The routing info.</param>
        public static async Task Publish(this IMessageSession context, object message, TopicRoutingInfo routingInfo)
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
        public static async Task Publish<T>(this IMessageSession context, Action<T> messageConstructor, TopicRoutingInfo routingInfo)
        {
            var options = new PublishOptions();

            // The ContextBag seems to be dropped by the core library, so we need to use headers to get these values all the way to the publish implementation
            options.SetHeader(TopicRoutingInfo.DestinationTopicOptionsHeaderName, routingInfo.Destination);
            options.SetHeader(TopicRoutingInfo.RoutingKeyOptionsHeaderName, routingInfo.RoutingKey);

            await context.Publish(messageConstructor, options).ConfigureAwait(false); // do overloads
        }

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
}