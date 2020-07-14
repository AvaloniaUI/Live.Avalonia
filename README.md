# Live Avalonia

`Live.Avalonia` is an experimental project which intends to have the hot reloading feature working in Avalonia-based multiplatform projects. The core idea of this project was originally proposed by [@Pix2d](https://twitter.com/pix2d) during a discussion in Avalonia Telegram chat. In `Live.Avalonia`, we rely on `dotnet watch build` .NET Core facility for rebuilding an Avalonia project from sources when any of the source files change, and to re-embed the updated controls into a simple `Window`. 

`Live.Avalonia` could possibly save you a lot of time spent clicking 'Build & Run' in your IDE, or typing `dotnet run` in console. Worth noting, that `Live.Avalonia` doesn't require you to install any particular IDE tooling™ — you can edit files even in [Vim](https://github.com/vim/vim), and the app will hot reload 🔥

<img src="./Live.Avalonia.gif" width="600" />

### Getting Started

> **A Very Important Note** `Live.Avalonia` setup was tested only on my Ubuntu 18.04 LTS machine on a very little amount of Avalonia projects, and is **not guaranteed** to work elsewhere. However, you could use this tool, if you have free time to experiment and stumble upon some dirty stuff, or if you are willing to help out with the development of `Live.Avalonia` tooling. Thank you for your flexibility.

[`Live.Avalonia`](https://www.nuget.org/packages/Live.Avalonia/0.1.0-alpha) is distributed via NuGet package manager:
```
dotnet add package Live.Avalonia
```
After installing the NuGet package, add the following lines to your `App.xaml.cs` file:
```cs
public class App : Application, ILiveView
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var window = new LiveViewHost(this, Console.WriteLine);
        window.StartWatchingSourceFilesForHotReloading();
        window.Show();

        base.OnFrameworkInitializationCompleted();
    }

    public object CreateView(Window window) => new TextBlock { Text = "Hi!" };
}
```
Then, run your Avalonia application once;
```
dotnet run
```
Make any changes in the control returned by the `ILiveView.CreateView` method, and the app will reload immediately!
