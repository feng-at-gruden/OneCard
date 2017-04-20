using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OneCard.Controllers
{
    public class ToolController : BaseController
    {
        //
        // GET: /Tool/

        public ActionResult Import()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(String em)
        {
            return View();
        }

    }
}
