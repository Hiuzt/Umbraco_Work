using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using Umbraco_Work.Core.Interfaces;
using Umbraco_Work.Core.ViewModel;

namespace Umbraco_Work.Core.Services
{
    public class EmailService : IEmailService
    {
        private UmbracoHelper _umbraco;
        private ContentService _contentService;
        private ILogger _logger;

        public EmailService(UmbracoHelper umbraco, ContentService contentService, ILogger Logger)
        {
            _umbraco = umbraco;
            _contentService = contentService;
            _logger = Logger;
        }



        public void SendContactNotificationToAdmin(ContactFormViewModel viewModel)
        {

            var emailTemplate = GetEmailTemplate("New Contact Form Notification");

            if (emailTemplate == null) {
                throw new Exception("Template not found");
            }

            var emailSubject = emailTemplate.Value<string>("emailTemplateSubject");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            MailMerge("name", viewModel.Name, ref htmlContent, ref textContent);
            MailMerge("email", viewModel.Email, ref htmlContent, ref textContent);
            MailMerge("comment", viewModel.Comment, ref htmlContent, ref textContent);


            // Site settings
            var siteSettings = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();

            if (siteSettings == null)
            {
                throw new Exception("There are no site settings");
            }

            var toAddresses = siteSettings.Value<string>("emailSettingsAdminAccounts");
            if (string.IsNullOrEmpty(toAddresses))
            {
                throw new Exception("There needs to be a to address in site settings");
            }

            SendMail(toAddresses, emailSubject, htmlContent, textContent);

        }

        private void SendMail(string toAddresses, string subject, string htmlContent, string textContent)
        {
            var siteSettings = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings == null)
            {
                throw new Exception("There are no site settings");
            }

            var fromAddress = siteSettings.Value<string>("emailSettingsFromAddress");
            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new Exception("There needs to be a from address in site settings");
            }

            var recipients = toAddresses;

            var emails = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("emails").FirstOrDefault();
            var newEmail = _contentService.Create(toAddresses, emails.Id, "Email");
            newEmail.SetValue("emailSubject", subject);
            newEmail.SetValue("emailToAddress", recipients);
            newEmail.SetValue("emailHtmlContent", htmlContent);
            newEmail.SetValue("emailTextContent", textContent);
            newEmail.SetValue("emailSent", false);
            _contentService.SaveAndPublish(newEmail);

            var mimeType = new System.Net.Mime.ContentType("text/html");
            var alternateView = AlternateView.CreateAlternateViewFromString(htmlContent, mimeType);

            var smtpMessage = new MailMessage();
            smtpMessage.AlternateViews.Add(alternateView);

            foreach (var recipient in recipients.Split(','))
            {
                if (!string.IsNullOrEmpty(recipient))
                {
                    smtpMessage.To.Add(recipient);
                }
            }

            smtpMessage.From = new MailAddress(fromAddress);
            smtpMessage.Subject = subject;
            smtpMessage.Body = textContent;

            using (var smtp = new SmtpClient())
            {
                try
                {
                    smtp.Send(smtpMessage);
                    newEmail.SetValue("emailSent", true);
                    newEmail.SetValue("emailSentDate", DateTime.Now);
                    _contentService.SaveAndPublish(newEmail);
                }
                catch (Exception exc)
                {
                    _logger.Error<EmailService>("Problem sending the email", exc);
                    throw exc;
                }
            }
        }

        public void SendVerifyEmailAddressNotification(string targetEmail, string emailToken)
        {
            var emailTemplate = GetEmailTemplate("Email Verification Template");

            if (emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var emailSubject = emailTemplate.Value<string>("emailTemplateSubject");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            var url = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.AbsolutePath, string.Empty);
            url += $"/verify?token={emailToken}";

            MailMerge("verify-url", url, ref htmlContent, ref textContent);

            SendMail(targetEmail, emailSubject, htmlContent, textContent);

            
        }

        public void SendResetPasswordNotification(string targetEmail, string emailToken)
        {
            var emailTemplate = GetEmailTemplate("Reset Password Link");

            if (emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var emailSubject = emailTemplate.Value<string>("emailTemplateSubject");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            var url = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.AbsolutePath, string.Empty);
            url += $"/reset-password?token={emailToken}";

            MailMerge("reset-url", url, ref htmlContent, ref textContent);

            SendMail(targetEmail, emailSubject, htmlContent, textContent);
        }

        public void SendPasswordChangedNotification(string targetEmail)
        {
            var emailTemplate = GetEmailTemplate("Reset Password Link");

            if (emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var emailSubject = emailTemplate.Value<string>("emailTemplateSubject");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            SendMail(targetEmail, emailSubject, htmlContent, textContent);
        }

        private void MailMerge(string token, string value, ref string htmlContent, ref string textContent)
        {
            htmlContent = htmlContent.Replace($"##{token}##", value);
            textContent = textContent.Replace($"##{token}##", value);
        }


        private IPublishedContent GetEmailTemplate(string templateName)
        {
            var template = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("emailTemplate").Where(x => x.Name == templateName).FirstOrDefault();

            return template;
        }
    }
}
