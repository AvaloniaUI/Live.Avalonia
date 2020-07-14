using System;
using System.IO;
using System.Linq;

namespace Avalonia.Reloading.Tool
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var project = args.Contains("debug") ? "../Avalonia.Reloading.Sample" : Directory.GetCurrentDirectory();
            new AvaloniaReloadingTool(project, Console.WriteLine).Run();
        }
    }
}
