using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MaxMaster.Controllers
{
    public class EmailController : ApiController
    {
        public bool SendEmail(string from, string from_name, string to, string subject, string body)
        {
            try
            {
                SmtpClient mailClient = new SmtpClient("smtp.1and1.com", 587);
                mailClient.Credentials = new NetworkCredential("phanim@maxtranssystems.com", "Phani@1072");
                mailClient.Port = 587;

                MailMessage message = new MailMessage();
                message.From = new MailAddress("phanim@maxtranssystems.com", "Max Master App");

                message.To.Add(new MailAddress(to));
                message.Bcc.Add(new MailAddress("priyankakodali611@gmail.com"));

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                mailClient.EnableSsl = true;
                mailClient.Send(message);

                return true;
            }

            catch (Exception ex)
            {
                return false;
            }

        }
    }
}