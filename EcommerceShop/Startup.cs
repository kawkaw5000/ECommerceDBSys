using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EcommerceShop.Startup))]
namespace EcommerceShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
