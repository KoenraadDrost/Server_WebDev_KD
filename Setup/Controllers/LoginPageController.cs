using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System.Text.Json;

namespace Setup.Controllers
{
    public class LoginPageController : Controller
    {
        private static readonly HttpClient client = new HttpClient();
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
        public async Task<IActionResult> SubmitAsync(UserLoginModel accountDetails)
        {
            string jsonString = JsonSerializer.Serialize<UserLoginModel>(accountDetails);
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7095/api/Login");
            httpRequest.Content = new StringContent(jsonString);

            var response = await client.SendAsync(httpRequest);

            return View("~/Views/Account/Login.cshtml");
        }
    }
}
