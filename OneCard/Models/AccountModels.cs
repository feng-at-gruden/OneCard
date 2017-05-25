﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace OneCard.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class UserViewModel
    {
        [Display(Name = "ID")]
        public int ID { get; set; }

        [Display(Name = "部门ID")]
        public int RoleId { get; set; }

        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "确认密码")]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        public string ConfirmPassword { get; set; }

        [Display(Name = "真实姓名")]
        public string RealName { get; set; }

        [Display(Name = "邮箱地址")]
        public string Email { get; set; }

        [Display(Name = "部门")]
        public string RoleName { get; set; }

        [Display(Name = "注册时间")]
        public DateTime? RegisterTime { get; set; }

        [Display(Name = "最后登录时间")]
        public DateTime? LastLoginTime { get; set; }

        public Boolean Locked {get; set;}

        public IEnumerable<UserRoleViweModel> Roles { get; set; }
    }

    public class UserRoleViweModel
    {
        public int ID { get; set; }

        public string Role { get; set; }


    }


    public class ChangeUserDetailModel
    {
        [Display(Name = "真实姓名")]
        public string RealName { get; set; }

        [Display(Name = "邮箱地址")]
        public string Email { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
