using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System.Text.Json;
using System.Text;
using System.Net;

namespace Setup.Controllers
{
    [Route("api/Login")]
    [ApiController]
    public class LoginAPIController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly SignInManager<ChatUser> _signInManager;
        private readonly ILogger<LoginAPIController> _logger;

        public LoginAPIController(SignInManager<ChatUser> signInManager, ILogger<LoginAPIController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public UserLoginModel? InputUser { get; set; }

        public async Task<ObjectResult> OnPostAsync()
        {
            Console.WriteLine("Incomming Login Request through Post");

            string jsonResponse;

            // <-- Deserialize the requestdata and turn it into an UserLoginModel implementation -->
            var request = HttpContext.Request;
            var requestContent = "";
            request.EnableBuffering();
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestContent = await reader.ReadToEndAsync();
            }
            request.Body.Position = 0;

            // Catch if received login information is invalid
            try
            {
                InputUser = JsonSerializer.Deserialize<UserLoginModel>(requestContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // TODO: Return message to client-side explaining to user what part of the register form was not propperly filled in.
                jsonResponse = JsonSerializer.Serialize("ERROR: Incomplete Login details.");
                return BadRequest(jsonResponse);
            }

            // <-- Check ReCaptcha verification -->
            string jsonString = JsonSerializer.Serialize<string>(InputUser.Verification);
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7095/api/Captcha");
            httpRequest.Content = new StringContent(jsonString);

            HttpResponseMessage response = await client.SendAsync(httpRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest(response);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(InputUser.Username, InputUser.Password, InputUser.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                //bool check = _signInManager.IsSignedIn(User);
                _logger.LogInformation("User logged in.");
                //Response.Cookies.Append();

                var loginResponse = new LoginResponse
                {
                    // TODO: actually retrieve username from Database in case Email is used instead of Username to login.
                    Username = InputUser.Username,
                    LoginTime = DateTime.Now,
                    Message = "Login succesfull"
                };

                jsonResponse = JsonSerializer.Serialize(loginResponse);
                return Ok(jsonResponse);
            }

            jsonResponse = JsonSerializer.Serialize("Login failed");
            return Ok(jsonResponse);
        }
    }
}
