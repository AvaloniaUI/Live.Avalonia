using System;
using System.Diagnostics;
using System.IO;

namespace Avalonia.Reloading.Tool
{
    public sealed class AvaloniaReloadingWatcher : IDisposable
    {
        private readonly Action<string> _logger;
        private readonly string _assemblyPath;
        private Process _process;
        
        public AvaloniaReloadingWatcher(string assemblyPath, Action<string> logger)
        {
            _assemblyPath = assemblyPath;
            _logger = logger;
        }

        public void StartWatchingSourceFiles()
        {
            var binPath = FindParentDirectory(_assemblyPath, "bin");
            var projectDirectory = Path.GetDirectoryName(binPath);
            if (projectDirectory == null)
                throw new IOException($"Unable to obtain parent directory of {binPath}");
            
            _logger($"Running dotnet watch from {projectDirectory}");
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "watch build",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                    WorkingDirectory = projectDirectory
                }
            };
            _process.Start();
        }

        private static string FindParentDirectory(string filePath, string directoryName)
        {
            var currentPath = filePath;
            while (true)
            {
                currentPath = Path.GetDirectoryName(currentPath);
                if (currentPath == null)
                    throw new IOException($"Unable to get parent directory of {filePath} named {directoryName}");
                    
                var directoryInfo = new DirectoryInfo(currentPath);
                if (directoryName == directoryInfo.Name)
                    return currentPath;
            }
        }

        public void Dispose()
        {
            _logger($"Killing dotnet watch process {_process?.Id} with all child processes");
            _process?.Kill(true);
            _process?.Dispose();
        }
    }
}