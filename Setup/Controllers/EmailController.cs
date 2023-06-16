﻿using Microsoft.AspNetCore.Http;
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
    // <hostingUrl>/api/Email
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        // To keep API-keys from public and GitRepo.
        private static readonly string jsonFileName = "APIkey.json";
        private static readonly string path = Path.Combine(Environment.CurrentDirectory, @"..\Restricted\", jsonFileName);

        [HttpPost]
        public async Task<ObjectResult> PostAsync()
        {
            //TODO: Remove testprint
            Console.WriteLine("Email POST triggered");
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

            //TODO: Remove printtest
            Console.WriteLine($"API call succesfull: {requestContent}");

            ContactMail? contactMail = JsonSerializer.Deserialize<ContactMail>(requestContent);

            if(contactMail == null)
            {
                jsonResponse = JsonSerializer.Serialize("ERROR: Could not reconstitute email.");
                return BadRequest(jsonResponse);
            }

            int CaptchaVerificationResult = await VerifyReCaptcha(contactMail);

            if (CaptchaVerificationResult == -1)
            {
                jsonResponse = JsonSerializer.Serialize("ERROR: reCaptcha sk1");
                return BadRequest(jsonResponse);
            }

            if (CaptchaVerificationResult == 0)
            {
                jsonResponse = JsonSerializer.Serialize("ERROR: Captcha failed");
                return BadRequest(jsonResponse);
            }

            //TODO: reënable mail sending
            //Execute(contactMail).Wait();

            jsonResponse = JsonSerializer.Serialize("Email sent succesfully");
            return Ok(jsonResponse);
        }

        /// <summary>
        /// Connects with Google ReCAPTCHA API to verify user CAPTCHA.
        /// Returns int indicating if ReCAPTCHA verification was passed succesfully.
        /// </summary>
        /// <returns> int: -1 = SecretKey Failure, 0 = CAPTCHA failed, 1 = CAPTCHA passed </returns>
        public async Task<int> VerifyReCaptcha(ContactMail contactMail)
        {
            // <-- Preperation for reCAPTCHA Verification request -->
            string recaptchaSecret = ""; // Secret key
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                JsonNode keyNode = JsonNode.Parse(json)!;
                recaptchaSecret = (string)keyNode!["ReCaptchaSecret"]!;
            }

            if(recaptchaSecret == "")
            {
                return -1;
            }

            // <-- Construct reCAPTCHA API verification and Execute -->
            string responseToken = contactMail.Verification;
            string recaptchaUrl = "https://www.google.com/recaptcha/api/siteverify"; // URL to the reCAPTCHA server
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), recaptchaUrl);

            // https://www.youtube.com/watch?v=t-SVLZxC6CQ
            // https://developers.google.com/recaptcha/docs/verify



            //string recaptchaResponse = contactMail.Verification;
            //recaptcha = file_get_contents($recaptcha_url.'?secret='.$recaptcha_secret.'&response='.$recaptcha_response); // Send request to the server
            //$recaptcha = json_decode($recaptcha); // Decode the JSON response
            //if ($recaptcha->success == true && $recaptcha->score >= 0.5 && $recaptcha->action == "contact"){ // If the response is valid
            //    // run email send routine
            //    $success_output = 'Your message was sent successfully.'; // Success message
            //}else
            //{
            //    $error_output = 'Something went wrong. Please try again later'; // Error message
            //}

            return 1;
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
