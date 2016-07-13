using SmartTalk.Models;
using SmartTalk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration.Provider;
using System.Web.UI;
using SmartTalk.Security;
using System.Web.UI.WebControls;

namespace SmartTalk.Controllers
{
    public class AccountController : BaseController
    {

        //Finished
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        //Finished
        [HttpPost]
        public ActionResult Register(AccountRegisterViewModel viewModel)
        {
            if (dataService.UsernameExist(viewModel.Username))
            {
                ModelState.AddModelError("Name", "Username already exists.");
            }
            if (ModelState.IsValid)
            {
                int newId = dataService.RegisterUser( HttpUtility.HtmlEncode(viewModel.Username), HttpUtility.HtmlEncode(viewModel.Firstname), HttpUtility.HtmlEncode(viewModel.Lastname), HttpUtility.HtmlEncode(viewModel.Email), viewModel.Password, "Member");
                Session["Username"] = viewModel.Username;
                Session["Role"] = "Member";
                Session["Id"] = newId;
                return RedirectToHomePage();
            }
            else
            {
                return View(viewModel);
            }
        }

        //Finished
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Finished
        [HttpPost]
        public ActionResult Login(AccountLoginViewModel viewModel)
        {
            try
            {
                int id = dataService.ValidateUser(viewModel.Username, viewModel.Password);
                Session["Username"] = viewModel.Username;
                Session["Role"] = dataService.GetUserById(id).Role;
                Session["Id"] = id;
                return RedirectToHomePage();
            }
            catch (ArgumentException ex)
            {
                if (ex.Message == "Username does not exist.")
                {
                    ModelState.AddModelError("Username", ex.Message);
                    return View(viewModel);
                }
                else
                {
                    ModelState.AddModelError("Password", ex.Message);
                    return View(viewModel);
                }
            }
        }

        //Finished
        [Member]
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToHomePage();
        }

        [HttpGet]
        public string ForgottenPassword(string username)
        {

            try
            {
                dataService.RequestNewPassword(username);
                return "Your new password was sent to your email.";
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
        }

        [Member]
        [HttpGet]
        public ActionResult ChangePassword()
        {
            var viewModel = new ChangePasswordViewModel
            {
                UserId = this.Id
            };
            return PartialView("_ChangePassword", viewModel);
        }

        [Member]
        [HttpPost]
        public ActionResult ChangePasswordPost(ChangePasswordViewModel viewModel)
        {
            try
            {
                //return dataService.ChangePassword(Convert.ToInt32(form["UserId"]), form["OldPassword"].ToString(), form["NewPassword"].ToString());
                ViewBag.Message = dataService.ChangePassword(viewModel.UserId, viewModel.OldPassword, viewModel.NewPassword);
                ViewBag.UrlReferrer = Request.UrlReferrer.ToString();
                return View("Success");
            }

            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                ViewBag.UrlReferrer = Request.UrlReferrer.ToString();
                return View("Success");
            }
        }

        //Finished
        [HttpGet]
        public ActionResult ProfileDetails(int id)
        {
            SmartTalk.Models.User user = new User();
            try
            {
                user = dataService.GetUserById(id);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }

            var viewModel = new AccountDetailsViewModel
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Username = user.Username,
                Role = user.Role
            };

            viewModel.Questions = new List<QuestionListViewModel>();
            foreach (var question in dataService.GetQuestionsForUser(user.Id))
            {               
                viewModel.Questions.Add(new QuestionListViewModel
                {
                    Id = question.Id,
                    AuthorName = user.Username,
                    QuestionBrief = question.QuestionBrief,
                    Category = question.Category.Name
                });
            }
            if (Id != 0)
            {
                viewModel.IsFollowedByLoggedUser = user.MyFollowers.Any(x => x.Id == this.Id);
                viewModel.isBannedByLoggedUser = dataService.GetUserById(this.Id).BannedUsers.Any(x => x.Id == user.Id);
                viewModel.isLoggedUserBanned = user.BannedUsers.Any(x => x.Id == this.Id);
            }
            if (id == Id)
            {
                viewModel.Followers = new List<AccountListSimpleViewModel>();
                viewModel.Favorites = new List<AccountListSimpleViewModel>();
                viewModel.Banned = new List<AccountListSimpleViewModel>();
                foreach (var follower in user.MyFollowers)
                {
                    viewModel.Followers.Add(new AccountListSimpleViewModel
                    {
                        Id = follower.Id,
                        Username = follower.Username
                    });
                }

                foreach (var favorite in user.MyFavorites)
                {
                    viewModel.Favorites.Add(new AccountListSimpleViewModel
                    {
                        Id = favorite.Id,
                        Username = favorite.Username
                    });
                }

                foreach (var banned in user.BannedUsers)
                {
                    viewModel.Banned.Add(new AccountListSimpleViewModel
                    {
                        Id = banned.Id,
                        Username = banned.Username
                    });
                }

            }
            return View(viewModel);
        }

        //Finished
        [Admin]
        public ActionResult Delete(int id)
        {
            try
            {
                dataService.DeleteUser(id);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        //Finished
        [HttpGet]
        public ActionResult TopTenUsers(int page)
        {
            var result = dataService.GetTopTenUsers(page);
            List<User> TopTenUsers = result.Item1;
            List<AccountListViewModel> usersViewModels = new List<AccountListViewModel>();
            foreach (var user in TopTenUsers) {
                var viewModel = new AccountListViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    FollowersCount = user.MyFollowers.Count,
                };
                if (Id != 0)
                {
                    viewModel.IsFollowedByLoggedUser = user.MyFollowers.Any(x => x.Id == this.Id);
                    viewModel.isBannedByLoggedUser = dataService.GetUserById(this.Id).BannedUsers.Any(x => x.Id == user.Id);
                    viewModel.isLoggedUserBanned = user.BannedUsers.Any(x => x.Id == this.Id);
                }
                usersViewModels.Add(viewModel);
            }
            return View(new SearchUsersViewModel
            {
                users = usersViewModels,
                NumberOfPages = (result.Item2 / 10) + 1,
                ActivePage = page
            });
        }

        //Finished
        [HttpGet]
        public ActionResult SearchUsers(string username, int page)
        {
            var result = dataService.SearchUsers(username, page);
            List<AccountListViewModel> usersViewModels = new List<AccountListViewModel>();
            foreach(var user in result.Item1)
            {
                var newViewModel = new AccountListViewModel();
                    newViewModel.Id = user.Id;
                    newViewModel.Username = user.Username;
                    newViewModel.FollowersCount = user.MyFollowers.Count;
                if (this.Id != 0)
                {
                    newViewModel.IsFollowedByLoggedUser = user.MyFollowers.Any(x => x.Id == this.Id);
                    newViewModel.isBannedByLoggedUser = dataService.GetUserById(this.Id).BannedUsers.Any(x => x.Id == user.Id);
                    newViewModel.isLoggedUserBanned = user.BannedUsers.Any(x => x.Id == this.Id);
                }
                usersViewModels.Add(newViewModel);
            }
            SearchUsersViewModel viewModel = new SearchUsersViewModel
            {
                users = usersViewModels,
                NumberOfPages = (result.Item2 / 10) + 1,
                ActivePage = page
            };
            return PartialView("_SearchUsers", viewModel);
        }

        [HttpGet]
        public ActionResult FollowUser(int id)
        {
            try
            {
                dataService.AddUserToFavorites(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult BanUser(int id)
        {
            try
            {
                dataService.BanUser(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Admin]
        public ActionResult Promote(int id)
        {
            try
            {
                dataService.PromoteUser(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        public ActionResult Demote(int id)
        {
            try
            {
                dataService.DemoteUser(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Member]
        public ActionResult Unfollow(int id)
        {
            try
            {
                dataService.UnfollowUser(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Member]
        public ActionResult Unban(int id)
        {
            try
            {
                dataService.Unbann(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }
    }
}

