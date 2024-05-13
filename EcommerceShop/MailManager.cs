using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace EcommerceShop
{
    public class MailManager
    {
        private String MailSender { get; set; }
        private String MailAppPassword { get; set; }

        public MailManager()
        {
            MailSender = ConfigurationManager.AppSettings["MailSender"];
            MailAppPassword = ConfigurationManager.AppSettings["MailSenderAppPassword"];
        }

        public bool SendEmail(string szRecipient, string subject, string textContent, string otp, ref string errResponse)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    var smtp = new SmtpClient();
                    message.From = new MailAddress(MailSender);
                    message.To.Add(new MailAddress(szRecipient));
                    message.Subject = subject;
                    message.IsBodyHtml = true;

                    // Create a well-formatted HTML body
                    message.Body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <title>{subject}</title>
                        </head>
                        <body>
                            <p>{textContent}</p>
                            <p><strong>Your OTP is: {otp}</strong></p> 
                        </body>
                        </html>";

                    smtp.Port = 587;
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(MailSender, MailAppPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                    errResponse = "Message Sent";
                    return true;
                }
            }
            catch (Exception ex)
            {
                errResponse = ex.Message;
                return false;
            }
        }
    }
}