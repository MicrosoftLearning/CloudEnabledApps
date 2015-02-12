using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Expenses.Web.Startup))]

namespace Expenses.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
