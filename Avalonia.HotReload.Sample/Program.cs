using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using JetBrains.Annotations;

namespace Avalonia.HotReload.Sample
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
        
        /// This method is called by the AvaloniaReloadingWindow. 
        /// It may return any type inherent from 'object', and accept
        /// a 'Window' as the first and only argument.  
        [UsedImplicitly]
        public static IVisual CreateReloadableControl(Window window) =>
            new Grid
            {
                Children =
                {
                    new Border
                    {
                        MaxWidth = 300,
                        MaxHeight = 300,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Classes = Classes.Parse("Card"),
                        Child = new StackPanel
                        {
                            Classes = Classes.Parse("Card"),
                            Children = 
                            { 
                                new TextBlock
                                {
                                    Margin = new Thickness(10, 10, 10, 10),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Text = "This is Avalonia hot reloading content!"
                                },
                                new Button
                                {
                                    Content = "I'm a button!"
                                }
                            }
                        }
                    }
                }
            };
    }
}