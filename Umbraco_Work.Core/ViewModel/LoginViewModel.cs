using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco_Work.Core.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }
        
        [Required] 
        public string Password { get; set; }

        public string RedirectUrl { get; set; } 
    }
}
