﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using OneCard.Filters;
using OneCard.Models;

namespace OneCard.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            /*if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }*/
            var u = db.Users.FirstOrDefault(m => m.UserName.Equals(model.UserName, StringComparison.CurrentCultureIgnoreCase) && m.Password.Equals(model.Password));
            if(u!=null){
                FormsAuthentication.SetAuthCookie(model.UserName, false);
                FormsAuthentication.RedirectFromLoginPage(u.UserName, false);
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, u.UserName, DateTime.Now,
                DateTime.Now.AddMinutes(120), false, string.Format("{0}|{1}", u.UserRole.Role.ToString(), u.RealName));
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);
                HttpContext.User = new OneCardPrincipal(u.UserRole.Role.ToString(), u.RealName, HttpContext.User.Identity);

                Log("登录系统", u);
                u.LastLoginTime = DateTime.Now;
                db.SaveChanges();
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", "提供的用户名或密码不正确。");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        public ActionResult LogOff()
        {
            Log("退出系统");
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // 尝试注册用户
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // 只有在当前登录用户是所有者时才取消关联帐户
            if (ownerAccount == User.Identity.Name)
            {
                // 使用事务来防止用户删除其上次使用的登录凭据
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");
            ChangeUserDetailModel model = new ChangeUserDetailModel
            { 
                RealName = CurrentUser.RealName,
            };
            return View(model);
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(ChangeUserDetailModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");
            var cu = CurrentUser;
            if (string.IsNullOrWhiteSpace(model.RealName) ||
                string.IsNullOrWhiteSpace(model.OldPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "请输入完整信息。");
                return View(model);
            }
            if(!model.OldPassword.Equals(cu.Password))
            {
                ModelState.AddModelError("", "请输入正确的当前密码。");
                return View(model);
            }
            if(!model.NewPassword.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "新密码不一致，请重试。");
                return View(model);
            }

            CurrentUser.RealName = model.RealName;
            CurrentUser.Password = model.NewPassword;
            db.SaveChanges();
            ViewBag.SuccessMessage = "你的信息已更新";

            Log("修改个人登录密码");
            return View(model);
        }

        [OneCardAuth(Roles = "管理员")]
        public ActionResult UserList()
        {
            var model = from row in db.Users
                        select new UserViewModel
                        {
                            ID = row.Id,
                            UserName = row.UserName,
                            RealName = row.RealName,
                            RoleId = row.RoleId.Value,
                            RoleName = row.UserRole.Role,
                            Password = row.Password,
                            LastLoginTime = row.LastLoginTime,
                            RegisterTime = row.RegisterTime,
                        };
            return View(model);
        }

        [OneCardAuth(Roles = "管理员")]
        public ActionResult UserInfo(int id)
        {
            return View(getUserInfoViewModel(id));
        }

        [HttpPost]
        [OneCardAuth(Roles = "管理员")]
        [ValidateAntiForgeryToken]
        public ActionResult UserInfo(UserViewModel model)
        {
            //TODO
            if (ModelState.IsValid)
            {
                var u = db.Users.SingleOrDefault(m => m.Id == model.ID);
                u.RealName = model.RealName;
                u.RoleId = model.RoleId;
                if(!string.IsNullOrWhiteSpace(model.Password))
                    u.Password = model.Password;
                db.SaveChanges();
                ViewBag.SuccessMessage = "用户信息已更新！";
            }
            return View(getUserInfoViewModel(model.ID));
        }


        [HttpPut]
        [OneCardAuth(Roles = "管理员")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel model)
        {
            //TODO            
            return View(model);
        }


        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // 如果当前用户已登录，则添加新帐户
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // 该用户是新用户，因此将要求该用户提供所需的成员名称
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // 将新用户插入到数据库
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // 检查用户是否已存在
                    if (user == null)
                    {
                        // 将名称插入到配置文件表
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "用户名已存在。请输入其他用户名。");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region 帮助程序
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // 请参见 http://go.microsoft.com/fwlink/?LinkID=177550 以查看
            // 状态代码的完整列表。
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "用户名已存在。请输入其他用户名。";

                case MembershipCreateStatus.DuplicateEmail:
                    return "该电子邮件地址的用户名已存在。请输入其他电子邮件地址。";

                case MembershipCreateStatus.InvalidPassword:
                    return "提供的密码无效。请输入有效的密码值。";

                case MembershipCreateStatus.InvalidEmail:
                    return "提供的电子邮件地址无效。请检查该值并重试。";

                case MembershipCreateStatus.InvalidAnswer:
                    return "提供的密码取回答案无效。请检查该值并重试。";

                case MembershipCreateStatus.InvalidQuestion:
                    return "提供的密码取回问题无效。请检查该值并重试。";

                case MembershipCreateStatus.InvalidUserName:
                    return "提供的用户名无效。请检查该值并重试。";

                case MembershipCreateStatus.ProviderError:
                    return "身份验证提供程序返回了错误。请验证您的输入并重试。如果问题仍然存在，请与系统管理员联系。";

                case MembershipCreateStatus.UserRejected:
                    return "已取消用户创建请求。请验证您的输入并重试。如果问题仍然存在，请与系统管理员联系。";

                default:
                    return "发生未知错误。请验证您的输入并重试。如果问题仍然存在，请与系统管理员联系。";
            }
        }
        #endregion


        private UserViewModel getUserInfoViewModel(int id)
        {
            var row = db.Users.SingleOrDefault(m => m.Id == id);
            UserViewModel u = new UserViewModel
                    {
                        ID = row.Id,
                        UserName = row.UserName,
                        RealName = row.RealName,
                        RoleId = row.RoleId.Value,
                        RoleName = row.UserRole.Role,
                        Password = row.Password,
                        LastLoginTime = row.LastLoginTime,
                        RegisterTime = row.RegisterTime,
                        Roles = from r in db.UserRole
                                select new UserRoleViweModel
                                {
                                    ID = r.Id,
                                    Role = r.Role,
                                }
                    };
            return u;
        }

    }
}
