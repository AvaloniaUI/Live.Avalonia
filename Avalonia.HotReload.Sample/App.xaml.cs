using System;
using Avalonia.Markup.Xaml;

namespace Avalonia.HotReload.Sample
{
    public class App : Application
    {
        private AvaloniaReloadingWindow _window;
        
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            // Obtain the assembly of the project by doing typeof on any type.
            // Then, instantiate the reloading Window class by passing the current
            // project assembly to it, as well as the logger. Then, show the window.
            // Using multiple reloading windows isn't currently supported.
            var assembly = typeof(App).Assembly;
            _window = new AvaloniaReloadingWindow(assembly, Console.WriteLine);
            _window.Show();
            base.OnFrameworkInitializationCompleted();
        }
    }
}
