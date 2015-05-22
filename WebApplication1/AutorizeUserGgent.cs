using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace gTravel
{
    public class AutorizeUserGgent : AuthorizeAttribute
    {
    
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated) return base.AuthorizeCore(httpContext);

            mLib.SetCurrentUserAgent(httpContext.User.Identity.GetUserId());

            return base.AuthorizeCore(httpContext);
        }
    }
}