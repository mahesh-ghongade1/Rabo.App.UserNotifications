using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Rabo.App.UserNotifications.Models;

namespace Rabo.App.UserNotifications.Services
{
    public class ServiceBusQueueProvider : IServiceBusQueueProvider
    {
        private ILogger<ServiceBusQueueProvider> _logger;

        public ServiceBusQueueProvider(ILogger<ServiceBusQueueProvider> logger)
        {
            _logger = logger;
        }
        public async Task SendServiceBusMessage(UserNotification notification, string serviceBusConnectionString, string serviceBusQueueName)
        {
            // Create a ServiceBusClient
            await using (ServiceBusClient client = new ServiceBusClient(serviceBusConnectionString))
            {
                // Create a sender for the queue
                ServiceBusSender sender = client.CreateSender(serviceBusQueueName);

                try
                {
                    // Prepare message properties
                    var messageProperties = new UserNotification
                    {
                        UserId = notification.UserId,
                        UserName = notification.UserName,
                        UserEmail = notification.UserEmail,
                        DataValue = notification.DataValue
                    };

                    // Create a ServiceBusMessage with the JSON body
                    ServiceBusMessage message = new ServiceBusMessage(new BinaryData(System.Text.Json.JsonSerializer.Serialize(messageProperties)));

                    // Add user-defined properties to the message
                    message.ApplicationProperties.Add("UserId", messageProperties.UserId);
                    message.ApplicationProperties.Add("UserName", messageProperties.UserName);
                    message.ApplicationProperties.Add("UserEmail", messageProperties.UserEmail);
                    message.ApplicationProperties.Add("DataValue", messageProperties.DataValue);

                    // Send the message to the Service Bus queue
                    await sender.SendMessageAsync(message);

                    _logger.LogInformation($"Message sent successfully on {serviceBusQueueName} queue.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occured while sending message : {ex.Message}");
                }
            }
        }

    }
}