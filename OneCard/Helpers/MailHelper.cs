using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.IO;
using OneCard.Models;

namespace OneCard.Helpers
{
    public class MailHelper
    {
        public const int RESULT_SUCCESS = 0;
        public const int RESULT_NO_ADDRESS = -1;

        public static int SendMail(string title, string toAddress, Stream attachmentStream, string attachmentName)
        {
            string smtpHost = ConfigurationHelper.GetSystemSettingString(Constants.ConfigurationKey.CON_KEY_SMTP_HOST);
            int smtpPort = ConfigurationHelper.GetSystemSettingInt(Constants.ConfigurationKey.CON_KEY_SMTP_PORT);
            string fromAddress = ConfigurationHelper.GetSystemSettingString(Constants.ConfigurationKey.CON_KEY_SMTP_FROM_ADDRESS);
            string fromPassword = ConfigurationHelper.GetSystemSettingString(Constants.ConfigurationKey.CON_KEY_SMTP_FROM_PASSWORD);

            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };
            using (var message = new MailMessage(
                fromAddress,
                toAddress, 
                title, 
                HttpUtility.HtmlEncode(title + "\r\n\r\n\r\n发自酒店一卡通查询系统")))
            {
                if(attachmentStream != null)
                {
                    Attachment objMailAttachment = new Attachment(attachmentStream, attachmentName);
                    message.Attachments.Add(objMailAttachment);
                }
                smtp.Send(message);
            }

            return RESULT_SUCCESS;
        }


    }
}