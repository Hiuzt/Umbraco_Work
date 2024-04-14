using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco_Work.Core.ViewModel;

namespace Umbraco_Work.Core.Interfaces
{
    public interface IEmailService
    {
        void SendContactNotificationToAdmin(ContactFormViewModel viewModel);
        void SendVerifyEmailAddressNotification(string email, string token);

        void SendResetPasswordNotification(string email, string token);

        void SendPasswordChangedNotification(string email);

    }
}
