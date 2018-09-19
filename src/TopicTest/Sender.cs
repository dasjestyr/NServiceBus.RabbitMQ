namespace Sender
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Transport.RabbitMQ;
    using Shared;

    class Sender
    {
        async Task RunTest()
        {
            Console.Title = "Sender";
            var config = new EndpointConfiguration("Sender");
            config.UsePersistence<LearningPersistence>();
            config.EnableInstallers();

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq;virtualhost=/;usetls=false;username=user;password=bitnami");
            transport
                .UseTopicRoutingTopology()
                .DeclareTopic("analytics") // to ensure we can write to it
                .Subscribe("notifications", "sender.notifications.response"); // only responses from "sender"

            var session = await Endpoint.Start(config);

            while (true)
            {
                await session.Send<SendNotification>("Notifications", c => { c.Id = Guid.NewGuid(); });

                // to analytics topic
                var topicInfo = new TopicRoutingInfo("Analytics", "notification.confirmation");
                await session.Publish<NotificationActivity>(e =>
                {
                    e.ActivityDate = DateTimeOffset.UtcNow;
                    e.ActivityType = ActivityType.NotificationRequested;
                }, topicInfo);

                await Task.Delay(1000);
            }
        }

        static void Main(string[] args)
        {
            var sender = new Sender();
            sender.RunTest().GetAwaiter().GetResult();
        }
    }

    public class NotificationSentHandler : IHandleMessages<NotificationSent>
    {
        public async Task Handle(NotificationSent message, IMessageHandlerContext context)
        {
            Console.Out.WriteLine($"Received notification sent {message.NotificationId}. Writing to topic...");
            var topicInfo = new TopicRoutingInfo("analytics", "notification.confirmation");
            await context.Publish<NotificationActivity>(e =>
            {
                e.ActivityDate = DateTimeOffset.UtcNow;
                e.ActivityType = ActivityType.NotificationConfirmed;
            } , topicInfo);
        }
    }
}
