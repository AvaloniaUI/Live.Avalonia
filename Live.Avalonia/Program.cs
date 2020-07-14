using System;
using System.IO;
using System.Linq;

namespace Live.Avalonia
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var verboseLogging = args.Contains("verbose");
            var debug = args.Contains("debug") ? "../Live.Avalonia.Sample" : Directory.GetCurrentDirectory();
            new AvaloniaReloadingTool(debug, verboseLogging, Console.WriteLine).Run();
        }
    }
}
