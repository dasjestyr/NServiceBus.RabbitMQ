namespace Notifications
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Transport.RabbitMQ;
    using Shared;

    class Notifications
    {
        public async Task RunTest()
        {
            Console.Title = "Notifications";
            var config = new EndpointConfiguration("Notifications");
            config.UsePersistence<LearningPersistence>();
            config.EnableInstallers();

            var trans = config.UseTransport<RabbitMQTransport>();
            trans.ConnectionString("host=rabbitmq;virtualhost=/;usetls=false;username=user;password=bitnami");
            trans.UseTopicRoutingTopology()
                .DeclareTopic("analytics")
                .DeclareTopic("notifications");

            await Endpoint.Start(config);

            // stay alive
            Task.Delay(Timeout.Infinite).GetAwaiter().GetResult();
        }

        static void Main(string[] args)
        {
            var notifications = new Notifications();
            notifications.RunTest().GetAwaiter().GetResult();
        }
    }

    public class SendNotificationHandler : IHandleMessages<SendNotification>
    {
        public async Task Handle(SendNotification message, IMessageHandlerContext context)
        {
            var key = $"{context.ReplyToAddress.ToLower()}.notifications.response";
            Console.WriteLine($"Simulated sending message {message.Id}. Replying to {key}");

            var responseRouting = new TopicRoutingInfo("notifications", key);
            await context.Publish<NotificationSent>(e =>
            {
                e.RequestId = message.Id;
                e.NotificationId = Guid.NewGuid();
            }, responseRouting);

            var analyticRouting = new TopicRoutingInfo("analytics", key);
            await context.Publish<NotificationActivity>(e =>
            {
                e.ActivityDate = DateTimeOffset.UtcNow;
                e.ActivityType = ActivityType.NotificationSent;
            }, analyticRouting);
        }
    }
}
