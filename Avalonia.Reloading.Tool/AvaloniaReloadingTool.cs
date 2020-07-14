using System;
using System.IO;

namespace Avalonia.Reloading.Tool
{
    public sealed class AvaloniaReloadingTool
    {
        private readonly string _projectDirectory;
        private readonly Action<string> _logger;

        public AvaloniaReloadingTool(string projectDirectory, Action<string> logger)
        {
            _projectDirectory = projectDirectory;
            _logger = logger;
        }

        public void Run()
        {
            _logger($"Running Avalonia Reloading Tool from {_projectDirectory}");
            var builder = new AvaloniaReloadingBuilder(_projectDirectory, _logger);
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