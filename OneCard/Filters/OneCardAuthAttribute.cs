using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Principal;
using OneCard.Models;

namespace OneCard.Filters
{
    public class OneCardAuthAttribute : AuthorizeAttribute  
    {

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)//判断用户是否通过验证
                return false;

            string[] StrRoles = Roles.Split(',');//通过逗号来分割允许进入的用户角色
            if (string.IsNullOrWhiteSpace(Roles))//如果只要求用户登录，即可访问的话
                return true;

            bool isAccess = JudgeAuthorize(httpContext.User.Identity.Name, StrRoles);
            if (StrRoles.Length > 0 && !isAccess) //先判断是否有设用户权限，如果没有不允许访问
                return false;


            return true;
        }


        private bool JudgeAuthorize(string UserName, string[] StrRoles)
        {
            string UserAuth = GetRole(UserName); 
            return StrRoles.Contains(UserAuth, StringComparer.OrdinalIgnoreCase);  //忽略大小写
        }

        // 返回用户对应的角色， 在实际中， 可以从SQL数据库中读取用户的角色信息  
        private string GetRole(string name)
        {
            using (onecardEntities db = new onecardEntities())
            {
                var u = db.Users.FirstOrDefault(m => m.UserName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (u != null)
                    return u.UserRole.Role;
                else
                    return null;
            }
        }


    }

}