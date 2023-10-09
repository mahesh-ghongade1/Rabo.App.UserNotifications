using Rabo.App.UserNotifications.Models;

namespace Rabo.App.UserNotifications.Services
{
    public interface IServiceBusQueueProvider
    {
        Task SendServiceBusMessage(UserNotification notification, string serviceBusConnectionString, string serviceBusQueueName);
    }
}