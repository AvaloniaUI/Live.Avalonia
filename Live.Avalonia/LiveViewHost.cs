using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Live.Avalonia
{
    public sealed class LiveViewHost : Window, IDisposable
    {
        private readonly string _programClassName;
        private readonly Action<string> _logger;
        private readonly string _assemblyPath;
        private readonly ILiveView _view;
        
        private IDisposable _timerSubscription;
        private Process _dotnetWatchBuildProcess;
        private string _latestVersion;

        public LiveViewHost(ILiveView view, Action<string> logger = null)
        {
            _view = view;
            _logger = logger ?? (message => { });
            var assembly = view.GetType().Assembly;
            if (assembly.FullName == null)
                throw new IOException("Bad assembly with missing name.");
            
            _assemblyPath = assembly.Location;
            _programClassName = assembly.FullName.Split(',')[0] + ".Program";
        }

        public void StartHostringLiveView()
        {
            DotnetWatchBuild();
            _logger("Loading initial content and registering observable timer...");
            _timerSubscription = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(ReloadWindowContentIfChanged);
        } 
        
        private void DotnetWatchBuild()
        {
            _logger($"Running 'dotnet watch' command for project {_programClassName}");
            var binPath = FindParentDirectory(_assemblyPath, "bin");
            var projectDirectory = Path.GetDirectoryName(binPath);
            if (projectDirectory == null)
                throw new IOException($"Unable to obtain parent directory of {binPath}");
            
            _logger($"Running dotnet watch from {projectDirectory}");
            _dotnetWatchBuildProcess = new Process
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
            _dotnetWatchBuildProcess.Start();
        }
        
        public void Dispose()
        {
            _logger("Killing dotnet watch process...");
            _dotnetWatchBuildProcess?.Kill(true);
            _dotnetWatchBuildProcess?.Dispose();
            _timerSubscription?.Dispose();
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
                var viewType = _view.GetType();
                var type = assembly.GetTypes().First(x => x.FullName == viewType.FullName);
                var method = type.GetMethod(nameof(ILiveView.CreateView));
                var instance = Activator.CreateInstance(type);
                AvaloniaScheduler.Instance.Schedule(() => Content = method!.Invoke(instance, new object[] {this}));
            }
            catch (Exception exception)
            {
                _logger($"Unable to reload assembly content: {exception}");
            }
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
    }
}