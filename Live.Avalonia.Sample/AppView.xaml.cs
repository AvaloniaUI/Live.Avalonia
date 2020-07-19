using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Live.Avalonia.Sample.Library;
using ReactiveUI;

namespace Live.Avalonia.Sample
{
    public sealed class AppView : ReactiveUserControl<AppViewModel>
    {
        public AppView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables => { });
        }
    }
}