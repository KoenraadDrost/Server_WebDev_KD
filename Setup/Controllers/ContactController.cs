using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using Setup.Models;
using System.Text.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Setup.Controllers
{
    public class ContactController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Submit")]
        public IActionResult Submit(ContactMail contactMail )
        {
            string jsonString = JsonSerializer.Serialize<ContactMail>(contactMail);
            HttpRequestMessage httpRequest = new HttpRequestMessage(new HttpMethod("POST"), "https://localhost:7095/api/Email/Send");
            httpRequest.Content = new StringContent(jsonString);

            client.SendAsync(httpRequest).Wait();

            return View("Index");
        }
    }
}
