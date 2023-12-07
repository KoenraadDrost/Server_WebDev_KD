using Microsoft.AspNetCore.Mvc;
using Setup.Models;
using System.Text.Json;

namespace Setup.Controllers
{
    public class RegisterPageController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        [ActionName("Submit")]
        public async Task<IActionResult> SubmitAsync(InputModel accountDetails)
        {
            string jsonString = JsonSerializer.Serialize<InputModel>(accountDetails);
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7095/api/Register");
            httpRequest.Content = new StringContent(jsonString);

            //client.SendAsync(httpRequest).Wait();
            var response =  await client.SendAsync(httpRequest);

            return View("~/Views/Account/Register.cshtml");
        }
    }
}
