using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.Services
{
    public class NotificationsService
    {
        public NotificationsService()
        {
            this.db = new AppContext();
        }

        private AppContext db;

        public void DeleteNotificationById(int id)
        {
            if (!db.Notifications.Any(x => x.Id == id))
            {
                throw new ArgumentException("Notification does not exist.");
            }
            else
            {
                db.Notifications.Remove(db.Notifications.Single(x => x.Id == id));
                db.SaveChanges();
            }
        }

    }
}