using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace gTravel
{
    public class UserIdFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            const string Key = "userid";

            if (filterContext.ActionParameters.ContainsKey(Key))
            {
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    filterContext.ActionParameters[Key] = filterContext.HttpContext.User.Identity.GetUserId();
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}