using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace Live.Avalonia.Sample
{
    public static class Program
    {
        public static void Main(string[] args) => 
            AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug()
                .StartWithClassicDesktopLifetime(args);
    }
}
