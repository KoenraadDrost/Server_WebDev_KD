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
using System.Net;

namespace Setup.Controllers
{
    // <hostingUrl>/api/Email
    [Route("api/Email")]
    [ApiController]
    public class EmailAPIController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        // To keep API-keys from public and GitRepo.
        private static readonly string jsonFileName = "APIkey.json";
        private static readonly string path = Path.Combine(Environment.CurrentDirectory, @"..\Restricted\", jsonFileName);

        [HttpPost("Send")]
        public async Task<ObjectResult> PostAsync()
        {
            Console.WriteLine("Incoming Email request through POST.");

            string jsonResponse;

            // <-- Deserialize the requestdata and turn it into an Contactmail implementation -->
            var request = HttpContext.Request;
            var requestContent = "";
            request.EnableBuffering();
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestContent = await reader.ReadToEndAsync();
            }
            request.Body.Position = 0;

            ContactMail? contactMail = JsonSerializer.Deserialize<ContactMail>(requestContent);

            if(contactMail == null)
            {
                jsonResponse = JsonSerializer.Serialize("ERROR: Could not reconstitute email.");
                return BadRequest(jsonResponse);
            }

            // <-- Check ReCaptcha verification -->
            string jsonString = JsonSerializer.Serialize<string>(contactMail.Verification);
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7095/api/Captcha");
            httpRequest.Content = new StringContent(jsonString);

            HttpResponseMessage response = await client.SendAsync(httpRequest);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(response);
            }

            // <-- Send Email through SendGrid -->
            bool emailSucces = await SendGridExecute(contactMail);

            if (emailSucces)
            {
                jsonResponse = JsonSerializer.Serialize("Email sent succesfully");
                return Ok(jsonResponse);
            }

            jsonResponse = JsonSerializer.Serialize("Something went wrong");
            return StatusCode(500, jsonResponse);
        }


        /// <summary>
        /// Send Email via SendGRid.
        /// </summary>
        /// <param name="contactMail"></param>
        /// <returns></returns>
        static async Task<bool> SendGridExecute(ContactMail contactMail)
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
                return false;
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

            //TODO: properly handle SendGrid Response
            if(response.StatusCode == HttpStatusCode.Accepted) return true;
            return false;
        }
    }
}
