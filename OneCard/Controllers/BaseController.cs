﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using OneCard.Models;
using OneCard.Filters;

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
            FormsIdentity fi = System.Web.HttpContext.Current.User.Identity as FormsIdentity;
            var u = db.Users.FirstOrDefault(m => m.UserName.Equals(fi.Name, StringComparison.CurrentCultureIgnoreCase));
            return u;
        } }
           
 
        protected void Log(string action)
        {
            db.Log.Add(new Log
            {
                UserId = CurrentUser.Id,
                Action = action,
                ActionTime = DateTime.Now
            });
            db.SaveChanges();
        }

    }
}
