using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace SmartTalk.Services
{
    public class DataService
    {
        public DataService()
        {
            this.db = new AppContext();
        }

        private AppContext db;

        //Moved
        #region AccountService

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
        /// Finds user by id.
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
                foreach (var user in userToRemove.MyFavorites) {
                    GetUserById(user.Id).MyFollowers.Remove(userToRemove);
                }
                foreach (var user in userToRemove.MyFollowers) {
                    GetUserById(user.Id).MyFavorites.Remove(userToRemove);
                }
                foreach (var question in db.Questions.Where(x => x.Author.Id == userToRemove.Id))
                {
                    db.Questions.Remove(question);
                }
                foreach (var answer in db.Answers.Where(x => x.Author.Id == userToRemove.Id))
                {
                    db.Answers.Remove(answer);
                }
                foreach (var comment in db.Comments.Where(x => x.Author.Id == userToRemove.Id))
                {
                    db.Comments.Remove(comment);
                }
                for (int i = 0; i < userToRemove.BannedUsers.Count; i++)
                {
                    userToRemove.BannedUsers.Remove(userToRemove.BannedUsers[i]);
                }
                var userGroups = db.Groups.Where(x => x.Members.Any(y => y.Username == userToRemove.Username));
                foreach (var group in userGroups) {
                    GetGroupById(group.Id).Members.Remove(userToRemove);
                }
                if (db.Groups.Any(x => x.GroupLeader.Id == userToRemove.Id))
                {
                    DeleteGroup(db.Groups.Single(x => x.GroupLeader.Id == userToRemove.Id).Id);
                }
                for (int i = 0; i < userToRemove.Notifications.Count; i++) {
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
        public Tuple<List<User>, int> GetTopTenUsers(int page)
        {
            var list = new List<User>((from user in db.Users
                                       orderby user.MyFollowers.Count descending
                                       select user));
            if ((list.Count / 10) + 1 < page)
            {
                page = 1;
            }
            return Tuple.Create(new List<User>(list.Skip((page - 1) * 10).Take(10).ToList()), list.Count);
        }

        /// <summary>
        /// Searches for users whose username cointains the given substring.
        /// </summary>
        /// <param name="usernameSubstring"></param>
        /// <returns></returns>
        public Tuple<List<User>, int> SearchUsers(string usernameSubstring, int page)
        {
            
            var list = new List<User>((from user in db.Users
                                   where user.Username.Contains(usernameSubstring)
                                   select user));
            if ((list.Count / 10) + 1 < page)
            {
                page = 1;
            }
            return Tuple.Create(new List<User>( list.Skip((page - 1) * 10).Take(10).ToList()), list.Count);
        }

        /// <summary>
        /// Gets all notifications for user by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
            if (db.Users.Single(x => x.Id == followed).BannedUsers.Contains(db.Users.Single(x => x.Id == loggedId))) {
                throw new ArgumentException("Този потребител ви е забранил да го следите.");
            }
            if (followed == loggedId)
            {
                throw new ArgumentException("Не можете да следите себе си.");
            }
            else
            {
                db.Users.Single(x => x.Id == followed).MyFollowers.Add(db.Users.Single(x => x.Id == loggedId));
                db.Users.Single(x => x.Id == loggedId).MyFavorites.Add(db.Users.Single(x => x.Id == followed));
                db.SaveChanges();
            }
        }

        public void BanUser(int bannedId)
        {
            int loggedId = (int)HttpContext.Current.Session["Id"];
            if (!db.Users.Any(x => x.Id == bannedId) || !db.Users.Any(x => x.Id == loggedId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            else
            {
                var banner = db.Users.Single(x => x.Id == loggedId);
                var banned = db.Users.Single(x => x.Id == bannedId);
                if (banner.MyFollowers.Contains(banned)) {
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
                throw new ArgumentException("Потребителят не съществува.");
            }
            User user = db.Users.Single(x => x.Id == id);
            if (user.Role.ToString() == "Moderator")
            {
                throw new ArgumentException("Този потребител вече е модератор.");
            }
            if (user.Role.ToString() == "Admin")
            {
                throw new ArgumentException("Този потребител е администратор.");
            }
            else
            {
                user.Role = "Moderator";
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "Вие бяхте повишен в модератор."
                });
                db.SaveChanges();
            }
        }

        public void DemoteUser(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            User user = db.Users.Single(x => x.Id == id);
            if (user.Role.ToString() == "Admin")
            {
                throw new ArgumentException("Този потребител е администратор и не може да бъде понижен.");
            }

            if (user.Role.ToString() != "Moderator")
            {
                throw new ArgumentException("Този потребител не е модератор.");
            }
            else
            {
                user.Role = "Member";
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "Вие бяхте понижен до обикновен потребител."
                });
                db.SaveChanges();
            }
        }

        public void UnfollowUser(int unfollowedId)
        {
            int loggedId = (int)HttpContext.Current.Session["Id"];
            User loggedUser = db.Users.Single(x => x.Id == loggedId);
            if (!db.Users.Any(x => x.Id == unfollowedId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            if (!loggedUser.MyFavorites.Any(x => x.Id == unfollowedId))
            {
                throw new ArgumentException("Вие не следите този потребител.");
            }
            else
            {
                User unfollowedUser = db.Users.Single(x => x.Id == unfollowedId);
                loggedUser.MyFavorites.Remove(unfollowedUser);
                unfollowedUser.MyFollowers.Remove(loggedUser);
                unfollowedUser.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = loggedUser.Username + " вече не ви следи."
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
                throw new ArgumentException("Потребителят не съществува.");
            }
            else
            {
                int loggedId = (int)HttpContext.Current.Session["Id"];
                User loggedUser = db.Users.Single(x => x.Id == loggedId);
                User unbannedUser = db.Users.Single(x => x.Id == id);
                if (!loggedUser.BannedUsers.Contains(unbannedUser))
                {
                    throw new ArgumentException("Не сте забранили на този потребител да ви следи.");
                }
                else
                {
                    loggedUser.BannedUsers.Remove(unbannedUser);
                    db.SaveChanges();
                }
            }
        }

        public void RequestNewPassword(string username)
        {
            if (!db.Users.Any(x => x.Username == username))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            else
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random random = new Random();
                string newPassword = new string(
                    Enumerable.Repeat(chars, 8)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());
                var user = db.Users.Single(x => x.Username == username);
                user.Password = GetPasswordHash(newPassword);
                //db.SaveChanges();

                var fromAddress = new MailAddress("smarttalkforum@gmail.com", "From Name");
                var toAddress = new MailAddress(db.Users.Single(x => x.Username == user.Username).Email , user.Firstname + " " + user.Lastname);
                string fromPassword = "qwerty426624";
                string subject = "Forgotten password";
                string body = "Your new password for Smart Talk is " + newPassword + " ."; 

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

            }
        }

        public string ChangePassword(int id, string oldPassword, string newPassword)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            var user = db.Users.Single(x => x.Id == id);
            if (user.Password != GetPasswordHash(oldPassword))
            {
                return "Грешна парола.";
            }
            else
            {
                user.Password = GetPasswordHash(newPassword);
                db.SaveChanges();
                return "Паролата беше сменена успешно.";
            }
        }

        #endregion

        //Moved
        #region CategoriesService

        /// <summary>
        /// Return a list with all active categories in the database.
        /// </summary>
        /// <returns></returns>
        public List<Category> GetAllActiveCategories()
        {
            return new List<Category>(db.Categories.Where(x => x.IsActive == true));
        }

        /// <summary>
        /// Creates new category.
        /// </summary>
        /// <param name="name"></param>
        public void CreateCategory(string name)
        {
            if (db.Categories.Any(x => x.Name == name))
            {
                throw new ArgumentException("Категорията вече съществува.");
            }
            else
            {
                db.Categories.Add(new Category
                {
                    Name = name,
                    IsActive = true
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes category by given id.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteCategory(int id)
        {
            if (!db.Categories.Any(x => x.Id == id))
            {
                throw new ArgumentException("Категорията не съществува.");
            }
            else
            {
                db.Categories.Remove(db.Categories.Single(x => x.Id == id));
                foreach (var question in db.Questions.Where(x => x.Id == id))
                {
                    db.Questions.Remove(question);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets all questions for given category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tuple<List<Question>, int> GetAllQuestions(int id, int page)
        {
            if (!db.Categories.Any(x => x.Id == id))
            {
                throw new ArgumentException("Категорията не съществува.");
            }
            else
            {
                List<Question> list = new List<Question>(db.Questions.Where(x => x.Category.Id == id && x.Group == null));
                if ((list.Count / 10) + 1 < page)
                {
                    page = 1;
                }
                return Tuple.Create(new List<Question>(list.Skip((page - 1) * 10).Take(10).ToList()), list.Count);
            }
        }

        /// <summary>
        /// Gets category from the database by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category GetCategoryById(int id)
        {
            return db.Categories.Single(x => x.Id == id);
        }


        public void RequestCategory(string name)
        {
            if (db.Categories.Any(x => x.Name == name && x.IsActive == true))
            {
                throw new ArgumentException("Категорията не съществува.");
            }
            if (db.Categories.Any(x => x.Name == name && x.IsActive == false))
            {
                throw new ArgumentException("Тази категория вече е заявена от друг потребител.");
            }
            else
            {
                db.Categories.Add(new Category
                {
                    Name = name,
                    IsActive = false
                });
                db.SaveChanges();
            }
        }

        public void ConfirmCategory(int id)
        {
            if (!db.Categories.Any(x => x.Id == id)) { throw new ArgumentException("Категорията не съществува."); }
            if (db.Categories.Single(x => x.Id == id).IsActive == true) { throw new ArgumentException("Категорията е активна."); }
            else
            {
                db.Categories.Single(x => x.Id == id).IsActive = true;
                db.SaveChanges();
            }
        }

        public List<Category> GetAllRequestedCategories()
        {
            return db.Categories.Where(x => x.IsActive == false).ToList();
        }

        public void DenyCategory(int id)
        {
            if (!db.Categories.Any(x => x.Id == id))
            {
                throw new ArgumentException("Тази категория не е била заявена.");
            }
            else
            {
                Category category = db.Categories.Single(x => x.Id == id);
                if (category.IsActive == true)
                {
                    throw new ArgumentException("Категорията е активна.");
                }
                else
                {
                    db.Categories.Remove(category);
                }
            }
        }

        #endregion

        //Moved
        #region GroupsService

        /// <summary>
        /// Gets the top ten groups with most members.
        /// </summary>
        /// <returns></returns>
        public Tuple<List<Group>, int> GetTopTenGroups(int page)
        {
            List<Group> list = new List<Group>((from gr in db.Groups
                                    orderby gr.Members.Count descending
                                    select gr));
            return Tuple.Create(new List<Group>(list.Skip((page - 1) / 10).Take(10).ToList()), list.Count);
        }

        /// <summary>
        /// Creates new group.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupLeader"></param>
        /// <returns></returns>
        public int CreateGroup(string name, User groupLeader)
        {
            if (db.Groups.Any(x => x.Name == name))
            {
                throw new ArgumentException("Името вече съществува.");
            }
            if (db.Groups.Any(x => x.GroupLeader.Id == groupLeader.Id))
            {
                throw new ArgumentException("Можете да бъдете лидер само на една група.");
            }
            else
            {
                var group = new Group
                {
                    Name = name,
                    GroupLeader = groupLeader
                };
                db.Groups.Add(group);
                db.SaveChanges();
                group.Members.Add(groupLeader);
                groupLeader.MyGroups.Add(group);
                db.SaveChanges();
                return group.Id;
            }
        }

        public Group GetGroupById(int id)
        {
            if (!db.Groups.Any(x => x.Id == id))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            else
            {
                return db.Groups.Single(x => x.Id == id);
            }
        }

        /// <summary>
        /// Adds user to group.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="groupId"></param>
        public void RequestGroupMembership(int groupId)
        {
            int userId = (int)HttpContext.Current.Session["Id"];
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            Group group = this.GetGroupById(groupId);
            User user = this.GetUserById(userId);
            if (group.Members.Any(x => x.Id == user.Id))
            {
                throw new ArgumentException("Вече сте член на тази група.");
            }
            if (group.MemberRequests.Any(x => x.Id == user.Id))
            {
                throw new ArgumentException("Вече сте заявили членство в тази група");
            }
            else
            {
                group.MemberRequests.Add(user);
                user.RequestedGroups.Add(group);
                db.SaveChanges();
            }
        }

        public void ConfirmMembership(int userId, int groupId)
        {
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = db.Users.Single(x => x.Id == userId);
            if (group.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("Вие не сте лидер на тази група.");
            }
            if (!group.MemberRequests.Contains(user))
            {
                throw new ArgumentException("Този потребител не е заявил тази група.");
            }
            if (group.Members.Contains(user))
            {
                throw new ArgumentException("Потребителят вече е член на групата.");
            }
            else
            {
                group.MemberRequests.Remove(user);
                user.RequestedGroups.Remove(group);
                group.Members.Add(user);
                user.MyGroups.Add(group);
                user.Notifications.Add(new Notification
                {
                    ActionLink = "/Groups/Details/" + group.Id,
                    Message = "Вие бяхте приет в " + group.Name
                });
                db.SaveChanges();
            }
        }

        public void LeaveGroup(int groupId)
        {
            int userId = (int)HttpContext.Current.Session["Id"];
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = db.Users.Single(x => x.Id == userId);
            if (!group.Members.Contains(user))
            {
                throw new ArgumentException("Вие не сте член на тази група.");
            }
            if (group.GroupLeader.Id == user.Id)
            {
                throw new ArgumentException("Не можете да напуснете групата защото сте нейн лидер.");
            }
            else
            {
                group.Members.Remove(user);
                db.SaveChanges();
            }
        }

        public void ExpellUserFromGroup(int userId, int groupId)
        {
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = db.Users.Single(x => x.Id == userId);
            if (group.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("Вие не сте лидер на тази група.");
            }
            if (!group.Members.Contains(user))
            {
                throw new ArgumentException("Този потребител не е член на групата.");
            }
            else
            {
                group.Members.Remove(user);
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "Вие бяхте изключен от " + group.Name
                });
                db.SaveChanges();
            }
        }

        public void DeleteGroup(int id) {
            if (!db.Groups.Any(x => x.Id == id))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            Group groupToRemove = db.Groups.Single(x => x.Id == id);
            if (groupToRemove.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("Вие не сте лидер на тази група.");
            }
            else {
                foreach (var user in groupToRemove.Members) {
                    user.MyGroups.Remove(groupToRemove);
                    user.Notifications.Add(new Notification{
                        Message = "Групата " + groupToRemove.Name + " беше премахната."
                    });
                }
                db.Groups.Remove(groupToRemove);
            }
        }

        public List<User> GetAllMembers(int id)
        {
            if (!db.Groups.Any(x => x.Id == id))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            else
            {
                return db.Groups.Single(x => x.Id == id).Members.ToList();
            }
        }

        public List<Question> GetAllGroupQuestions(int id)
        {
            if (!db.Groups.Any(x => x.Id == id))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            else
            {
                return db.Questions.Where(x => x.Group.Id == id).ToList();
            }
        }

        public Tuple<List<Group>, int> SearchGroups(string nameSubstring, int page)
        {
            List<Group> list = db.Groups.Where(x => x.Name.Contains(nameSubstring)).ToList();
            if ((list.Count / 10) + 1 < page)
            {
                page = 1;
            }
            return Tuple.Create(new List<Group>(list.Skip((page - 1) / 10).Take(10)), list.Count);
        }

        public List<Question> SearchQuestionsInGroup(string questionSubstring, int groupId)
        {
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            else
            {
                return db.Questions.Where(x => x.QuestionBrief.Contains(questionSubstring) && x.Group.Id == groupId).ToList();
            }
        }

        public void DenyMembership(int userId, int groupId)
        {
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Групата не съществува.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = db.Users.Single(x => x.Id == userId);
            if (group.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("Вие не сте лидер на тази група.");
            }
            if (!group.MemberRequests.Contains(user))
            {
                throw new ArgumentException("Този потребител не е заявил членство в тази група.");
            }
            else
            {
                group.MemberRequests.Remove(user);
                user.RequestedGroups.Remove(group);
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "Членството ви в " + group.Name + " беше отказано."
                });
                db.SaveChanges();
            }
        }

        #endregion

        //Moved
        #region QuestionService

        /// <summary>
        /// Gets the ten most recent questions in the datanbase.
        /// </summary>
        /// <returns></returns>
        public Tuple<List<Question>, int> GetTenMostRecentQuestions(int page)
        {
            var list = new List<Question>((from q in db.Questions
                                           orderby q.Date descending
                                                    where q.Group == null
                                                    select q));
            if ((list.Count / 10) + 1 < page)
            {
                page = 1;
            }
            return Tuple.Create(new List<Question>(list.Skip((page - 1) * 10).Take(10)), list.Count);
        }

        public List<Question> GetQuestionsForUser(int id)
        {
            if (!db.Users.Any(x => x.Id == id))
            {
                throw new ArgumentException("Потребителят не съществува.");
            }
            else
            {
                return db.Questions.Where(x => x.Author.Id == id && x.Group == null).ToList();
            }
        }

        /// <summary>
        /// Adds question to the database and return it's Id.
        /// </summary>
        /// <param name="questionBrief"></param>
        /// <param name="description"></param>
        /// <param name="category"></param>
        /// <param name="author"></param>
        public int AddQuestion(string questionBrief, string description, Category category, User author, Group group = null)
        {
            if (category.IsActive == false)
            {
                throw new ArgumentException("Категорията не съществува");
            }
            if (group == null)
            {
                var question = new Question
                    {
                        QuestionBrief = questionBrief,
                        Description = description,
                        Category = category,
                        Author = author,
                        Date = DateTime.Now,
                        IsReported = false
                    };
                db.Questions.Add(question);
                db.SaveChanges();
                foreach (var user in this.GetUserById(author.Id).MyFollowers)
                {
                    user.Notifications.Add(new Notification
                    {
                        ActionLink = "/Questions/Details/" + question.Id,
                        Message = author.Username + " зададе въпрос."
                    });
                }
                db.SaveChanges();
                return question.Id;
            }
            else
            {
                if (!group.Members.Any(x => x.Id == (int)HttpContext.Current.Session["Id"]))
                {
                    throw new ArgumentException("Вие не сте член на тази група.");
                }
                else
                {
                    var question = new Question
                    {
                        QuestionBrief = questionBrief,
                        Description = description,
                        Category = category,
                        Author = author,
                        Date = DateTime.Now,
                        IsReported = false,
                        Group = group
                    };
                    db.Questions.Add(question);
                    db.SaveChanges();
                    foreach (var member in group.Members)
                    {
                        if (member.Id != (int)HttpContext.Current.Session["Id"])
                        {
                            member.Notifications.Add(new Notification
                            {
                                ActionLink = "Questions/Details/" + question.Id,
                                Message = author.Username + " зададе въпрос в " + group.Name + "."
                            });
                        }
                    }
                    db.SaveChanges();
                    return question.Id;
                }
            }
        }

        /// <summary>
        /// Searches for questions in the database whose QuestionBrief contains the given substring.
        /// </summary>
        /// <param name="questionSubstring"></param>
        /// <returns></returns>
        public Tuple<List<Question>, int> SearchQuestion(string questionSubstring, int page)
        {
            List<Question> list = new List<Question>(db.Questions.Where(x => x.QuestionBrief.Contains(questionSubstring) && x.Group == null));
            if ((list.Count / 10) + 1 < page)
            {
                page = 1;
            }
            return Tuple.Create(list.Skip((page - 1) * 10).Take(10).ToList(), list.Count);
        }

        /// <summary>
        /// Searches for questions in given the category whose QuestionBrief contains the given substring.
        /// </summary>
        /// <param name="questionSubstring"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tuple<List<Question>, int> SearchQuestionInCategory(string questionSubstring, int id, int page)
        {
            List<Question> list = new List<Question>(db.Questions.Where(x => x.QuestionBrief.Contains(questionSubstring) && x.Category.Id == id && x.Group == null));
            if ((list.Count / 10) + 1 < page)
            {
                page = 1;
            }
            return Tuple.Create(list.Skip((page - 1) * 10).Take(10).ToList(), list.Count);
        }

        /// <summary>
        /// Gets question from the database by given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Question GetQuestionById(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Въпросът не съществува.");
            }
            else
            {
                Question question = db.Questions.Single(x => x.Id == id);
                if ( HttpContext.Current.Session["Id"] != null && question.Group != null && !(question.Group.Members.Any(x => x.Id == (int)HttpContext.Current.Session["Id"])))
                {
                    throw new ArgumentException("Не можете да достъпите въпроса защото не сте член на  " + question.Group.Name);
                }
                else
                {
                    return question;
                }
            }
        }

        /// <summary>
        /// Adds answer to the given question and add notification to the user who asked the question.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="author"></param>
        /// <param name="answerBody"></param>
        public void AnswerQuestion(Question question, User author, string answerBody)
        {
            question.Answers.Add(new Answer
            {
                Question = question,
                Author = author,
                AnswerBody = answerBody,
                Date = DateTime.Now,
                IsReported = false
            });
            question.Author.Notifications.Add(new Notification
            {
                Message = "Въпросът ви беше отговорен от " + author.Username,
                ActionLink = "/Questions/Details/" + question.Id
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Deletes question by given id and removes all notifications related to it.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteQuestion(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Въпросът не съществува.");
            }
            var questionToRemove = db.Questions.Single(x => x.Id == id);
            if (HttpContext.Current.Session["Role"].ToString() == "Admin" || HttpContext.Current.Session["Role"].ToString() == "Moderator" || (int)HttpContext.Current.Session["Id"] == questionToRemove.Author.Id)
            {                
                {
                    for (int i = 0; i < questionToRemove.Comments.Count; i++)
                    {
                        db.Comments.Remove(questionToRemove.Comments[i]);
                    }
                    for (int i = 0; i < questionToRemove.Answers.Count; i++)
                    {
                        for (int j = 0; j < questionToRemove.Answers[i].Comments.Count; j++)
                        {
                            db.Comments.Remove(questionToRemove.Answers[i].Comments[j]);
                        }
                        db.Answers.Remove(questionToRemove.Answers[i]);
                    }
                    string urlInNotification = "Questions/Details/" + id.ToString();
                    foreach (var notification in db.Notifications.Where(x => x.ActionLink.Contains(urlInNotification)))
                    {
                        db.Notifications.Remove(notification);
                    }
                    db.Questions.Remove(questionToRemove);
                    db.SaveChanges();
                }
            }
            else
            {
                throw new ProviderException("Нямате права.");
            }
        }

        public List<Question> GetQuestionsForGroup(int id) {
            return db.Questions.Where(x => x.Group.Id == id).ToList();
        }

        public void ReportQuestion(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Въпросът не съществува.");
            }
            else
            {
                db.Questions.Single(x => x.Id == id).IsReported = true;
                db.SaveChanges();
            }
        }

        public List<Question> GetAllReportedQuestions()
        {
            return db.Questions.Where(x => x.IsReported == true).ToList();
        }

        /// <summary>
        /// Clears reported state of question if it exists.
        /// </summary>
        /// <param name="id"></param>
        public void ClearQuestionState(int id)
        {
            if (!db.Questions.Any(x => x.Id == id))
            {
                throw new ArgumentException("Въпросът не съществува.");
            }
            else
            {
                Question question = db.Questions.Single(x => x.Id == id);
                if (question.IsReported == false)
                {
                    throw new ArgumentException("Въпросът не е докладван.");
                }
                else
                {
                    question.IsReported = false;
                    db.SaveChanges();
                }
            }
        }

        #endregion

        //Moved
        #region CommentsService

        public Comment GetCommentById(int id) {
            if (!db.Comments.Any(x => x.Id == id)) {
                throw new ArgumentException("Коментарът не съществува.");
            }
            else
            {
                return db.Comments.Single(x => x.Id == id);            
            }
        }

        /// <summary>
        /// Adds comment to the given question.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="author"></param>
        /// <param name="commentBody"></param>
        public void AddCommentToQuestion(Question question, User author, string commentBody)
        {
            question.Comments.Add(new Comment
            {
                Author = author,
                CommentBody = commentBody,
                IsReported = false,
                Date = DateTime.Now
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Adds comment to given answer.
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="author"></param>
        /// <param name="commentBody"></param>
        public void AddCommentToAnswer(Answer answer, User author, string commentBody)
        {
            answer.Comments.Add(new Comment
            {
                Author = author,
                CommentBody = commentBody,
                IsReported = false,
                Date = DateTime.Now
            });
            db.SaveChanges();
        }

        public void ReportComment(int id) {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Коментарът не съществува.");
            }
            else
            {
                db.Comments.Single(x => x.Id == id).IsReported = true;
                db.SaveChanges();
            }
        }

        public void DeleteComment(int id)
        {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Коментарът не съществува.");
            }
            var commentToRemove = db.Comments.Single(x => x.Id == id);
            if (HttpContext.Current.Session["Role"].ToString() == "Admin" || HttpContext.Current.Session["Role"].ToString() == "Moderator" || (int)HttpContext.Current.Session["Id"] == commentToRemove.Author.Id)
            {
                db.Comments.Remove(commentToRemove);
                db.SaveChanges();
            }
            else
            {
                throw new ProviderException("Нямате права.");
            }
        }

        public List<Comment> GetAllReportedComments()
        {
            return db.Comments.Where(x => x.IsReported == true).ToList();
        }

        public void ClearCommentState(int id)
        {
            if (!db.Comments.Any(x => x.Id == id))
            {
                throw new ArgumentException("Коментарът не съществува.");
            }
            else
            {
                Comment comment = db.Comments.Single(x => x.Id == id);
                if (comment.IsReported == false)
                {
                    throw new ArgumentException("Коментарът не е докладван.");
                }
                else
                {
                    comment.IsReported = false;
                    db.SaveChanges();
                }
            }
        }

        #endregion

        //Moved
        #region AnswersService

        public Answer GetAnswerById(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Отговорът не съществува.");
            }
            else
            {
                return db.Answers.Single(x => x.Id == id);
            }
        }

        public void ReportAnswer(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Отговорът не съществува.");
            }
            else
            {
                db.Answers.Single(x => x.Id == id).IsReported = true;
                db.SaveChanges();
            }
        }

        public void DeleteAnswer(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Отговорът не съществува.");
            }
            var answerToRemove = db.Answers.Single(x => x.Id == id);
            if (HttpContext.Current.Session["Role"].ToString() == "Admin" || HttpContext.Current.Session["Role"].ToString() == "Moderator" || (int)HttpContext.Current.Session["Id"] == answerToRemove.Author.Id)
            {
                foreach (var comment in answerToRemove.Comments)
                {
                    answerToRemove.Comments.Remove(comment);
                    db.Comments.Remove(comment);
                }
                db.Answers.Remove(answerToRemove);
                db.SaveChanges();
            }
            else
            {
                throw new ProviderException("Нямате права.");
            }
        }

        public List<Answer> GetAllReportedAnswers()
        {
            return db.Answers.Where(x => x.IsReported == true).ToList();
        }

        public void ClearAnswerState(int id)
        {
            if (!db.Answers.Any(x => x.Id == id))
            {
                throw new ArgumentException("Отговорът не съществува.");
            }
            else
            {
                Answer answer = db.Answers.Single(x => x.Id == id);
                if (answer.IsReported == false)
                {
                    throw new ArgumentException("Отговорът не е докладван.");
                }
                else
                {
                    answer.IsReported = false;
                    db.SaveChanges();
                }
            }
        }

        #endregion

        #region NotificationsService

        public void DeleteNotificationById(int id)
        {
            if (!db.Notifications.Any(x => x.Id == id))
            {
                throw new ArgumentException("Известието не съществува.");
            }
            else
            {
                db.Notifications.Remove(db.Notifications.Single(x => x.Id == id));
                db.SaveChanges();
            }
        }

        #endregion
    }
}