using Serilog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace Ibento.DevelopmentHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = Options.Parse<HostingOptions>(args, Opts.EnvironmentPrefix);
            var host = CreateWebHost(args, options);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("\n{0,-25} {1} ({2}/{3}, {4})", "IBENTO VERSION:", VersionInfo.Version, VersionInfo.Branch, VersionInfo.Hashtag, VersionInfo.Timestamp);
            Log.Logger.Information(Options.DumpOptions());

            await host.RunAsync();
        }

        private static IWebHost CreateWebHost(string[] args, HostingOptions hostingOptions) =>
            WebHost.CreateDefaultBuilder(args)
            .UseSerilog()
            .UseKestrel(options =>
            {
                options.ListenAnyIP(hostingOptions.HttpPort);
            })
            .UseStartup<Startup>()
            .Build();
    }
}
