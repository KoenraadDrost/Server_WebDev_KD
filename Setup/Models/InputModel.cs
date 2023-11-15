using System.ComponentModel.DataAnnotations;

namespace Setup.Models
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        private string _Username;
        private string _Email;
        private string _Password;
        private string _ConfirmPassword;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Username")]
        public string Username
        {
            get { return _Username; }
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null)
                {
                    MemberName = "Username",
                });
                _Username = value;
            }
        }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email
        {
            get => _Email;
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null)
                {
                    MemberName = "Email",
                });
                _Email = value;
            }
        }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get => _Password;
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null)
                {
                    MemberName = "Password",
                });
                _Password = value;
            }
        }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword
        {
            get => _ConfirmPassword;
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null)
                {
                    MemberName = "ConfirmPassword",
                });
                _ConfirmPassword = value;
            }
        }
    }
}
