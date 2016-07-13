using SmartTalk.Security;
using SmartTalk.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTalk.Controllers
{
    public class CommentsController : BaseController
    {

        [HttpPost]
        [Member]
        public ActionResult AddCommentToQuestion() {
            var form = Request.Form;
            dataService.AddCommentToQuestion(dataService.GetQuestionById(Convert.ToInt32(form["questionId"])), dataService.GetUserById(this.Id), form["commentBody"]);
            return Redirect("/Questions/Details/" + form["questionId"]);
        }

        [HttpPost]
        [Member]
        public ActionResult AddCommentToAnswer()
        {
            var form = Request.Form;
            dataService.AddCommentToAnswer(dataService.GetAnswerById(Convert.ToInt32(form["answerId"])), dataService.GetUserById(this.Id), form["commentBody"]);
            return Redirect("/Questions/Details/" + form["questionId"]);
        }

        [HttpGet]
        [Member]
        public ActionResult Report(int id) {
            try
            {
                dataService.ReportComment(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex) {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Moderator]
        public ActionResult Delete(int id)
        {
            try
            {
                dataService.DeleteComment(id);
                return Redirect(Request.UrlReferrer.ToString());
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
                dataService.ClearCommentState(id);
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
