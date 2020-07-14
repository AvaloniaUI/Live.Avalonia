using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Live.Avalonia
{
    public sealed class LiveViewHost : Window, IDisposable
    {
        private readonly LiveFileCreationWatcher _assemblyWatcher;
        private readonly LiveSourceWatcher _sourceWatcher;
        private readonly IDisposable _subscription;
        private readonly Action<string> _logger;
        private readonly string _assemblyPath;

        public LiveViewHost(ILiveView view, Action<string> logger)
        {
            _logger = logger;
            _sourceWatcher = new LiveSourceWatcher(logger);
            _assemblyWatcher = new LiveFileCreationWatcher(logger);
            _assemblyPath = view.GetType().Assembly.Location;
            
            var extractor = new LiveAssemblyExtractor(logger);
            _subscription = _assemblyWatcher
                .FileChanged
                .ObserveOn(AvaloniaScheduler.Instance)
                .Select(unit => extractor.ExtractCreateViewMethod(_assemblyPath))
                .Subscribe(method => Content = method(this));

            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                Dispose();
                Process.GetCurrentProcess().Kill();
            };
            
            Console.CancelKeyPress += (sender, args) =>
            {
                Dispose();
                Process.GetCurrentProcess().Kill();
            };
        }

        public void StartWatchingProjectFiles()
        {
            _logger("Starting source and assembly file watchers...");
            _sourceWatcher.StartWatchingAssemblySources(_assemblyPath);
            _assemblyWatcher.StartWatchingFileCreation(_assemblyPath);
        }

        public void Dispose()
        {
            _logger("Disposing LiveViewHost internals...");
            _sourceWatcher.Dispose();
            _assemblyWatcher.Dispose();
            _subscription.Dispose();
            _logger("Successfully disposed LiveViewHost internals.");
        }
    }
}