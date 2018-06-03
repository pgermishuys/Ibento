using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ibento.DevelopmentHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection collection)
        {
            collection.AddLogging();
            collection.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }
    }
}
