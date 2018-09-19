using System;

namespace Analytics
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus;
    using Shared;

    class Analytics
    {
        public async Task RunTest()
        {
            Console.Title = "Analytics";
            var config = new EndpointConfiguration("Analytics");
            config.UsePersistence<LearningPersistence>();
            config.EnableInstallers();

            var transport = config.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=rabbitmq;virtualhost=/;usetls=false;username=user;password=bitnami");
            transport
                .UseTopicRoutingTopology()
                .Subscribe("analytics", "#"); // everything

            await Endpoint.Start(config);
            Task.Delay(Timeout.Infinite).GetAwaiter().GetResult();
        }

        static void Main(string[] args)
        {
            var analytics = new Analytics();
            analytics.RunTest().GetAwaiter().GetResult();
        }
    }
    
    public class NotificationActivityHandler : IHandleMessages<NotificationActivity>
    {
        static NotificationHistory _audit = new NotificationHistory();

        public Task Handle(NotificationActivity message, IMessageHandlerContext context)
        {
            var cId = context.MessageHeaders[Headers.ConversationId];
            EnsureRecord(cId);

            switch (message.ActivityType)
            {
                case ActivityType.NotificationRequested:
                    _audit[cId].RequestDate = message.ActivityDate;
                    break;
                case ActivityType.NotificationSent:
                    _audit[cId].SentDate = message.ActivityDate;
                    break;
                case ActivityType.NotificationConfirmed:
                    _audit[cId].ConfirmDate = message.ActivityDate;
                    break;
            }
            _audit.PrintCompletionRate();
            return Task.CompletedTask;
        }

        private static void EnsureRecord(string conversationId)
        {
            if (!_audit.ContainsKey(conversationId))
                _audit.Add(conversationId, new NotificationAudit {RequestId = conversationId});
        }
    }
    public class NotificationAudit
    {
        public string RequestId { get; set; }

        public DateTimeOffset? RequestDate { get; set; }

        public DateTimeOffset? SentDate { get; set; }

        public DateTimeOffset? ConfirmDate { get; set; }

        public NotificationAudit() { }

        public NotificationAudit(string requestId, DateTimeOffset requestDate)
        {
            RequestId = requestId;
            RequestDate = requestDate;
        }
    }
    public class NotificationHistory : Dictionary<string, NotificationAudit>
    {
        public void PrintCompletionRate()
        {
            var total = Values.Count(v => v.RequestDate.HasValue);
            var confirmed = Values.Count(v => v.ConfirmDate.HasValue);
            if (total == 0)
                return;

            var ratio = confirmed / total;
            Console.Out.WriteLine($"Successful sends {ratio:P}");
        }
    }
}
