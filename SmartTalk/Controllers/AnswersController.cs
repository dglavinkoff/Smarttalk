using SmartTalk.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTalk.Controllers
{
    public class AnswersController : BaseController
    {

        [HttpGet]
        [Member]
        public ActionResult Report(int id)
        {
            try {
                dataService.ReportAnswer(id);
                return Redirect("/Questions/Details/" + dataService.GetAnswerById(id).Question.Id.ToString());
            }
            catch(ArgumentException ex)
            {
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
                dataService.DeleteAnswer(id);
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (ArgumentException ex)
            {
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
                dataService.ClearAnswerState(id);
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
