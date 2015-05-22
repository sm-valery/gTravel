using System;
using System.Web.Mvc;
using gTravel.Models;
using gTravel.Servises;
using Microsoft.AspNet.Identity;

namespace gTravel
{
    public class ContractUserFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            const string Key = "contractid";

            if (filterContext.ActionParameters.ContainsKey(Key))
            {
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                   
                    if(!new ContractService().spContract(new goDbEntities(),  filterContext.HttpContext.User.Identity.GetUserId(),
                        Guid.Parse(filterContext.ActionParameters[Key].ToString())))
                    {
                        filterContext.Result = new HttpNotFoundResult("ContractId: wrong id");
                         //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
                    }
                   
                }
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}