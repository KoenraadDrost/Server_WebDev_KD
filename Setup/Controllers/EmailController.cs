using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace Setup.Controllers
{

    // SendGrid API KEY: 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
    // <hostingUrl>/api/Email
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        [HttpPost]
        public async Task PostAsync()
        {
            //TODO: Remove testprint
            Console.WriteLine("Email POST triggered");

            var request = HttpContext.Request;
            var requestContent = "";
            request.EnableBuffering();
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestContent = await reader.ReadToEndAsync();
            }
            request.Body.Position = 0;

            Console.WriteLine($"API call succesfull: {requestContent}");

            ContactMail? contactMail = JsonSerializer.Deserialize<ContactMail>(requestContent);

            //TODO: reënable mail sending
            //Execute(contactMail).Wait();
        }

        static async Task Execute(ContactMail contactMail)
        {
            var verifiedEmailSenderAddress = "kdr.entertainment.dev@gmail.com";
            var emailReceiverAddress = "koenraad.drost@gmail.com";

            var apiKey = "";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(verifiedEmailSenderAddress, "[KDR-webdev] Sendgridmail");
            var subject = "[KDR-webdev]" + contactMail.Subject;
            var to = new EmailAddress(emailReceiverAddress, "Example User");
            var plainTextContent = "Plain text delivered";
            var htmlContent = $"{contactMail.Message} <br><br>" +
                $"<strong>Mail sent in name of: {contactMail.Name}, Email Address: <a>{contactMail.Email}</a> .</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
