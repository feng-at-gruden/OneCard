using System;
using System.Collections.Generic;

namespace OneCard.Models
{
    public class Constants 
    {

        public class Roles
        {
            public const string ROLE_ADMIN = "超级管理员";
            public const string ROLE_IT = "IT部";
            public const string ROLE_FINANCE = "财务部";
            public const string ROLE_DIET = "餐饮部";
            public const string ROLE_LOBBY = "前厅部";
        }


        public class ConfigurationKey
        {
            public const string CON_KEY_SMTP_HOST = "_smtpHost";
            public const string CON_KEY_SMTP_PORT = "_smtpHostPort";
            public const string CON_KEY_SMTP_FROM_ADDRESS = "_smtpFromAddress";
            public const string CON_KEY_SMTP_FROM_PASSWORD = "_smtpFromPassword";
        }


    }
}
