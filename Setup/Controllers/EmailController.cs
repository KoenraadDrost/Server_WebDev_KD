using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace Setup.Controllers
{

    // SendGrid API KEY: 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
    // <hostingUrl>/api/Email
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        // To keep API-keys from public and GitRepo.
        private static readonly string jsonFileName = "APIkey.json";
        private static readonly string path = Path.Combine(Environment.CurrentDirectory, @"..\Restricted\", jsonFileName);

        [HttpPost]
        public async Task<ObjectResult> PostAsync()
        {
            //TODO: Remove testprint
            Console.WriteLine("Email POST triggered");
            string jsonResponse;

            var request = HttpContext.Request;
            var requestContent = "";
            request.EnableBuffering();
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestContent = await reader.ReadToEndAsync();
            }
            request.Body.Position = 0;

            //TODO: Remove printtest
            Console.WriteLine($"API call succesfull: {requestContent}");

            ContactMail? contactMail = JsonSerializer.Deserialize<ContactMail>(requestContent);

            string recaptchaUrl = "https://www.google.com/recaptcha/api/siteverify"; // URL to the reCAPTCHA server
            string recaptchaSecret = ""; // Secret key
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                JsonNode keyNode = JsonNode.Parse(json)!;
                Console.WriteLine($"jsonNode = {keyNode}");
                recaptchaSecret = (string)keyNode!["ReCaptchaSecret"]!;
            }

            if (recaptchaSecret == "")
            {
                Console.WriteLine("captcha apikey is empty.");
                jsonResponse = JsonSerializer.Serialize("No captcha provided");
                return BadRequest(jsonResponse);
            }

            //string recaptchaResponse = "$_POST['recaptchaResponse']";
            //$recaptcha = file_get_contents($recaptcha_url.'?secret='.$recaptcha_secret.'&response='.$recaptcha_response); // Send request to the server
            //$recaptcha = json_decode($recaptcha); // Decode the JSON response
            //if ($recaptcha->success == true && $recaptcha->score >= 0.5 && $recaptcha->action == "contact"){ // If the response is valid
            //    // run email send routine
            //    $success_output = 'Your message was sent successfully.'; // Success message
            //}else
            //{
            //    $error_output = 'Something went wrong. Please try again later'; // Error message
            //}

            //TODO: reënable mail sending
            //Execute(contactMail).Wait();

            jsonResponse = JsonSerializer.Serialize("Email sent succesfully");
            return Ok(jsonResponse);
        }

        static async Task Execute(ContactMail contactMail)
        {
            var verifiedEmailSenderAddress = "kdr.entertainment.dev@gmail.com";
            var emailReceiverAddress = "kdr.devmail@gmail.com";

            // To keep API-keys from public and GitRepo.

            var sendgridApiKey = "";
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                JsonNode keyNode = JsonNode.Parse(json)!;
                sendgridApiKey = (string)keyNode!["SendgridApiKey"]!;
            }

            if (sendgridApiKey == "")
            {
                Console.WriteLine("send apikey is empty.");
                return;
            }

            var client = new SendGridClient(sendgridApiKey);
            var from = new EmailAddress(verifiedEmailSenderAddress, "[KDR-webdev] Sendgridmail");
            var subject = "[KDR-webdev]" + contactMail.Subject;
            var to = new EmailAddress(emailReceiverAddress, "Example User");
            var plainTextContent = "Plain text delivered";
            var htmlContent = $"{contactMail.Message} <br><br>" +
                $"<strong>Mail sent in name of: {contactMail.Name}, Email Address: <a>{contactMail.Email}</a> .</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            Console.WriteLine(response);
        }
    }
}
