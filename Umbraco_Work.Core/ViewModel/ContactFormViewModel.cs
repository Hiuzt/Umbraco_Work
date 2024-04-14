using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco_Work.Core.ViewModel
{
    public class ContactFormViewModel
    {
        [Required] public string Name { get; set; }
        [Required] [EmailAddress] public string Email { get; set; }
        [Required] public string Comment { get; set; }
        [Required] public string Subject { get; set; }
    }
}
