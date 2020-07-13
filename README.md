# Avalonia.HotReload

This repository demonstrates how to use the hot reload feature in your Avalonia applications. The core idea is to rely on `dotnet watch` for rebuilding the projects from sources when any of the source files change, and to re-embed the updated controls into the `Window` without any need to press the 'Run' button by hands multiple times.

### Getting Started

1. Create a new project based on a template from the [Avalonia Templates repository](https://github.com/AvaloniaUI/avalonia-dotnet-templates). Or, use [AvaloniaVS](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio).
2. Install the [`Avalonia.ReactiveUI`](https://www.nuget.org/packages/Avalonia.ReactiveUI/) package into your newly created project:
```sh
# Execute this command from the project root.
dotnet add package Avalonia.ReactiveUI
```
3. Copy-paste the [`AvaloniaReloadingWindow.cs`](./Avalonia.HotReload/AvaloniaReloadingWindow.cs) file into your newly created project.
4. Create a static method [`CreateReloadableControl`](https://github.com/worldbeater/Avalonia.HotReload/blob/master/Avalonia.HotReload.Sample/Program.cs#L24) in your the `Program.cs` file:
```cs
// This method will be the hot-reloadable composition root of your Avalonia application.
// Remember to use this signature! Otherwise the things won't work.
public static object CreateReloadableControl(Window window) => new TextBlock { Text = "Ok" };
```
5. Instantiate the [`AvaloniaReloadingWindow`](./Avalonia.HotReload/AvaloniaReloadingWindow.cs) in your [`App.xaml.cs`](https://github.com/worldbeater/Avalonia.HotReload/blob/master/Avalonia.HotReload.Sample/App.xaml.cs#L12) file as such:
```cs
// Obtain the assembly of the project by doing typeof on any type from the assembly.
// Then, instantiate the reloading Window class by passing the current project
// assembly to it, as well as the logger. Then, show the window. Using multiple 
// reloading windows isn't currently supported.
var assembly = typeof(App).Assembly;
var window = new AvaloniaReloadingWindow(assembly, Console.WriteLine);
window.Show();
```
6. Run your project using .NET CLI, as follows:
```sh
# Don't use the 'dotnet run' without the '--no-build' argument.
dotnet run --no-build
```

> **Important Note**: Don't use the `dotnet run` command without the `--no-build` argument! Always use `dotnet run --no-build`, or `dotnet build && dotnet run --no-build`. Otherwise the executable file will get locked, see: https://github.com/dotnet/sdk/issues/11766

7. Done! Make some changes in the control returned by `CreateReloadableControl`, press `Ctrl+S` and the app will hot-reload. If you experience any issues with this setup, try cloning this repository and running the `Avalonia.HotReload.Sample` project by executing `dotnet run --no-build` from the project root.

> **Important Note**: By default, `dotnet watch` tracks changes in `.cs` files only. In order to have hot-reload working with `.xaml` files, add the `<Watch Include="**\*.xaml" />` directive to your `.csproj` file. See the [project file](https://github.com/worldbeater/Avalonia.HotReload/blob/master/Avalonia.HotReload.Sample/Avalonia.HotReload.Sample.csproj) in the demo project for more context.

<img src="./demo.gif" width="800" />
