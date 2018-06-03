
using EventStore.Rags;

namespace Ibento.DevelopmentHost
{
    public class Opts
    {
        public static string EnvironmentPrefix = "IBENTO_";
        public const string HostingGroup = "Hosting";
        public const string HttpPortDescription = "The Port to listen for Http Requests on";
        public const int HttpPortDefault = 5001;
    }
    public class HostingOptions
    {
        [ArgDescription(Opts.HttpPortDescription, Opts.HostingGroup)]
        public int HttpPort { get; set; }
        public HostingOptions()
        {
            HttpPort = Opts.HttpPortDefault;
        }
    }
}
