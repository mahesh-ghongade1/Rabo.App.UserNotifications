namespace Rabo.App.UserNotifications.Models
{
    public class UserNotification
    {
        public int RecordId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string DataValue { get; set; }
        public bool NotificationFlag { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsModified { get; set; }
    }
}