using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;

namespace cs3750LMS.Models.Repository
{
    public interface INotificationRepository
    {
        Notification GetNotification(int Id);
        IEnumerable<Notification> GetAllNotification();
        IEnumerable<Notification> GetAllUserNotifications(int userID);
        Notification Add(Notification notification);
        Notification Update(Notification notification);
        bool DeleteNotification(Notification notification);

    }
}
