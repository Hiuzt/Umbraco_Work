using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco_Work.Core.ViewModel
{
    public class RegisterViewModel
    {

        [DisplayName("First Name")]
        [Required(ErrorMessage = "Please enter your first name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Required(ErrorMessage = "Please enter your last name")]
        public string LastName { get; set; }

        [DisplayName("Username")]
        [Required(ErrorMessage = "Please choose a username")]
        [MinLength(6)]
        public string UserName { get; set; }

        [DisplayName("Email")]
        [Required(ErrorMessage = "Please enter your email")]
        public string Email { get; set; }

        [UIHint("Password")]
        [DisplayName("Password")]
        [Required(ErrorMessage = "Please enter your password")]
        [MinLength(10, ErrorMessage = "Please make your password at least 8 characters")]
        public string Password { get; set; }

        [UIHint("Confirm Password")]
        [DisplayName("Confirm Password")]
        [Required(ErrorMessage = "Please enter your password confirmation")]
        [EqualTo("Password", ErrorMessage = "Please ensure that your password match")]
        public string ConfirmPassword { get; set; }
    }
}
