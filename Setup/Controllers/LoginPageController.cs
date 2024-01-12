using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Policy;
using System.Text.Json;

namespace Setup.Controllers
{
    public class LoginPageController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly SignInManager<ChatUser> _signInManager;
        private readonly ILogger<LoginPageController> _logger;

        public LoginPageController(SignInManager<ChatUser> signInManager, ILogger<LoginPageController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        [ActionName("Submit")]
        public async Task<IActionResult> SubmitAsync(UserLoginModel accountDetails, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            string jsonString = JsonSerializer.Serialize<UserLoginModel>(accountDetails);
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7095/api/Login");
            httpRequest.Content = new StringContent(jsonString);

            HttpResponseMessage response = await client.SendAsync(httpRequest);

            Uri uri = new Uri("Https://localhost:7095/");
            CookieContainer cookies = new CookieContainer();
            foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
                cookies.SetCookies(uri, cookieHeader);
            string cookieValue = cookies.GetCookies(uri).FirstOrDefault(c => c.Name == "LoginCookie")?.Value;

            //IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;

            Response.Cookies.Append("LoginCookie", cookieValue);
            // TODO: Receiving logincookie, but not sent to page propperly?

            //var result = await _signInManager.PasswordSignInAsync(accountDetails.Email, accountDetails.Password, accountDetails.RememberMe, lockoutOnFailure: false);
            return LocalRedirect(returnUrl);

            //return View("~/Views/Account/Login.cshtml");
        }
    }
}
