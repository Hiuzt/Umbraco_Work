using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco_Work.Core.Interfaces;
using Umbraco_Work.Core.Services;

namespace Umbraco_Work.Core
{
    public class RegisterServicesComposer : IUserComposer
    {
        void IComposer.Compose(Composition composition)
        {
            composition.Register<IEmailService, EmailService>(Lifetime.Request);
        }
    }
}
