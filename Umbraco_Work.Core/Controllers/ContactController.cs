using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco_Work.Core.Interfaces;
using Umbraco_Work.Core.ViewModel;

namespace Umbraco_Work.Core.Controllers
{
    public class ContactController : SurfaceController
    {
        private IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public ActionResult RenderContactForm()
        {
            var viewModel = new ContactFormViewModel();

            return PartialView("~/Views/Partials/Contact Form.cshtml");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult handleContactForm(ContactFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Please check the form");

                return CurrentUmbracoPage();
            }

            try
            {
                var contactForms = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("contactMessages").FirstOrDefault();

                if (contactForms != null)
                {
                    var parentId = contactForms.Id;
                    var newContact = Services.ContentService.Create("Contact", parentId, "contactMessage");

                    newContact.SetValue("contactName", viewModel.Name);
                    newContact.SetValue("contactEmail", viewModel.Email);
                    newContact.SetValue("contactSubject", viewModel.Subject);
                    newContact.SetValue("contactComment", viewModel.Comment);

                    Services.ContentService.SaveAndPublish(newContact);
                }

                // Email sending
                _emailService.SendContactNotificationToAdmin(viewModel);

                TempData["status"] = "OK";


                return CurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                Logger.Error<ContactController>("There was an error in the contact for submission", ex.Message);
                ModelState.AddModelError("Error", "Sorry there was a problem nothing your details. Would you try again later?");
            }

            return CurrentUmbracoPage();
        }


    }
}
