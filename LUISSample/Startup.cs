using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LUISSample.Startup))]
namespace LUISSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
