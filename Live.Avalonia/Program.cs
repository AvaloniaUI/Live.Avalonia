using System;
using System.IO;
using System.Linq;

namespace Live.Avalonia
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            new AvaloniaReloadingTool(
                    args.Contains("debug")
                        ? "../Live.Avalonia.Sample" 
                        : Directory.GetCurrentDirectory(), 
                    args.Contains("verbose"), 
                    Console.WriteLine)
                .Run();
        }
    }
}
