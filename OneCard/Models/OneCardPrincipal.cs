using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}