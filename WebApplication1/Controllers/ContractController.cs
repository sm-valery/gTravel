using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class ContractController : Controller
    {
        //
        // GET: /Contract/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {

            return View();
        }

        public ActionResult Contract(FormCollection oform)
        {
            string aaa = "";
            return View();
        }
	}
}