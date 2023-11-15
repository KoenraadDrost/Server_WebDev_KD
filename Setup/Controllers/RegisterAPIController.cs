using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Setup.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;

namespace Setup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterAPIController : ControllerBase
    {
        // See: "D:\My Programming Projects\Visual Studio Project\WebApp1\Areas\Identity\Pages\Account\Register.cshtml.cs"



        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterAPIController> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterAPIController(
            SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager, 
            IUserStore<IdentityUser> userStore, 
            IUserEmailStore<IdentityUser> emailStore, 
            ILogger<RegisterAPIController> logger, 
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = emailStore;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        // Probably don't need this, as Ít's only used for page controlers, not API.
        public string ReturnUrl { get; set; }

        // Probably can remove this, Not using an external Login provider like Google or Facebook.
        // public IList<AuthenticationScheme> ExternalLogins { get; set; }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<ObjectResult> OnPostAsync(string returnUrl = null)
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
            InputModel? input;
            try
            {
                input = JsonSerializer.Deserialize<InputModel>(requestContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // TODO: Return message to client-side explaining to user what part of the register form was not propperly filled in.
            }

            // Model Valuidation issue from client-side, for possible fix, see: https://stackoverflow.com/questions/20850035/can-you-validate-a-model-instance-using-properties-from-its-controller
            if (ModelState.IsValid)
            {
                var user = new User();

                await _userStore.SetUserNameAsync(user, input.Username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // TODO: convert E-mail confirmation to propper MVC.
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                }
            }

                // <-- copied code from microsoft below -->

                returnUrl ??= Url.Content("~/");
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            jsonResponse = JsonSerializer.Serialize("Something went wrong");
            return StatusCode(500, jsonResponse);
            jsonResponse = JsonSerializer.Serialize("Register Succesfull");
            return Ok("Register Succesfull");
        }

        private IdentityUser CreateUser()
        {
            try
            {
                // TODO: Change to "User"?
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
