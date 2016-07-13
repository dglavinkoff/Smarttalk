using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmartTalk.Models
{
    public class User
    {
        public User() {
            this.myFavorites = new List<User>();
            this.myFollowers = new List<User>();
            this.notifications = new List<Notification>();
        }

        [Key]
        public int Id { get; set; }

        public string Password { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }
        
        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Role { get; set; }

        public virtual List<Notification> Notifications
        {
            get { return notifications; }
            set { notifications = value; }
        }

        public virtual ICollection<User> MyFavorites
        {
            get { return myFavorites; }
            set { myFavorites = value; }
        }


        public virtual ICollection<User> MyFollowers
        {
            get { return myFollowers; }
            set { myFollowers = value; }
        }

        public virtual List<User> BannedUsers
        {
            get { return bannedUsers; }
            set { bannedUsers = value; }
        }

        public virtual List<Group> MyGroups
        {
            get { return myGroups; }
            set { myGroups = value; }
        }

        public virtual List<Group> RequestedGroups
        {
            get { return requestedGroups; }
            set { requestedGroups = value; }
        }

        private ICollection<User> myFavorites;

        private List<Group> requestedGroups;

        private ICollection<User> myFollowers;

        private List<Group> myGroups;

        private List<Notification> notifications;

        private List<User> bannedUsers;
    }
}