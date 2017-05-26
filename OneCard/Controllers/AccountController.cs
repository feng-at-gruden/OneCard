using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
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
                if(u.Locked)
                {
                    ModelState.AddModelError("", "您的帐户已被锁定，请与系统管理员联系！");
                    return View();
                }

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
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");
            ChangeUserDetailModel model = new ChangeUserDetailModel
            { 
                RealName = CurrentUser.RealName,
                Email = CurrentUser.Email,
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
            if (string.IsNullOrWhiteSpace(model.RealName))
            {
                ModelState.AddModelError("", "请输入真实姓名。");
                return View(model);
            }
            if( !string.IsNullOrWhiteSpace(model.OldPassword) ||
                !string.IsNullOrWhiteSpace(model.NewPassword) ||
                !string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                if (!model.OldPassword.Equals(cu.Password))
                {
                    ModelState.AddModelError("", "请输入正确的当前密码。");
                    return View(model);
                }
                if(string.IsNullOrWhiteSpace(model.NewPassword) ||
                    string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "请输入新密码及确认密码。");
                    return View(model);
                }
                if (!model.NewPassword.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "新密码/确认密码不一致，请重试。");
                    return View(model);
                }
                CurrentUser.Password = model.NewPassword;
            }

            CurrentUser.Email = model.Email;
            CurrentUser.RealName = model.RealName;
            db.SaveChanges();
            ViewBag.SuccessMessage = "你的信息已更新";

            Log("修改个人登录密码");
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
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
                            Locked = row.Locked,
                        };
            return View(model);
        }

        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
        public ActionResult UserInfo(int id)
        {
            return View(getUserInfoViewModel(id));
        }

        [HttpPost]
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
        [ValidateAntiForgeryToken]
        public ActionResult UserInfo(UserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.RealName))
                ModelState.AddModelError("", "请输入真实姓名");
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError("", "请输入登录密码");
            if (ModelState.IsValid)
            {
                var u = db.Users.SingleOrDefault(m => m.Id == model.ID);
                u.RealName = model.RealName;
                u.RoleId = model.RoleId;
                if(!string.IsNullOrWhiteSpace(model.Password))
                    u.Password = model.Password;
                db.SaveChanges();
                Log("修改用户信息:" + u.UserName);
                ViewBag.SuccessMessage = "用户信息已更新！";
            }
            return View(getUserInfoViewModel(model.ID));
        }

        [OneCardAuth(Roles = "管理员")]
        public ActionResult Create()
        {
            var model = new UserViewModel
            {
                Roles = from r in db.UserRole
                        select new UserRoleViweModel
                        {
                            ID = r.Id,
                            Role = r.Role,
                        }
            };
            return View(model);
        }

        [HttpPost]
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName))
                ModelState.AddModelError("", "请输入登录账号");
            if (string.IsNullOrWhiteSpace(model.RealName))
                ModelState.AddModelError("", "请输入真实姓名");
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError("", "请输入登录密码");            
            if(string.IsNullOrWhiteSpace(model.ConfirmPassword))
                ModelState.AddModelError("","请输入确认密码");
            else if (model.ConfirmPassword != model.Password)
                ModelState.AddModelError("", "两次密码输入不一致");
            if(model.RoleId<=0)
                ModelState.AddModelError("", "请选择用户部门");

            if (ModelState.IsValid)
            {
                var i = db.Users.Count(m => m.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase));
                if(i>0)
                {
                    ModelState.AddModelError("", "用户名已存在");                    
                }
                else
                {
                    db.Users.Add(new User
                    {
                        UserName = model.UserName,
                        RealName = model.RealName,
                        RoleId = model.RoleId,
                        Password = model.Password,
                        RegisterTime = DateTime.Now,
                    });
                    db.SaveChanges();
                    ViewBag.SuccessMessage = "添加用户成功！";
                    Log("添加用户:" + model.UserName);
                }                 
            }
            model.Roles = from r in db.UserRole
                          select new UserRoleViweModel
                          {
                              ID = r.Id,
                              Role = r.Role,
                          };
            return View(model);
        }


        [HttpPost]
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var u = db.Users.SingleOrDefault(m => m.Id == id);
            if(u!=null)
            {
                db.Users.Remove(u);
                db.SaveChanges();
                Log("删除用户:" + u.UserName);
                ViewBag.SuccessMessage = "删除用户成功！";
            }
            else
            {
                ModelState.AddModelError("", "找不到指定用户");
            }
            return RedirectToAction("UserList");
        }

        [HttpPost]
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
        [ValidateAntiForgeryToken]
        public ActionResult Lock(int id)
        {
            var u = db.Users.SingleOrDefault(m => m.Id == id);
            if (u != null)
            {
                u.Locked = true;
                db.SaveChanges();
                Log("锁定用户:" + u.UserName);
                ViewBag.SuccessMessage = "锁定用户成功！";
            }
            else
            {
                ModelState.AddModelError("", "找不到指定用户");
            }
            return RedirectToAction("UserList");
        }

        [HttpPost]
        [OneCardAuth(Roles = Constants.Roles.ROLE_ADMIN + "," + Constants.Roles.ROLE_IT)]
        [ValidateAntiForgeryToken]
        public ActionResult Unlock(int id)
        {
            var u = db.Users.SingleOrDefault(m => m.Id == id);
            if (u != null)
            {
                u.Locked = false;
                db.SaveChanges();
                Log("解锁用户:" + u.UserName);
                ViewBag.SuccessMessage = "解锁用户成功！";
            }
            else
            {
                ModelState.AddModelError("", "找不到指定用户");
            }
            return RedirectToAction("UserList");
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
                        Email = row.Email,
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
