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
            public const string CON_KEY_SMTP_PASSWORD = "_smtpFromPassword";
            public const string CON_KEY_SMTP_NEED_AUTH = "_smtpAuth";
        }


        public class PackageDisplay
        {
            public const string Package1 = "R-BF";
            public const string Package2 = "F-BF";
        }

        public string[] PackageCode = new string[] { "ABF", "ABF 350", "ABF 550", "ABF BFR", "ABF COMP", "ABF COR", "ABF CPC", "ABF TEST", "ABF VAT", "ABF - CPC" };

       

    }
}
