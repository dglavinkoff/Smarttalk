using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;

namespace SmartTalk.ViewModels
{
    public class FeedItemViewModel
    {
        public string Title { get; set; }

        public string Link { get; set; }
    }

    public class NewsReviewViewModel
    {
        public string Title { get; set; }

        public string Content { get; set; }
    }
}