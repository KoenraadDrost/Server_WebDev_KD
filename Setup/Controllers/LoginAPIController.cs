using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System.Text.Json;
using System.Text;

namespace Setup.Controllers
{
    [Route("api/Login")]
    [ApiController]
    public class LoginAPIController : ControllerBase
    {
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

            // Catch if received register information is invalid
            try
            {
                InputUser = JsonSerializer.Deserialize<UserLoginModel>(requestContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // TODO: Return message to client-side explaining to user what part of the register form was not propperly filled in.
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(InputUser.Email, InputUser.Password, InputUser.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                //bool check = _signInManager.IsSignedIn(User);
                _logger.LogInformation("User logged in.");
                //Response.Cookies.Append();
                return Ok("Login succesfull");
            }

            return Ok("Login succesfull??");
        }
    }
}
