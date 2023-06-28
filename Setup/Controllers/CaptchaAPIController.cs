using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Setup.Controllers
{
    [Route("api/Captcha")]
    [ApiController]
    public class CaptchaAPIController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        // To keep API-keys from public and GitRepo.
        private static readonly string jsonFileName = "APIkey.json";
        private static readonly string path = Path.Combine(Environment.CurrentDirectory, @"..\Restricted\", jsonFileName);

        [HttpPost]
        public async Task<ObjectResult> PostAsync()
        {
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

            string? responseToken = JsonSerializer.Deserialize<string>(requestContent);

            if(responseToken == null || responseToken == default)
            {
                jsonResponse = JsonSerializer.Serialize("ERROR: No Token");
                return BadRequest(jsonResponse);
            }

            int CaptchaVerificationResult = await VerifyReCaptcha(responseToken);

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

            if (CaptchaVerificationResult == 1)
            {
                jsonResponse = JsonSerializer.Serialize("ReCAPTCHA passed succesfully");
                return Ok(jsonResponse);
            }

            jsonResponse = JsonSerializer.Serialize("Something went wrong");
            return StatusCode(500, jsonResponse);
        }

        /// <summary>
        /// Connects with Google ReCAPTCHA API to verify user CAPTCHA.
        /// Returns int indicating if ReCAPTCHA verification was passed succesfully.
        /// For more info:
        /// https://developers.google.com/recaptcha/docs/verify
        /// </summary>
        /// <returns> int: -1 = SecretKey Failure, 0 = CAPTCHA failed, 1 = CAPTCHA passed </returns>
        public async Task<int> VerifyReCaptcha(string responseToken)
        {
            // <-- Preperation for reCAPTCHA Verification request -->
            string recaptchaSecret = ""; // Secret key
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                JsonNode keyNode = JsonNode.Parse(json)!;
                recaptchaSecret = (string)keyNode!["ReCaptchaSecret"]!;
            }

            if (recaptchaSecret == "") return -1;

            // <-- Construct reCAPTCHA API verification and SendGridExecute -->
            string recaptchaUrl = "https://www.google.com/recaptcha/api/siteverify"; // URL to the reCAPTCHA server
            string fullRequestUrl = $"{recaptchaUrl}?secret={recaptchaSecret}&response={responseToken}";

            var httpResult = await client.GetAsync(fullRequestUrl);
            if (httpResult.StatusCode != HttpStatusCode.OK) return 0;

            var responseString = await httpResult.Content.ReadAsStringAsync();
            ReCaptchaResponse? googleResult = JsonSerializer.Deserialize<ReCaptchaResponse>(responseString);

            if (googleResult == null) return 0;

            if (googleResult.success && googleResult.score > 0.5) return 1;

            return 0;
        }
    }
}
