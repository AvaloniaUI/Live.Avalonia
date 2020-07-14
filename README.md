# Live.Avalonia

`Live.Avalonia` is an experimental project which intends to have the hot reloading feature working in Avalonia-based multiplatform projects. The core idea of this project was originally proposed by [@Pix2d](https://twitter.com/pix2d) during a discussion in Avalonia Telegram chat. In `Live.Avalonia`, we rely on `dotnet watch build` .NET Core facility to rebuild an Avalonia project from sources when any of the source files change. Then, we re-embed the updated controls into a simple Avalonia `Window`. 

`Live.Avalonia` could possibly save you a lot of time spent clicking 'Build & Run' in your IDE, or typing `dotnet run` in the console. Worth noting, that `Live.Avalonia` doesn't require you to install any particular IDE tooling™ — you can edit files even in [Vim](https://github.com/vim/vim), and the app will hot reload 🔥

<img src="./Live.Avalonia.gif" width="600" />

### Getting Started

> **Important Note** `Live.Avalonia` setup was tested only on my Ubuntu 18.04 LTS machine on a very little amount of Avalonia projects, and is **not guaranteed** to work elsewhere. However, you could use this tool, if you have free time to experiment and stumble upon some dirty stuff, or if you are willing to help out with the development of `Live.Avalonia` tooling. Thank you for your flexibility.

[`Live.Avalonia`](https://www.nuget.org/packages/Live.Avalonia/0.1.0-alpha) is distributed via NuGet package manager:
```
dotnet add package Live.Avalonia
```
After installing the NuGet package, add the following lines to your `App.xaml.cs` file:
```cs
public class App : Application, ILiveView
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    // This curious method you are seeing here is due to Application Lifetimes. 
    // See the following Avalonia documentation page for more info:
    // https://github.com/AvaloniaUI/Avalonia/wiki/Application-lifetimes
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
    
    // This is a very special method. When any of the source files change,
    // a new assembly is built, and this method is called. The returned
    // content gets embedded into the LiveViewHost window.
    public object CreateView(Window window) => new TextBlock { Text = "Hi!" };
}
```
Then, run your Avalonia application:
```
dotnet run
```
Now, edit the control returned by `ILiveView.CreateView`, and the app will hot reload! 🔥

> **Important Note** By default, `dotnet watch build` triggers the build only when `.cs` files change. In order to have live reload working for `.xaml` files too, add the following line to your `.csproj` file: `<Watch Include="**\*.xaml" />`. See `Live.Avalonia.Sample` project for more info.
