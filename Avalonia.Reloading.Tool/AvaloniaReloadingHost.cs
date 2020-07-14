using System;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Avalonia.Reloading.Tool
{
    public sealed class AvaloniaReloadingHost : IDisposable
    {
        private const string CreateReloadableControlMethod = "CreateReloadableControl";
        private const string BuildAvaloniaAppMethod = "BuildAvaloniaApp";
        private readonly IDisposable _timerSubscription;
        private readonly string _programClassName;
        private readonly Action<string> _logger;
        private readonly string _assemblyPath;
        private string _latestVersion;
        private Window _window;

        public AvaloniaReloadingHost(string assemblyPath, Action<string> logger)
        {
            _logger = logger;
            _assemblyPath = assemblyPath;
            _programClassName = ExtractFullProgramClassName(assemblyPath);
            
            _logger("Registering observable timer...");
            _timerSubscription = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(ReloadWindowContentIfChanged);
        }

        public void Start()
        {
            _logger("Reading Avalonia project assembly...");
            var assemblyBytes = File.ReadAllBytes(_assemblyPath);
            var assembly = Assembly.Load(assemblyBytes);
            var builderMethod = BuildBuilderMethod(assembly);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            _logger("Setting up Avalonia app with classic desktop app lifetime...");
            var applicationBuilder = builderMethod();
            var lifetime = new ClassicDesktopStyleApplicationLifetime();
            lifetime.Startup += (sender, args) => OnApplicationStartup();
            applicationBuilder.SetupWithLifetime(lifetime);
            lifetime.Start(new string[0]);
        }

        public void Dispose()
        {
            _timerSubscription.Dispose();
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        }

        private void OnApplicationStartup()
        {
            _logger("Showing the window on main thread...");
            AvaloniaScheduler.Instance.Schedule(() =>
            {
                _window = new Window();
                _window.Show();
            });
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == null) return null;
            
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";
            var directoryName = Path.GetDirectoryName(_assemblyPath) ?? throw new NullReferenceException();
            var assemblyPath = Path.Combine(directoryName, assemblyName);
            _logger($"Loading assembly {assemblyName} from {directoryName}");

            if (!File.Exists(assemblyPath)) return null;
            var assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        private void ReloadWindowContentIfChanged(long timesReloaded)
        {
            try
            {
                var fileInfo = new FileInfo(_assemblyPath);
                var latestFileVersion = fileInfo.CreationTime.ToString(CultureInfo.InvariantCulture);
                if (_latestVersion == latestFileVersion) 
                    return;

                _logger($"File version has changed, new version is: {_latestVersion} (check #{timesReloaded})");
                _latestVersion = latestFileVersion;
                
                _logger($"Loading assembly from {_assemblyPath}");
                var assemblyBytes = File.ReadAllBytes(_assemblyPath);
                var assembly = Assembly.Load(assemblyBytes);
                var method = BuildReloaderMethod(assembly);
                
                _logger($"Invoking the {_programClassName}.{CreateReloadableControlMethod} method...");
                AvaloniaScheduler.Instance.Schedule(() => _window.Content = method(_window));
                _logger("Successfully reloaded the assembly!");
            }
            catch (Exception exception)
            {
                _logger($"Unable to reload assembly content: {exception}");
            }
        }

        private Func<Window, object> BuildReloaderMethod(Assembly assembly)
        {
            _logger($"Searching the {_programClassName}.{CreateReloadableControlMethod} method...");
            var type = assembly.GetType(_programClassName);
            if (type == null)
                throw new TypeLoadException($"Unable to load {_programClassName}");
            
            var method = type.GetMethod(CreateReloadableControlMethod);
            if (method == null)
                throw new TypeLoadException($"Unable to find {CreateReloadableControlMethod} in {_programClassName}");

            _logger($"Successfully found the {_programClassName}.{CreateReloadableControlMethod} method.");
            return window => method.Invoke(null, new object[] {window});
        }
        
        private Func<AppBuilder> BuildBuilderMethod(Assembly assembly)
        {
            _logger($"Searching the {_programClassName}.{BuildAvaloniaAppMethod} method...");
            var type = assembly.GetType(_programClassName);
            if (type == null)
                throw new TypeLoadException($"Unable to load {_programClassName}");
            
            var method = type.GetMethod(BuildAvaloniaAppMethod);
            if (method == null)
                throw new TypeLoadException($"Unable to find {BuildAvaloniaAppMethod} in {_programClassName}");
            
            _logger($"Successfully found the {_programClassName}.{BuildAvaloniaAppMethod} method.");
            return () => (AppBuilder) method.Invoke(null, new object[0]);
        }
        
        private static string ExtractFullProgramClassName(string assemblyPath)
        {
            var assemblyBytes = File.ReadAllBytes(assemblyPath);
            var assembly = Assembly.Load(assemblyBytes);
            if (assembly.FullName == null)
                throw new IOException("Unable to obtain assembly name");
            return assembly.FullName.Split(',')[0] + ".Program";
        }
    }
}