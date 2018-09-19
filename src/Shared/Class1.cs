using System;

namespace Shared
{
    using NServiceBus;
    public class SendNotification : ICommand
    {
        public Guid Id { get; set; }
    }

    public class NotificationSent : IEvent
    {
        public Guid RequestId { get; set; }

        public Guid NotificationId { get; set; }
    }

    public class NotificationActivity : IEvent
    {
        public DateTimeOffset ActivityDate { get; set; }

        public string ActivityType { get; set; }
    }

    public static class ActivityType
    {
        public const string NotificationRequested = "NotificationRequested";
        public const string NotificationSent = "NotificationSent";
        public const string NotificationConfirmed = "NotificationConfirmed";
    }
}
