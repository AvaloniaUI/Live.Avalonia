using Avalonia.Markup.Xaml;

namespace Avalonia.Reloading.Sample
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);
    }
}
