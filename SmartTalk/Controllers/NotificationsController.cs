using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTalk.Controllers
{
    public class NotificationsController : BaseController
    {
        public ActionResult Delete(int id) {
            try
            {
                dataService.DeleteNotificationById(id);
                return RedirectToHomePage();
            }
            catch (ArgumentException ex) {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }

    }
}
