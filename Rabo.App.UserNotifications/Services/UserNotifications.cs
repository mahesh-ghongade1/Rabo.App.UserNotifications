using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Rabo.App.UserNotifications.Models;

namespace Rabo.App.UserNotifications.Services
{
    public class UserNotifications
    {
        public readonly ILogger<UserNotifications> _logger;
        private readonly IServiceBusQueueProvider _serviceBusProvider;

        public UserNotifications(ILogger<UserNotifications> logger, IServiceBusQueueProvider serviceBusProvider)
        {
            _logger = logger;
            _serviceBusProvider = serviceBusProvider;
        }

        [Function("UserNotifications")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation($"Rabo.App.UserNotifications function executed at: {DateTime.Now}");

            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            string logTableName = Environment.GetEnvironmentVariable("LogTableName");
            string userNotificationTableName = Environment.GetEnvironmentVariable("UserNotificationTableName");
            string storedProcedureName = Environment.GetEnvironmentVariable("StoredProcedureName");
            string serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            string serviceBusQueueName = Environment.GetEnvironmentVariable("ServiceBusQueueName");

            try
            {
                // Get the last execution time from Azure Table Storage
                DateTime lastExecutionDate = GetLastExecutionTimeFromStorage(connectionString, logTableName);

                // Update last execution date time for initial first run if no last execution date is null
                if (lastExecutionDate == DateTime.MinValue)
                    UpdateLastExecutionTimeInStorage(connectionString, logTableName, DateTime.Now);

                List<UserNotification> userRecords = new List<UserNotification>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@LastExecutionDate", lastExecutionDate));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserNotification userRecord = new UserNotification
                                {
                                    RecordId = Convert.ToInt32(reader["RecordId"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    UserName = reader["UserName"].ToString(),
                                    UserEmail = reader["UserEmail"].ToString(),
                                    DataValue = reader["DataValue"].ToString(),
                                    NotificationFlag = Convert.ToBoolean(reader["NotificationFlag"]),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : null,
                                    IsModified = Convert.ToBoolean(reader["IsModified"])
                                };

                                userRecords.Add(userRecord);
                            }
                        }
                    }

                    if (userRecords?.Count > 0)
                    {
                        foreach (UserNotification notification in userRecords)
                        {
                            // Send the message using Azure Service Bus Queue for each newly created or updated record
                            await _serviceBusProvider.SendServiceBusMessage(notification, serviceBusConnectionString, serviceBusQueueName);
                            _logger.LogInformation($"Azure Service Bus message sent for RecordId : {notification.RecordId}");

                            //Update notification flag for this record once notification is sent
                            UpdateNotificationFlag(connectionString, userNotificationTableName, notification.RecordId);
                        }
                    }

                }

                // Update the last execution time in Azure Table Storage for the next execution
                UpdateLastExecutionTimeInStorage(connectionString, logTableName, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while running Rabo.App.UserNotifications function : {ex.Message}");
            }
        }

        private DateTime GetLastExecutionTimeFromStorage(string connectionString, string tableName)
        {
            DateTime recentDate = DateTime.MinValue;
            string query = $"SELECT MAX(LastExecutionDate) AS LastExecutionDate FROM {tableName}";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();

                    if (result != DBNull.Value)
                    {
                        recentDate = Convert.ToDateTime(result);
                        _logger.LogInformation("The last execution date is: " + recentDate);
                    }
                    else
                    {
                        _logger.LogError("No records found for last execution date.");
                    }
                }
            }
            return recentDate;
        }

        private void UpdateLastExecutionTimeInStorage(string connectionString, string tableName, DateTime lastExecutionTime)
        {
            try
            {
                // Create a SQL query with parameters to prevent SQL injection
                string insertQuery = $"INSERT INTO {tableName} VALUES (GETDATE())";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Execute the query
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation($"New record for last execution date is added successfully in {tableName} table.");
                        }
                        else
                        {
                            _logger.LogError($"Error occured while adding new record for last execution date in {tableName} table.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while updating LastExecutionDate column: {ex.Message}");
            }
        }

        private void UpdateNotificationFlag(string connectionString, string userNotificationTableName, int recordId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL update command
                    string updateCommandText = $"UPDATE {userNotificationTableName} SET NotificationFlag = 1 WHERE RecordId = {recordId}";

                    using (SqlCommand command = new SqlCommand(updateCommandText, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        _logger.LogInformation($"NotificationFlag updated successfully for RecordId: {recordId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while updating NotificationFlag column: {ex.Message}");
            }
        }
    }
}