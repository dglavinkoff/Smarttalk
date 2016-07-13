using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SmartTalk.Services
{
    public class AccountsService
    {
        public AccountsService()
        {
            this.db = new AppContext();
            this.groupService = new GroupsService();
        }

        private AppContext db;
        private GroupsService groupService;
        /// <summary>
        /// Gets the hash of password string.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string GetPasswordHash(string password)
        {
            var sha256 = new SHA256Managed();
            var bytes = UTF8Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Determines whether username already exists in the database.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool UsernameExist(string username)
        {
            if (db.Users.Any(x => x.Username == username))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Registers new user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        public int RegisterUser(string username, string firstname, string lastname, string email, string password, string role)
        {
            var user = new User
            {
                Username = username,
                Firstname = firstname,
                Lastname = lastname,
                Email = email,
                Password = GetPasswordHash(password),
                Role = "Member"
            };
            db.Users.Add(user);
            db.SaveChanges();
            return user.Id;
        }

        /// <summary>
        /// Validates user's password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int ValidateUser(string username, string password)
        {
            if (!db.Users.Any(x => x.Username == username))
            {
                throw new ArgumentException("User does not exist.");
            }
            else if (db.Users.Single(x => x.Username == username).Password != GetPasswordHash(password))
            {
                throw new ArgumentException("Wrong password.");
            }
            else
            {
                return db.Users.Single(x => x.Username == username).Id;
            }
        }

        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public User GetUserById(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("User does not exist.");
            }
            else
            {
                return db.Users.Single(x => x.Id == id);
            }
        }

        /// <summary>
        /// Deletes user by id.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteUser(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("User does not exist.");
            }
            else
            {
                //Remove the user and all related content.
                var userToRemove = GetUserById(id);
                foreach (var user in userToRemove.MyFavorites)
                {
                    GetUserById(user.Id).MyFollowers.Remove(userToRemove);
                }
                foreach (var user in userToRemove.MyFollowers)
                {
                    GetUserById(user.Id).MyFavorites.Remove(userToRemove);
                }
                foreach (var group in db.Groups.Where(x => x.Members.Contains(userToRemove)))
                {
                    groupService.GetGroupById(group.Id).Members.Remove(userToRemove);
                }
                groupService.DeleteGroup(db.Groups.Single(x => x.GroupLeader.Id == userToRemove.Id).Id);
                for (int i = 0; i < userToRemove.Notifications.Count; i++)
                {
                    db.Notifications.Remove(userToRemove.Notifications[i]);
                }
                db.Users.Remove(this.GetUserById(id));
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the top ten users with most followers.
        /// </summary>
        /// <returns></returns>
        public List<User> GetTopTenUsers()
        {
            return new List<User>((from user in db.Users
                                   orderby user.MyFollowers.Count descending
                                   select user).Take(10));
        }

        /// <summary>
        /// Searches for users whose username cointains the given substring.
        /// </summary>
        /// <param name="usernameSubstring"></param>
        /// <returns></returns>
        public List<User> SearchUsers(string usernameSubstring)
        {
            return new List<User>((from user in db.Users
                                   where user.Username.Contains(usernameSubstring)
                                   select user));
        }

        public List<Notification> GetNotifications(int id)
        {
            return db.Users.Single(x => x.Id == id).Notifications.ToList();
        }

        /// <summary>
        /// Adds user to favorites to a given user and to followers to the followed user.
        /// </summary>
        /// <param name="followedId"></param>
        /// <param name="followerId"></param>
        public void AddUserToFavorites(int followed)
        {
            int loggedId = (int)HttpContext.Current.Session["Id"];
            if (!db.Users.Any(x => x.Id == followed) || !db.Users.Any(x => x.Id == loggedId))
            {
                throw new ArgumentException("User does not exist.");
            }
            if (db.Users.Single(x => x.Id == followed).BannedUsers.Contains(db.Users.Single(x => x.Id == loggedId)))
            {
                throw new ArgumentException("This user has banned you as a follower.");
            }
            if (followed == loggedId)
            {
                throw new ArgumentException("You cannot follow yourself.");
            }
            else
            {
                db.Users.Single(x => x.Id == followed).MyFollowers.Add(db.Users.Single(x => x.Id == loggedId));
                db.Users.Single(x => x.Id == loggedId).MyFavorites.Add(db.Users.Single(x => x.Id == followed));
                db.SaveChanges();
            }
        }

        public void BannUser(int bannedId)
        {
            int loggedId = (int)HttpContext.Current.Session["Id"];
            if (!db.Users.Any(x => x.Id == bannedId) || !db.Users.Any(x => x.Id == loggedId))
            {
                throw new ArgumentException("User does not exist.");
            }
            else
            {
                var banner = db.Users.Single(x => x.Id == loggedId);
                var banned = db.Users.Single(x => x.Id == bannedId);
                if (banner.MyFollowers.Contains(banned))
                {
                    banner.MyFollowers.Remove(banned);
                }
                db.Users.Single(x => x.Id == loggedId).BannedUsers.Add(db.Users.Single(x => x.Id == bannedId));
                db.SaveChanges();
            }
        }

        public void PromoteUser(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("User does not exist.");
            }
            User user = db.Users.Single(x => x.Id == id);
            if (user.Role.ToString() == "Moderator")
            {
                throw new ArgumentException("This user is already moderator.");
            }
            if (user.Role.ToString() == "Admin")
            {
                throw new ArgumentException("This user is administrator.");
            }
            else
            {
                user.Role = "Moderator";
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "You have been promoted to moderator."
                });
                db.SaveChanges();
            }
        }

        public void DemoteUser(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("User does not exist.");
            }
            User user = db.Users.Single(x => x.Id == id);
            if (user.Role.ToString() == "Admin")
            {
                throw new ArgumentException("This user is administrator and cannot be demoted.");
            }
            if (user.Role.ToString() != "Moderator")
            {
                throw new ArgumentException("This user is not a moderator! Click to go to moderator panel.");
            }
            else
            {
                user.Role = "Member";
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "You have been demoted to member."
                });
            }
        }

        public void UnfollowUser(int unfollowedId)
        {
            User loggedUser = db.Users.Single(x => x.Id == (int)HttpContext.Current.Session["Id"]);
            if (!db.Users.Any(x => x.Id == unfollowedId))
            {
                throw new ArgumentException("User does not exist.");
            }
            if (!loggedUser.MyFavorites.Any(x => x.Id == unfollowedId))
            {
                throw new ArgumentException("You are not following this user.");
            }
            else
            {
                User unfollowedUser = db.Users.Single(x => x.Id == unfollowedId);
                loggedUser.MyFavorites.Remove(unfollowedUser);
                unfollowedUser.MyFollowers.Remove(loggedUser);
                unfollowedUser.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = loggedUser.Username + " unfollowed you."
                });
                db.SaveChanges();
            }
        }

        public List<User> GetAllModerators()
        {
            return db.Users.Where(x => x.Role == "Moderator").ToList();
        }

        public void Unbann(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("User does not exist.");
            }
            else
            {
                User loggedUser = db.Users.Single(x => x.Id == (int)HttpContext.Current.Session["Id"]);
                User unbannedUser = db.Users.Single(x => x.Id == id);
                if (!loggedUser.BannedUsers.Contains(unbannedUser))
                {
                    throw new ArgumentException("You haven't banned this user.");
                }
                else
                {
                    loggedUser.BannedUsers.Remove(unbannedUser);
                }
            }
        }

    }
}