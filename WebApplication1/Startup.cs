using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(gTravel.Startup))]
namespace gTravel
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
