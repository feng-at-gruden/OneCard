using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Security.Principal;

namespace OneCard.Models
{

    public class OneCardPrincipal : IPrincipal
    {
        public string RoleName { get; set; }
        public string RealName { get; set; }

        private IIdentity _identity;

        public OneCardPrincipal(string roleName, string realName, IIdentity identity)
        {
            RealName = realName;
            RoleName = roleName;
            _identity = identity;
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }

        public bool IsInRole(string role)
        {
            return RoleName.IndexOf(role) >= 0;
        }


        public static OneCardPrincipal GetUser(HttpContext httpContext)
        {
            if (httpContext.Request.IsAuthenticated)
            {
                FormsIdentity fi = httpContext.User.Identity as FormsIdentity;
                if (fi != null)
                {
                    string[] userData = fi.Ticket.UserData.Split('|');
                    if (userData.Length == 2)
                    {
                        OneCardPrincipal newPrincipal = new OneCardPrincipal(userData[0],
                            userData[1],
                            httpContext.User.Identity);
                        return newPrincipal;
                    }
                    return null;
                }
                return null;
            }
            return null;
        } 
    }
}