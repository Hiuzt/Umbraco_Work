using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;
using Umbraco_Work.Core.ViewModel;

namespace Umbraco_Work.Core.Controllers
{
    public class AccountController : SurfaceController
    {
        private const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/Account/";

        public AccountController() { }

        public ActionResult RenderMyAccount()
        {
            var viewModel = new AccountViewModel();

            if (!Umbraco.MemberIsLoggedOn())
            {
                ModelState.AddModelError("Error", "Please login into your account");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByUsername(Membership.GetUser().UserName);

            if (member == null)
            {
                ModelState.AddModelError("Error", "We could not find you in the system");
                return CurrentUmbracoPage();
            }

            viewModel.Name = member.Name;
            viewModel.Email = member.Email;
            viewModel.Username = member.Username;

            return PartialView(PARTIAL_VIEW_FOLDER + "MyAccount.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleUpdateDetails(AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Please fill the form correctly");
                return CurrentUmbracoPage();
            }

            if (!Umbraco.MemberIsLoggedOn())
            {
                ModelState.AddModelError("Error", "You are not logged in!");
                return CurrentUmbracoPage();
            }

            if (Membership.GetUser() == null)
            {
                ModelState.AddModelError("Error", "You are not logged in!");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByUsername(Membership.GetUser().UserName);

            if (member == null)
            {
                ModelState.AddModelError("Error", "You are not logged in!");
                return CurrentUmbracoPage();
            }

            member.Name = viewModel.Name;
            member.Email = viewModel.Email;

            Services.MemberService.Save(member);

            TempData["status"] = "Updated Details";

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandlePasswordChange(AccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Fill the form correctly");
                return CurrentUmbracoPage();
            }

            if (!Umbraco.MemberIsLoggedOn())
            {
                ModelState.AddModelError("Error", "You are not logged in");
                return CurrentUmbracoPage();
            }

            var member = Services.MemberService.GetByEmail(Membership.GetUser().Email);

            if (member == null)
            {
                ModelState.AddModelError("Error", "User was not found in the systme");
                return CurrentUmbracoPage();
            }

            try
            {

                Services.MemberService.SavePassword(member, viewModel.Password);
            } catch (Exception ex)
            {
                ModelState.AddModelError("Error", "There was a problem: " + ex.Message);
                return CurrentUmbracoPage();
            }

            TempData["status"] = "Password Changed";

            return RedirectToCurrentUmbracoPage();
        }
    }
}
