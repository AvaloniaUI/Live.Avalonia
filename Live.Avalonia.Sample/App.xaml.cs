using Avalonia;
using Avalonia.Markup.Xaml;

namespace Live.Avalonia.Sample
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);
    }
}
