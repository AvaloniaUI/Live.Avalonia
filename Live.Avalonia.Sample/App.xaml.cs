using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Live.Avalonia.Library;

namespace Live.Avalonia.Sample
{
    public class App : Application, ILiveView
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            // Here, we create a new LiveViewHost, located in the 'Live.Avalonia'
            // namespace, and pass an ILiveView implementation to it. The ILiveView
            // implementation should have a parameterless constructor! Next, we
            // start listening for any changes in the source files. And then, we
            // show the LiveViewHost window. Simple enough, huh?
            var window = new LiveViewHost(this, Console.WriteLine);
            window.StartWatchingSourceFilesForHotReloading();
            window.Show();

            base.OnFrameworkInitializationCompleted();
        }

        // When any of the source files change, a new version of
        // the assembly is built, and this method gets called.
        // The returned content gets embedded into the window.
        public object CreateView(Window window)
        {
            // The AppView class will inherit the DataContext
            // of the window. The AppView class can be a 
            // UserControl, a Grid, a TextBlock, whatever.
            window.DataContext ??= new AppViewModel();
            return new AppView();
        }
    }
}
