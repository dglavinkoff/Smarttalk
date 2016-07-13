using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartTalk
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {          
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ForgottenPassword",
                url: "Account/ForgottenPassword/{username}",
                defaults: new { controller = "Account", action = "ForgottenPassword", username = "{username}" }
            );

            routes.MapRoute(
                name: "ShowQuestions",
                url: "Questions/ShowTenMostRecent/{page}",
                defaults: new { controller = "Questions", action = "ShowTenMostRecent", page = "{page}" }
            );

            routes.MapRoute(
                name: "ReviewNews",
                url: "News/Review/{channel}/{title}",
                defaults: new { controller = "News", action = "Review", channel = "{channel}", title = "{title}" }
            );

            routes.MapRoute(
                name: "CreateCategory",
                url: "Categories/Create/{name}",
                defaults: new { controller = "Categories", action = "Create", name = "{name}" }
            );

            routes.MapRoute(
                name: "GetUsers",
                url: "Account/TopTenUsers/{page}",
                defaults: new { controller = "Account", action = "TopTenUsers", page = "{page}" }
            );

            routes.MapRoute(
                name: "GetGroups",
                url: "Groups/TopTenGroups/{page}",
                defaults: new { controller = "Groups", action = "TopTenGroups", page = "{page}" }
            );

            routes.MapRoute(
                name: "RequestCategory",
                url: "Categories/Request/{name}",
                defaults: new { controller = "Categories", action = "RequestCategory", name="{name}" }
            );

            routes.MapRoute(
                name: "SearchQuestionsByCategory",
                url: "Questions/SearchByCategory/{id}/{questionSubstring}/{page}",
                defaults: new { controller = "Questions", action = "SearchByCategory", id = "{id}", questionSubstring = "{questionSubstring}", page = "{page}" }
            );

            routes.MapRoute(
                name: "ShowCategoryQuestions",
                url: "Categories/ShowQuestions/{id}/{page}",
                defaults: new { controller = "Categories", action = "ShowQuestions", id = "{id}", page = "{page}" }
            );

            routes.MapRoute(
                name: "SearchQuestions",
                url: "Questions/Search/{questionSubstring}/{page}",
                defaults: new { controller = "Questions", action = "Search", questionSubstring = "{questionSubstring}", page = "{page}" }
            );

            routes.MapRoute(
                name: "SearchUsers",
                url: "Account/SearchUsers/{username}/{page}",
                defaults: new { controller = "Account", action = "SearchUsers", username = "{username}", page = "{page}" }
            );

            routes.MapRoute(
                name: "SearchGroups",
                url: "Groups/SearchGroups/{nameSubstring}/{page}",
                defaults: new { controller = "Groups", action = "SearchGroups", nameSubstring = "{nameSubstring}", page = "{page}" }
            );

            routes.MapRoute(
                name: "AskQuestion",
                url: "Questions/AskQuestion/{groupId}",
                defaults: new { controller = "Questions", action = "AskQuestion", groupId = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SpecialRoute",
                url: "{controller}/{action}/{userId}/{groupId}",
                defaults: new { controller = "{controller}", action = "{action}", userId = "{userId}", groupId = "{groupId}" }
            );
        }
    }
}