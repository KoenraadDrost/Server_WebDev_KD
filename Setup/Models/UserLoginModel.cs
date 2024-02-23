using System.ComponentModel.DataAnnotations;

namespace Setup.Models
{
    public class UserLoginModel
    {
        public string Verification { get; set; }

        private string _Username;
        private string _Email;
        private string _Password;
        private bool _RememberMe;

        [Required]
        [StringLength(100, ErrorMessage = "Username doesn't have the required length, please try a different username.", MinimumLength = 2)]
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

        //[Required]
        //[EmailAddress]
        //[Display(Name = "Email")]
        //public string Email
        //{
        //    get => _Email;
        //    set
        //    {
        //        Validator.ValidateProperty(value, new ValidationContext(this, null, null)
        //        {
        //            MemberName = "Email",
        //        });
        //        _Email = value;
        //    }
        //}

        [Required]
        [StringLength(100, ErrorMessage = "Password not strong enough to be secure, please try a different password.", MinimumLength = 8)]
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

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
