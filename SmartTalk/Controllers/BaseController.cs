using SmartTalk.Models;
using SmartTalk.Services;
using SmartTalk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace SmartTalk.Controllers
{
    public abstract class BaseController : Controller
    {
        private string username = null;

        public string Username
        {
            get
            {
                if (this.username == null)
                {
                    this.username = Session["Username"].ToString();
                }
                return username;
            }
        }

        private int id = 0;

        public int Id
        {
            get
            {
                if (this.id == 0 && Session["Id"] != null)
                {                 
                    this.id = (int)Session["Id"];
                }
                return this.id;
            }
        }

        protected ActionResult RedirectToHomePage() {
            var viewModel = new AccountHomeViewModel();
            var reader = XmlReader.Create("http://msdn.microsoft.com/bg-bg/magazine/rss/default(en-us).aspx?z=z&iss=1");
            var news = SyndicationFeed.Load(reader);
            viewModel.News = new List<FeedItemViewModel>();
            foreach (var item in news.Items)
            {
                viewModel.News.Add(new FeedItemViewModel
                {
                    Title = item.Title.Text,
                    Link = item.Links[0].Uri.ToString()
                });
            }
            if (Session["Id"] != null)
            {
                viewModel.Notifications = new List<Notification>(dataService.GetNotifications(Id));
                return View("Index", viewModel);
            }
            else
            {
                return View("Index",viewModel);
            }
        }

        public DataService dataService = new DataService();
    }
}
