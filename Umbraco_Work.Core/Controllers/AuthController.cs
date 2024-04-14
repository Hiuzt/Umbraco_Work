using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Core.Logging;
using Umbraco.Web.Controllers;
using Umbraco.Web.Mvc;
using Umbraco_Work.Core.Interfaces;
using Umbraco_Work.Core.ViewModel;

namespace Umbraco_Work.Core.Controllers
{
    public class AuthController : SurfaceController
    {
        private const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/";
        private IEmailService _emailService;

        public AuthController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public ActionResult RenderRegister()
        {
            var viewModel = new RegisterViewModel();

            return PartialView(PARTIAL_VIEW_FOLDER + "Register.cshtml", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleRegister(RegisterViewModel viewModel) {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var existingMember = Services.MemberService.GetByEmail(viewModel.Email);
            if (existingMember != null)
            {
                ModelState.AddModelError("Account Error", "There is already a user with this email address");
                return CurrentUmbracoPage();
            }

            existingMember = Services.MemberService.GetByUsername(viewModel.UserName);
            if (existingMember != null)
            {
                ModelState.AddModelError("Account Error", "There is already a user with this username");
                return CurrentUmbracoPage();
            }

            var newMember = Services.MemberService.CreateMember(viewModel.UserName, viewModel.Email, $"{viewModel.FirstName} {viewModel.LastName}", "Member");
            newMember.PasswordQuestion = "";
            newMember.RawPasswordAnswerValue = "";
            Services.MemberService.Save(newMember);
            Services.MemberService.SavePassword(newMember, viewModel.Password);

            Services.MemberService.AssignRole(newMember.Id, "Normal Users");

            var token = Guid.NewGuid().ToString();
            newMember.SetValue("emailVerifyToken", token);
            Services.MemberService.Save(newMember);

            _emailService.SendVerifyEmailAddressNotification(newMember.Email, token);

            TempData["status"] = "OK";

            return CurrentUmbracoPage();
        }

        public ActionResult RenderEmailVerification(string token)
        {
            var member = Services.MemberService.GetMembersByPropertyValue("emailVerifyToken", token).SingleOrDefault();

            if (member != null)
            {
                var alreadyVerified = member.GetValue<bool>("emailVerified");
                if (alreadyVerified)
                {
                    ModelState.AddModelError("Verified", "You have already verified your email");
                    return PartialView(PARTIAL_VIEW_FOLDER + "EmailVerification.cshtml");
                }

                member.SetValue("emailVerified", true);
                member.SetValue("emailVerifiedDate", DateTime.Now);
                Services.MemberService.Save(member);

                TempData["status"] = "OK";

                return PartialView(PARTIAL_VIEW_FOLDER + "EmailVerification.cshtml");
            }

            ModelState.AddModelError("Error", "Apologies there has been some problem!");

            return PartialView(PARTIAL_VIEW_FOLDER + "EmailVerification.cshtml");
        }

        #region login
        public ActionResult RenderLogin()
        {

            var viewModel = new LoginViewModel();
            viewModel.RedirectUrl = HttpContext.Request.Url.AbsolutePath;

            return PartialView(PARTIAL_VIEW_FOLDER + "/Login/Login.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleLogin(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByUsername(viewModel.Username);

            if (member == null)
            {
                ModelState.AddModelError("Login", "Cannot find user with this username");
                return CurrentUmbracoPage();
            }

            if (member.IsLockedOut)
            {
                ModelState.AddModelError("Login", "You are locked out from the website");
                return CurrentUmbracoPage();
            }

            var emailVerified = member.GetValue<bool>("EmailVerified");
            if (!emailVerified)
            {
                ModelState.AddModelError("Login", "Your email is not verified");
            }

            if (!Members.Login(viewModel.Username, viewModel.Password))
            {
                ModelState.AddModelError("Login", "The username or the password is not right");
                return CurrentUmbracoPage();
            }

            if (!string.IsNullOrEmpty(viewModel.RedirectUrl))
            {
                return Redirect(viewModel.RedirectUrl);
            }

            return RedirectToCurrentUmbracoPage();

        }

        #endregion
        #region forgot password
        public ActionResult RenderForgottenPassword()
        {
            var viewModel = new ForgottenPasswordViewModel();

            return PartialView(PARTIAL_VIEW_FOLDER + "ForgottenPassword.cshtml", viewModel);
        }

        public ActionResult HandleForgottenPassword(ForgottenPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Password", "Please fill the form correctly");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByEmail(viewModel.Email);

            if (member == null)
            {
                ModelState.AddModelError("Password", "Sorry there is no such email address");
                return CurrentUmbracoPage();
            }


            var resetToken = Guid.NewGuid().ToString();
            var expiryDate = DateTime.Now.AddHours(12);

            member.SetValue("resetExpiryDate", expiryDate);
            member.SetValue("resetLinkToken", resetToken);
            Services.MemberService.Save(member);

            _emailService.SendResetPasswordNotification(viewModel.Email, resetToken);
         
            Logger.Info<AuthController>($"Sent a password reset to {viewModel.Email}");

            TempData["status"] = "OK";

            return CurrentUmbracoPage();
        }

        #endregion

        #region reset password
        public ActionResult RenderResetPassword()
        {
            var viewModel = new ResetPasswordViewModel();
            return PartialView(PARTIAL_VIEW_FOLDER + "ResetPassword.cshtml", viewModel);
        }

        public ActionResult HandleResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Your form is not valid!");
                return CurrentUmbracoPage();
            }

            var token = Request.QueryString["token"];
            if (string.IsNullOrEmpty(token))
            {
                Logger.Warn<AuthController>("Reset Password - no token found");
                ModelState.AddModelError("Error", "Token not found");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetMembersByPropertyValue("resetLinkToken", token).SingleOrDefault();

            if (member == null)
            {
                ModelState.AddModelError("Error", "That link is no longer valid");
                return CurrentUmbracoPage();
            }

            var membersTokenExpiryDate = member.GetValue<DateTime>("resetExpiryDate");
            if (DateTime.Now.CompareTo(membersTokenExpiryDate) >= 0)
            {
                ModelState.AddModelError("Error", "Sorry the link has expiry");
                return CurrentUmbracoPage();
            }

            Services.MemberService.SavePassword(member, viewModel.Password);
            member.SetValue("resetLinkToken", string.Empty);
            member.SetValue("resetExpiryDate", null);
            member.IsLockedOut = false;

            Services.MemberService.Save(member);

            _emailService.SendPasswordChangedNotification(member.Email);

            TempData["status"] = "OK";

            return RedirectToCurrentUmbracoPage();
 
        }
        #endregion

    }
}
