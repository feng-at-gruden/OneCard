using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace OneCard.Helpers
{
    public class MailHelper
    {
        public const int RESULT_SUCCESS = 0;
        public const int RESULT_NO_ADDRESS = -1;

        public static string smtpHost = "smtp.qq.com";
        public static int smtpPort = 25;
        public static string fromAddress = "691427@qq.com";
        public static string fromPassword = "dadbvlhgkswzcbdi";

        public static int SendMail(string title, string toAddress, string filePath)
        {
            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress, title, HttpUtility.HtmlEncode("<h1>Test</h1>")))
            {
                smtp.Send(message);
            }

            return RESULT_SUCCESS;
        }


    }
}