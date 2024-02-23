using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
using Setup.Models;
using System.Text.Json;

namespace Setup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutAPIController : ControllerBase
    {
        private readonly SignInManager<ChatUser> _signInManager;
        private readonly ILogger<LoginAPIController> _logger;

        public LogoutAPIController(SignInManager<ChatUser> signInManager, ILogger<LoginAPIController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        public async Task<ObjectResult> OnPostAsync()
        {
            string jsonResponse;

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            jsonResponse = JsonSerializer.Serialize("You've succefulyl logged out.");
            return Ok(jsonResponse);
        }
    }
}
