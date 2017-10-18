using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PraksaProjekat.Startup))]
namespace PraksaProjekat
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
