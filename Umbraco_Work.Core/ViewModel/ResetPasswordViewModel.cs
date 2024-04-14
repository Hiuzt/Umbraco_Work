using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco_Work.Core.ViewModel
{
    public class ResetPasswordViewModel
    {
        [UIHint("Password")]
        [Required(ErrorMessage = "Please enter a new password")]
        public string Password { get; set; }
        
        [UIHint("Password")]
        [Required(ErrorMessage = "Please enter a new password")]
        [EqualTo("Password", ErrorMessage = "2 password has to match")]
        public string ConfirmPassword { get; set; }
    }
}
