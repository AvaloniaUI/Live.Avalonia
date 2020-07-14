using System;
using System.IO;

namespace Live.Avalonia
{
    public sealed class AvaloniaReloadingTool
    {
        private readonly string _projectDirectory;
        private readonly Action<string> _logger;
        private readonly bool _verbose;

        public AvaloniaReloadingTool(string projectDirectory, bool verbose, Action<string> logger)
        {
            _projectDirectory = projectDirectory;
            _verbose = verbose;
            _logger = logger;
        }

        public void Run()
        {
            _logger($"Running Avalonia Reloading Tool from {_projectDirectory}");
            var builder = new AvaloniaReloadingBuilder(_projectDirectory, _verbose, _logger);
            var compiledAssemblyPath = builder.BuildDll();
            var assemblyName = Path.GetFileNameWithoutExtension(compiledAssemblyPath);
            _logger($"Successfully managed to build {assemblyName}: {compiledAssemblyPath}");

            _logger($"Starting hot reloading application and watching for source file changes...");
            using var watcher = new AvaloniaReloadingWatcher(compiledAssemblyPath, _logger);
            using var hosting = new AvaloniaReloadingHost(compiledAssemblyPath, _logger);
            watcher.StartWatchingSourceFiles();
            hosting.Start();
        }
    }
}