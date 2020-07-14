using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using JetBrains.Annotations;

namespace Avalonia.Reloading.Sample
{
    public static class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        [UsedImplicitly]
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();

        [UsedImplicitly]
        public static object CreateReloadableControl(Window window) =>
            new Grid
            {
                Children =
                {
                    new Border
                    {
                        Width = 500,
                        Height = 300,
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