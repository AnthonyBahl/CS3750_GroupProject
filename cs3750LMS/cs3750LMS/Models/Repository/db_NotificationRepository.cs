using cs3750LMS.Models.entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.Repository
{
    public class db_NotificationRepository : INotificationRepository
    {
        private readonly cs3750Context context;

        public db_NotificationRepository(cs3750Context context)
        {
            this.context = context;
        }

        public Notification Add(Notification notification)
        {
            context.Notifications.Add(notification);
            context.SaveChanges();
            return notification;
        }

        public Notification DeleteNotification(int id)
        {
            Notification notification = context.Notifications.Find(id);
            if(notification != null)
            {
                context.Notifications.Remove(notification);
                context.SaveChanges();
            }
            return notification;
        }

        public IEnumerable<Notification> GetAllNotification()
        {
            return context.Notifications;
        }

        public IEnumerable<Notification> GetAllUserNotifications(int userID)
        {
          return context.Notifications.Where(n => n.RecipientID == userID);

        }

        public Notification GetNotification(int Id)
        {
            return context.Notifications.Find(Id);
        }

        public Notification Update(Notification changedNoti)
        {
          var notification = context.Notifications.Attach(changedNoti);
            notification.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return changedNoti;
        }
    }
}
