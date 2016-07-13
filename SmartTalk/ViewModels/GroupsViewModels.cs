using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTalk.ViewModels
{

    public class GroupListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string GroupLeaderName { get; set; }

        public int NumberOfMembers { get; set; }

        public bool IsLoggedUserMember { get; set; }

        public bool IsLoggedUserRequested { get; set; }

    }

    public class GroupsShowViewModel
    {
        public List<GroupListViewModel> Groups { get; set; }
    }

    public class GroupsSearchViewModel
    {
        public List<GroupListViewModel> Groups { get; set; }

        public int NumberOfPages { get; set; }

        public int ActivePage { get; set; }
    }

    public class GroupsCreateViewModel
    {
        public string Name { get; set; }
    }

    public class GroupsDetailsViewModel
    {
        public int Id { get; set; }

        public string GroupLeaderName { get; set; }

        public int GroupLeaderId { get; set; }

        public bool IsLoggedUserLeader { get; set; }

        public bool IsLoggedUserMember { get; set; }

        public List<UserRequestViewModel> RequestedUsers { get; set; }

        public List<QuestionListViewModel> Questions { get; set; }

        public List<AccountListViewModel> Members { get; set; }
    }

    public class UserRequestViewModel
    {
        public int Id { get; set;}

        public string Username { get; set; }
    }
}