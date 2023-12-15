using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System.Text;
using System.Text.Json;

namespace Setup.Controllers
{
    [Route("api/Register")]
    [ApiController]
    public class RegisterAPIController : ControllerBase
    {
        private readonly SignInManager<ChatUser> _signInManager;
        private readonly UserManager<ChatUser> _userManager;
        private readonly IUserStore<ChatUser> _userStore;
        private readonly IUserEmailStore<ChatUser> _emailStore;
        private readonly ILogger<RegisterAPIController> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterAPIController(
            UserManager<ChatUser> userManager,
            IUserStore<ChatUser> userStore,
            SignInManager<ChatUser> signInManager,
            ILogger<RegisterAPIController> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public UserInputModel? InputUser { get; set; }

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
                InputUser = JsonSerializer.Deserialize<UserInputModel>(requestContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // TODO: Return message to client-side explaining to user what part of the register form was not propperly filled in.
            }

            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, InputUser.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, InputUser.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, InputUser.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
            }

            return Ok("Register Succesfull");
        }

        //public IHttpActionResult Post(RegisterApiModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var user = new ApplicationUser
        //    {
        //        Email = model.Email,
        //        UserName = model.Email,
        //        EmailConfirmed = true
        //    };

        //    var result = UserManager.Create(user, model.Password);
        //    return result.Succeeded ? Ok() : GetErrorResult(result);
        //}

        private ChatUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ChatUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ChatUser)}'. " +
                    $"Ensure that '{nameof(ChatUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ChatUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ChatUser>)_userStore;
        }
    }
}
