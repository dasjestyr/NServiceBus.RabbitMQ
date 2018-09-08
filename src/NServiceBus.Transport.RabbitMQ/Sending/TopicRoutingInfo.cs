namespace NServiceBus.Transport.RabbitMQ
{
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