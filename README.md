[![nuget](https://img.shields.io/nuget/v/Live.Avalonia.svg)](https://www.nuget.org/packages/Live.Avalonia) [![downloads](https://img.shields.io/nuget/dt/live.avalonia)](https://www.nuget.org/packages/live.Avalonia) ![Size](https://img.shields.io/github/repo-size/worldbeater/live.avalonia.svg) ![License](https://img.shields.io/github/license/worldbeater/live.avalonia.svg) 

# Live.Avalonia

`Live.Avalonia` is an experimental project which intends to make the hot reloading feature working in Avalonia-based applications.

<img src="./Live.Avalonia.gif" width="500" />

In `Live.Avalonia`, we rely on `dotnet watch build` .NET Core facility to rebuild an Avalonia project from sources when any of the source files change. Then, we re-embed the updated controls into a simple Avalonia `Window`. `Live.Avalonia` could possibly save you a lot of time spent clicking 'Build & Run' in your IDE, or typing `dotnet run` in the console. Worth noting, that `Live.Avalonia` doesn't require you to install any particular IDE tooling™ — you can edit files even in [Vim](https://github.com/vim/vim), and the app will hot reload 🔥

### Getting Started

> **Warning** `Live.Avalonia` was not extensively tested, and is not guaranteed to work with every project setup, especially if you do some extraordinary stuff with weird MSBuild properties and your output assemblies. Use this tool at your own risk. Thank you for your flexibility.

> **Important Note** By default, `dotnet watch build` triggers the build only when any `.cs` file changes. In order to have live reload working for `.xaml` files too, add the following line to your `.csproj` file: `<Watch Include="**\*.xaml" />`. See the [`Live.Avalonia.Sample`](https://github.com/worldbeater/Live.Avalonia/blob/master/Live.Avalonia.Sample/Live.Avalonia.Sample.csproj#L16) project for more info.

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
        if (Debugger.IsAttached || IsProduction())
        {
            // Debugging requires pdb loading etc, so we disable live reloading
            // during a test run with an attached debugger.
            var window = new Window();
            window.Content = CreateView(window);
            window.Show();
        }
        else
        {
            // Here, we create a new LiveViewHost, located in the 'Live.Avalonia'
            // namespace, and pass an ILiveView implementation to it. The ILiveView
            // implementation should have a parameterless constructor! Next, we
            // start listening for any changes in the source files. And then, we
            // show the LiveViewHost window. Simple enough, huh?
            var window = new LiveViewHost(this, Console.WriteLine);
            window.StartWatchingSourceFilesForHotReloading();
            window.Show();
        }

        // Here we subscribe to ReactiveUI default exception handler to avoid app
        // termination in case if we do something wrong in our view models. See:
        // https://www.reactiveui.net/docs/handbook/default-exception-handler/
        //
        // In case if you are using another MV* framework, please refer to its 
        // documentation explaining global exception handling.
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(Console.WriteLine);
        base.OnFrameworkInitializationCompleted();
    }

    // When any of the source files change, a new version of the assembly is 
    // built, and this method gets called. The returned content gets embedded 
    // into the LiveViewHost window.
    public object CreateView(Window window) => new TextBlock { Text = "Hi!" };

    private static bool IsProduction()
    {
#if DEBUG
        return false;
#else
        return true;
#endif
    }
}
```
Then, run your Avalonia application:
```
dotnet run
```
Now, edit the control returned by `ILiveView.CreateView`, and the app will hot reload! 🔥

> **Pro tip** If you are willing to use an assembly weaving tool like [ReactiveUI.Fody](https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code) for `INotifyPropertyChanged` injections, extract your view models into a separate assembly. For example, the [`Live.Avalonia.Sample`](https://github.com/worldbeater/Live.Avalonia/blob/master/Live.Avalonia.Sample/Live.Avalonia.Sample.csproj#L16) project references the [`Live.Avalonia.Sample.Library`](https://github.com/worldbeater/Live.Avalonia/tree/main/Live.Avalonia.Sample.Library) project in order to have assembly postprocessing working as expected. Beware: if you change the code in the referenced assemblies, the app won't hot reload, and you will have to restart the app on your own to see changes.

### Retaining App State

As we discovered in [this Twitter thread](https://twitter.com/MihaMarkic/status/1283345704405082112), the state is retained, if you keep it in your [ViewModel](https://www.reactiveui.net/docs/handbook/view-models/) and pass it from `Window` to your `View` inside the `ILiveView.CreateView` method. So, if you are willing to keep app state the same after a hot reload, use the following `ILiveView.CreateView` implementation: 

```cs
public object CreateView(Window window) {
    if (window.DataContext == null)
        window.DataContext = new AppViewModel();

    // The AppView class will inherit the DataContext
    // of the window. The AppView class can be a 
    // UserControl, a Grid, a TextBlock, whatever.
    return new AppView();
}
```

### Getting Started with F#

Thanks to [@AngelMunoz](https://github.com/angelmunoz) and to [@JaggerJo](https://github.com/jaggerjo) `Live.Avalonia` now supports MVU and [`Avalonia.FuncUI`](https://github.com/AvaloniaCommunity/Avalonia.FuncUI) as well. See the [`Live.Avalonia.FuncUI.Sample`](https://github.com/worldbeater/Live.Avalonia/tree/main/Live.Avalonia.FuncUI.Sample) directory in this repository for a compelling example. The composition root is located inside the [Program.fs](https://github.com/worldbeater/Live.Avalonia/blob/main/Live.Avalonia.FuncUI.Sample/Program.fs#L35) file.

> **Important Note** By default, `dotnet watch build` triggers the build only when any `.cs` file changes. In order to have live reload working for `.fs` files too, add the following line to your `.fsproj` file: `<Watch Include="**\*.fs" />`. See the [`Live.Avalonia.FuncUI.Sample`](https://github.com/worldbeater/Live.Avalonia/blob/main/Live.Avalonia.FuncUI.Sample/Live.Avalonia.FuncUI.Sample.fsproj#L15) project for more info.
