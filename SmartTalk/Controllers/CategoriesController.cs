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
    public class CategoriesController : BaseController
    {

        //Finished
        public ActionResult ShowAll()
        {
            List<CategoriesListViewModel> categories = new List<CategoriesListViewModel>();

            foreach (var category in dataService.GetAllActiveCategories())
            {
                categories.Add(new CategoriesListViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    NumberOfQuestions = category.Questions.Count(x => x.Group == null),
                });
            }
            var viewModel = new CategoriesShowViewModel
            {
                Categories = categories
            };
            return View(viewModel);
        }

        //Finished
        [Admin]
        [HttpGet]
        public string Create(string name){
                try
                {
                    dataService.CreateCategory(name);
                    return "Category successfully created. Reload the page for the changes to take effect.";
                }
                catch (ArgumentException ex) {
                    return ex.Message;
                }
        }

        //Finished
        [Admin]
        public ActionResult Delete(int id) {
            try
            {
                dataService.DeleteCategory(id);
            }
            catch(ArgumentException ex) {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
            return Redirect("/Categories/ShowAll");
        }

        //Finished
        [HttpGet]
        public ActionResult ShowQuestions(int id, int page) {
            var result = dataService.GetAllQuestions(id, page);
            List<QuestionListViewModel> questions = new List<QuestionListViewModel>();
            foreach (var question in result.Item1)
            {
                questions.Add(new QuestionListViewModel
                {
                    Id = question.Id,
                    Category = question.Category.Name,
                    AuthorName = question.Author.Username,
                    QuestionBrief = question.QuestionBrief
                });
            }
            return View(new CategoriesShowQuestionsViewModel {
                Id = id,
                Questions = questions,
                NumberOfPages = (result.Item2 / 10) + 1,
                ActivePage = page
            });
        }

        [HttpGet]
        [Member]
        public string RequestCategory(string name)
        {
            try
            {
                dataService.RequestCategory(HttpUtility.HtmlEncode(name));
                return "Category successfully requested.";
            }
            catch (ArgumentException ex) {
                return ex.Message;
            }
        }

        [HttpGet]
        [Admin]
        public ActionResult Confirm(int id)
        {
            try
            {
                dataService.ConfirmCategory(id);
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
        public ActionResult Deny(int id)
        {
            try
            {
                dataService.DenyCategory(id);
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
