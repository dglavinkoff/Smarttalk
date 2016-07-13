using SmartTalk.Models;
using SmartTalk.Security;
using SmartTalk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTalk.Controllers
{
    [Member]
    public class GroupsController : BaseController
    {

        //Finished
        [HttpGet]
        public ActionResult TopTenGroups(int page)
        {
            var result = dataService.GetTopTenGroups(page);
            List<Group> topTenGroups = result.Item1;
            GroupsSearchViewModel viewModel = new GroupsSearchViewModel();
            viewModel.Groups = new List<GroupListViewModel>();
            foreach (var group in topTenGroups)
            {
                var groupViewModel = new GroupListViewModel();
                    groupViewModel.Id = group.Id;
                    groupViewModel.Name = group.Name;
                    groupViewModel.GroupLeaderName = group.GroupLeader.Username;
                    groupViewModel.NumberOfMembers = group.Members.Count;
                if(this.Id != 0)
                {
                    groupViewModel.IsLoggedUserMember = group.Members.Any(x => x.Id == Id);
                    groupViewModel.IsLoggedUserRequested = group.MemberRequests.Any(x => x.Id == Id);
                }
                viewModel.Groups.Add(groupViewModel);
            }
            viewModel.NumberOfPages = result.Item2 / 10 + 1;
            viewModel.ActivePage = page;
            return View(viewModel);
        }

        //Should redirect to group's homepage.
        [HttpPost]
        public ActionResult Create(GroupsCreateViewModel viewModel)
        {
            try
            {
                int newId = dataService.CreateGroup(viewModel.Name, dataService.GetUserById(Id));

            }
            catch (ArgumentException ex)
            {
                if (ex.Message == "Name already exists.")
                {
                    ModelState.AddModelError("Name", ex.Message);
                    return View(viewModel);
                }
                else
                {
                    ModelState.AddModelError("Name", ex.Message);
                    return View(viewModel);
                }
            }
            return Redirect("/Groups/TopTenGroups/1");
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //Finished
        public ActionResult Details(int id)
        {
            try
            {
                Group group = dataService.GetGroupById(id);
                bool isLoggedUserLeader = (group.GroupLeader.Id == Id);
                bool isLoggedUserMember = (group.Members.Any(x => x.Id == Id));
                List<QuestionListViewModel> questions = new List<QuestionListViewModel>();
                List<AccountListViewModel> members = new List<AccountListViewModel>();
                List<UserRequestViewModel> requested = new List<UserRequestViewModel>();
                if (isLoggedUserMember)
                {
                    foreach (var question in dataService.GetAllGroupQuestions(group.Id))
                    {
                        questions.Add(new QuestionListViewModel
                        {
                            Id = question.Id,
                            QuestionBrief = question.QuestionBrief,
                            AuthorName = question.Author.Username,
                            Category = question.Category.Name
                        });
                    }
                }
                foreach (var user in group.Members)
                {
                    members.Add(new AccountListViewModel
                    {
                        Id = user.Id,
                        Username = user.Username,
                        FollowersCount = user.MyFollowers.Count,
                        IsFollowedByLoggedUser = user.MyFollowers.Any(x => x.Id == this.Id),
                        isBannedByLoggedUser = dataService.GetUserById(this.Id).BannedUsers.Any(x => x.Id == user.Id),
                        isLoggedUserBanned = user.BannedUsers.Any(x => x.Id == this.Id)
                    });
                }
                if (isLoggedUserLeader)
                {
                    foreach (var user in group.MemberRequests)
                    {
                        requested.Add(new UserRequestViewModel
                        {
                            Id = user.Id,
                            Username = user.Username
                        });
                    }
                }
                var viewModel = new GroupsDetailsViewModel
                {
                    Id = group.Id,
                    GroupLeaderName = group.GroupLeader.Username,
                    GroupLeaderId = group.GroupLeader.Id,
                    IsLoggedUserLeader = isLoggedUserLeader,
                    IsLoggedUserMember = isLoggedUserMember,
                    Members = members,
                    Questions = questions,
                    RequestedUsers = requested
                };
                return View(viewModel);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        //Finished
        public ActionResult RequestGroup(int id)
        {
            try
            {
                dataService.RequestGroupMembership(id);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpGet]
        public ActionResult Leave(int id)
        {
            try
            {
                dataService.LeaveGroup(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult Expell(int userId, int groupId)
        {
            try
            {
                dataService.ExpellUserFromGroup(userId, groupId);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        public ActionResult ConfirmMembership(int userId, int groupId)
        {
            try
            {
                dataService.ConfirmMembership(userId, groupId);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        public ActionResult DenyMembership(int userId, int groupId)
        {
            try
            {
                dataService.DenyMembership(userId, groupId);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        public ActionResult SearchGroups(string nameSubstring, int page)
        {
            var result = dataService.SearchGroups(nameSubstring, page);
            List<Group> matchedGroups = result.Item1;
            List<GroupListViewModel> groupsList = new List<GroupListViewModel>();
            var viewModel = new GroupsSearchViewModel();
            foreach (var group in matchedGroups)
            {
                var groupViewModel = new GroupListViewModel();
                groupViewModel.Id = group.Id;
                groupViewModel.Name = group.Name;
                groupViewModel.GroupLeaderName = group.GroupLeader.Username;
                groupViewModel.NumberOfMembers = group.Members.Count;
                if (this.Id != 0)
                {
                    groupViewModel.IsLoggedUserMember = group.Members.Any(x => x.Id == Id);
                    groupViewModel.IsLoggedUserRequested = group.MemberRequests.Any(x => x.Id == Id);
                }
                groupsList.Add(groupViewModel);
            }
            viewModel.Groups = groupsList;
            viewModel.NumberOfPages = result.Item2 / 10 + 1;
            viewModel.ActivePage = page;
            return PartialView("_SearchGroups", viewModel);
        }
    } 
}
