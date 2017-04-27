using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using OneCard.Models;

namespace OneCard.Controllers
{
    public class BaseController : Controller
    {
        protected onecardEntities db = new onecardEntities();


        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
                db = null;
            }
            base.Dispose(disposing);
        }


        protected User CurrentUser { get {
            FormsIdentity fi = HttpContext.User.Identity as FormsIdentity;
            string username = "admin";
            string password = "password";
            var u = db.Users.FirstOrDefault(m => m.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase) && m.Password.Equals(password));
            return u;
        } }
            

    }
}
