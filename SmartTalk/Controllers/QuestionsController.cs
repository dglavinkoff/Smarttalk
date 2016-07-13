using SmartTalk.Models;
using SmartTalk.Security;
using SmartTalk.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SmartTalk.Controllers
{
    public class QuestionsController : BaseController
    {
        public QuestionsController()
        {
            this.db = new AppContext();
        }

        private AppContext db;

        //Finished
        [HttpGet]
        public ActionResult ShowTenMostRecent(int page)
        {
            var result = dataService.GetTenMostRecentQuestions(page);
            List<QuestionListViewModel> questionList = new List<QuestionListViewModel>();
            foreach (var question in result.Item1)
            {
                questionList.Add(new QuestionListViewModel
                {
                    Id = question.Id,
                    QuestionBrief = question.QuestionBrief,
                    AuthorName = question.Author.Username,
                    Category = question.Category.Name
                });
            }
            return View(new SearchQuestionsViewModel
            {
                Questions = questionList,
                NumberOfPages = (result.Item2 / 10) + 1,
                ActivePage = page
            });
        }

        [HttpGet]
        [Member]
        public ActionResult AskQuestion(int groupId = 0) {
            QuestionAskViewModel viewModel;
            if (groupId != 0)
            {
                viewModel = new QuestionAskViewModel
                {
                    Categories = new SelectList(dataService.GetAllActiveCategories(), "Id", "Name"),
                    GroupId = groupId,
                    UrlReferrer = Request.UrlReferrer.ToString()
                };
                return View(viewModel);
            }
            else
            {
                viewModel = new QuestionAskViewModel
                {
                    Categories = new SelectList(dataService.GetAllActiveCategories(), "Id", "Name"),
                    UrlReferrer = Request.UrlReferrer.ToString()
                };
                return View(viewModel);
            }
        }

        //Finished
        [HttpPost]
        [Member]
        public ActionResult AskQuestion(QuestionAskViewModel viewModel)
        {
            var User = dataService.GetUserById(Id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.GroupId == 0)
                    {
                        int newId = dataService.AddQuestion(HttpUtility.HtmlEncode(viewModel.QuestionBrief), HttpUtility.HtmlEncode(viewModel.Description), dataService.GetCategoryById(viewModel.Category), dataService.GetUserById(User.Id));
                        return Redirect(viewModel.UrlReferrer.ToString());
                    }
                    else
                    {
                        int newId = dataService.AddQuestion(HttpUtility.HtmlEncode(viewModel.QuestionBrief), HttpUtility.HtmlEncode(viewModel.Description), dataService.GetCategoryById(viewModel.Category), dataService.GetUserById(User.Id), dataService.GetGroupById(viewModel.GroupId));
                        return Redirect(viewModel.UrlReferrer.ToString());
                    }
                }
                catch (ArgumentException ex) {
                    ViewBag.Message = ex.Message;
                    return View("Error");
                }

            }
            else
            {
                return View(viewModel);
            }
        }

        //Finished
        [HttpGet]
        public ActionResult Details(int id)
        {
            try
            {
                Question question = dataService.GetQuestionById(id);
                var viewModel = new QuestionDetailsViewModel
                {
                    Id = question.Id,
                    QuestionBrief = question.QuestionBrief,
                    Description = question.Description,
                    AuthorName = question.Author.Username,
                    DateAsked = question.Date,
                    Answers = question.Answers.ToList(),
                    Comments = question.Comments.ToList(),
                    IsReported = question.IsReported
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
        [HttpGet]
        public ActionResult Search(string questionSubstring, int page)
        {
            var result = dataService.SearchQuestion(questionSubstring, page);
            List<Question> matchedQuestions = result.Item1;
            List<QuestionListViewModel> questionList = new List<QuestionListViewModel>();
            foreach (var question in matchedQuestions)
            {
                questionList.Add(new QuestionListViewModel
                {
                    Id = question.Id,
                    QuestionBrief = question.QuestionBrief,
                    AuthorName = question.Author.Username,
                    Category = question.Category.Name
                });
            }
            var viewModel = new SearchQuestionsViewModel
            {
                Questions = questionList,
                NumberOfPages = (result.Item2 / 10) + 1,
                ActivePage = page
            };
            return PartialView("_SearchQuestions", viewModel);
        }

        public ActionResult SearchByCategory(string questionSubstring, int id, int page) {
            var result = dataService.SearchQuestionInCategory(questionSubstring, id, page);
            List<Question> matchedQuestions = result.Item1;
            List<QuestionListViewModel> questionList = new List<QuestionListViewModel>();
            foreach (var question in matchedQuestions)
            {
                questionList.Add(new QuestionListViewModel
                {
                    Id = question.Id,
                    QuestionBrief = question.QuestionBrief,
                    AuthorName = question.Author.Username,
                    Category = question.Category.Name
                });
            }
            var viewModel = new SearchQuestionsViewModel
            {
                Questions = questionList,
                NumberOfPages = (result.Item2 / 10) + 1,
                ActivePage = page
            };
            return PartialView("_SearchQuestions", viewModel);
        }

        //Finished
        [HttpPost]
        [Member]
        public ActionResult Answer()
        {
            NameValueCollection form = Request.Form;
            int questionId = Convert.ToInt32(form["QuestionId"]);
            var user = dataService.GetUserById(Convert.ToInt32(form["UserId"]));
            dataService.AnswerQuestion(dataService.GetQuestionById(questionId), user, form["AnswerBody"]);
            foreach (var follower in user.MyFollowers) {
                follower.Notifications.Add(new Notification
                {
                    ActionLink = "/Questions/Details/" + questionId,
                    Message = user.Username + " answered to " + dataService.GetQuestionById(questionId).QuestionBrief
                });
            }
            return Redirect("~/Questions/Details/" + questionId);
        }

        [HttpGet]
        [Member]
        public ActionResult Delete(int id)
        {
            try{
                dataService.DeleteQuestion(id);
                return Redirect("/Questions/ShowTenMostRecent/1");
            }
            catch(ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }


        [HttpGet]
        [Member]
        public ActionResult Report(int id) {
            try
            {
                dataService.ReportQuestion(id);
                return Redirect("/Questions/Details/" + id);
            }
            catch (ArgumentException ex) {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Moderator]
        public ActionResult ClearState(int id)
        {
            try
            {
                dataService.ClearQuestionState(id);
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
