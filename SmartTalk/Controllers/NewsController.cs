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
    public class NewsController : Controller
    {
        public ActionResult Review(string channel, string title)
        {
            if (channel == "Microsoft")
            {
                var reader = XmlReader.Create("http://msdn.microsoft.com/bg-bg/magazine/rss/default(en-us).aspx?z=z&iss=1");
                var allNews = SyndicationFeed.Load(reader);
                var item = allNews.Items.Single(x => x.Title.Text == title);
                var viewModel = new NewsReviewViewModel();
                viewModel.Title = item.Title.Text;
                viewModel.Content = item.Summary.Text;
                return View(viewModel);
            }
            else
            {
                ViewBag.Message = "Channel is not supported.";
                return View("Error");
            }
        }

    }
}
