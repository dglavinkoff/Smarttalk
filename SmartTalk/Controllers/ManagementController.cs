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
    [Moderator]
    public class ManagementController : BaseController
    {

        public ActionResult ControlPanel()
        {
            var viewModel = new ControlViewModel
            {
                ReportedAnswers = dataService.GetAllReportedAnswers(),
                ReportedQuestions = dataService.GetAllReportedQuestions(),
                ReportedComments = dataService.GetAllReportedComments()
            };
            if (HttpContext.Session["Role"].ToString() == "Admin")
            {
                List<Category> requestedCategories = dataService.GetAllRequestedCategories();
                List<CategoriesListSimpleViewModel> categoriesList = new List<CategoriesListSimpleViewModel>();
                foreach (var category in requestedCategories)
                {
                    categoriesList.Add(new CategoriesListSimpleViewModel
                    {
                        Id = category.Id,
                        Name = category.Name
                    });
                }
                List<User> moderators = dataService.GetAllModerators();
                List<AccountListSimpleViewModel> moderatorsList = new List<AccountListSimpleViewModel>();
                foreach (var moderator in moderators)
                {
                    moderatorsList.Add(new AccountListSimpleViewModel
                    {
                        Id = moderator.Id,
                        Username = moderator.Username
                    });
                }
                viewModel.RequestedCategories = categoriesList;
                viewModel.Moderators = moderatorsList;
            }
            return View(viewModel);
        }
    }
}
