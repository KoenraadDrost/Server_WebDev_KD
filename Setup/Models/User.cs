using Microsoft.AspNetCore.Identity;

namespace Setup.Models
{
    // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-7.0&tabs=visual-studio#create-a-web-app-with-authentication
    // Setup DB and make sure it is included in Github. [CHECK]
    // Create this user model as a Subclass of default Authentication.Account. [CHECK]
    // Add link to "Player" for the game and other User specific data not standard in default Account class. [Pending]
    public class User : IdentityUser
    {

    }
}
