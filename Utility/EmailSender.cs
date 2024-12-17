using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("graduationproject926", "zije nfgu bcol puyv")
            };

            return client.SendMailAsync(
                new MailMessage(from: "your.email@live.com",
                                to: email,
                                subject,
                                message
                                )
                { 
                    IsBodyHtml = true
                }

                );
        }

    }
}
