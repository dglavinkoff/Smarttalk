using SmartTalk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.Services
{
    public class GroupsService
    {
        public GroupsService()
        {
            this.db = new AppContext();
            this.accountService = new AccountsService();
        }

        private AppContext db;
        private AccountsService accountService;

        /// <summary>
        /// Gets the top ten groups with most members.
        /// </summary>
        /// <returns></returns>
        public List<Group> GetTopTenGroups()
        {
            return new List<Group>((from gr in db.Groups
                                    orderby gr.Members.Count descending
                                    select gr).Take(10));
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
                throw new ArgumentException("Name already exist.");
            }
            if (db.Groups.Any(x => x.GroupLeader.Id == groupLeader.Id))
            {
                throw new ArgumentException("You can be leader of only one group at a time.");
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
                throw new ArgumentException("Group does not exist.");
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
                throw new ArgumentException("Group does not exist.");
            }
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("User does not exist.");
            }
            Group group = this.GetGroupById(groupId);
            User user = accountService.GetUserById(userId);
            if (group.Members.Any(x => x.Id == user.Id))
            {
                throw new ArgumentException("You are alredy member of this group.");
            }
            if (group.MemberRequests.Any(x => x.Id == user.Id))
            {
                throw new ArgumentException("You have already requested membership of that group;");
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
                throw new ArgumentException("Group does not exist.");
            }
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("User does not exist.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = accountService.GetUserById(userId);
            if (group.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("You are not group leader of this group.");
            }
            if (!group.MemberRequests.Contains(user))
            {
                throw new ArgumentException("User has not requested this group.");
            }
            if (group.Members.Contains(user))
            {
                throw new ArgumentException("User is already member of that group.");
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
                    Message = "You have been accepted to " + group.Name
                });
                db.SaveChanges();
            }
        }

        public void LeaveGroup(int groupId)
        {
            int userId = (int)HttpContext.Current.Session["Id"];
            if (!db.Users.Any(x => x.Id == userId))
            {
                throw new ArgumentException("User does not exist.");
            }
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Group does not exist.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = accountService.GetUserById(userId);
            if (!group.Members.Contains(user))
            {
                throw new ArgumentException("You are not member oh this group.");
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
                throw new ArgumentException("User does not exist.");
            }
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Group does not exist.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = accountService.GetUserById(userId);
            if (group.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("You are not group leader of this group.");
            }
            if (!group.Members.Contains(user))
            {
                throw new ArgumentException("User is not member of this group.");
            }
            else
            {
                group.Members.Remove(user);
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "You have been expelled from " + group.Name
                });
                db.SaveChanges();
            }
        }

        public void DeleteGroup(int id)
        {
            if (!db.Groups.Any(x => x.Id == id))
            {
                throw new ArgumentException("Group does not exist.");
            }
            Group groupToRemove = db.Groups.Single(x => x.Id == id);
            if (groupToRemove.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("You are not group leader of this group.");
            }
            else
            {
                foreach (var user in groupToRemove.Members)
                {
                    user.MyGroups.Remove(groupToRemove);
                    user.Notifications.Add(new Notification
                    {
                        Message = "The group " + groupToRemove.Name + " has been removed."
                    });
                }
                db.Groups.Remove(groupToRemove);
            }
        }

        public List<User> GetAllMembers(int id)
        {
            if (!db.Groups.Any(x => x.Id == id))
            {
                throw new ArgumentException("Group does not exist.");
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
                throw new ArgumentException("Group does not exist.");
            }
            else
            {
                return db.Questions.Where(x => x.Group.Id == id).ToList();
            }
        }

        public List<Group> SearchGroups(string nameSubstring)
        {
            return db.Groups.Where(x => x.Name.Contains(nameSubstring)).ToList();
        }

        public List<Question> SearchQuestionsInGroup(string questionSubstring, int groupId)
        {
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Group does not exist.");
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
                throw new ArgumentException("User does not exist.");
            }
            if (!db.Groups.Any(x => x.Id == groupId))
            {
                throw new ArgumentException("Group does not exist.");
            }
            Group group = db.Groups.Single(x => x.Id == groupId);
            User user = accountService.GetUserById(userId);
            if (group.GroupLeader.Id != (int)HttpContext.Current.Session["Id"])
            {
                throw new ArgumentException("You are not group leader of this group.");
            }
            if (!group.MemberRequests.Contains(user))
            {
                throw new ArgumentException("This user has not requested membership to this group.");
            }
            else
            {
                group.MemberRequests.Remove(user);
                user.RequestedGroups.Remove(group);
                user.Notifications.Add(new Notification
                {
                    ActionLink = "",
                    Message = "Your membership to " + group.Name + " has been denied."
                });
                db.SaveChanges();
            }
        }

    }
}