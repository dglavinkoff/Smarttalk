using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTalk.Security
{
    public class ModeratorAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["Role"].ToString() == "Moderator" || httpContext.Session["Role"].ToString() == "Admin")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}