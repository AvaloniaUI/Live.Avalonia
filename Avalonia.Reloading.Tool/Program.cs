using System;
using System.IO;
using System.Linq;

namespace Avalonia.Reloading.Tool
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            new AvaloniaReloadingTool(
                    args.Contains("debug")
                        ? "../Avalonia.Reloading.Sample" 
                        : Directory.GetCurrentDirectory(), 
                    args.Contains("verbose"), 
                    Console.WriteLine)
                .Run();
        }
    }
}
