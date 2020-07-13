using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Controls;
using ReactiveUI;

namespace Avalonia.HotReload
{
    /// <summary>
    /// This Window starts the 'dotnet watch build' process in the project directory of the passed
    /// assembly, so every time any source files in the project change, the rebuild happens, and the
    /// 'CreateReloadableControl' method is invoked. This method must be static and defined in the
    /// 'Program' class in your assembly, accepting the Window as the first argument. The IVisual
    /// obtained from 'CreateReloadableControl' method invocation replaces the content of this Window.
    /// </summary>
    public class AvaloniaReloadingWindow : Window, IDisposable
    {
        private const string CreateReloadableControlMethod = "CreateReloadableControl";
        
        private readonly IDisposable _timerSubscription;
        private readonly Process _dotnetWatchBuildProcess;
        private readonly string _programClassName;
        private readonly Action<string> _logger;
        private readonly string _assemblyPath;
        
        private string _latestVersion;

        /// <summary>
        /// Creates a new instance of AvaloniaReloadingWindow, starts the 'dotnet watch build'
        /// process, starts the timers, obtains assembly location and name.
        /// </summary>
        /// <param name="assembly">Assembly of the project you are willing to hot reload.</param>
        /// <param name="logger">Logger that can be used for debugging. You may pass Console.WriteLine.</param>
        /// <exception cref="ArgumentException">Thrown when assembly full name is not available.</exception>
        public AvaloniaReloadingWindow(Assembly assembly, Action<string> logger = null)
        {
            _logger = logger ?? (message => { });
            _assemblyPath = assembly.Location;
            if (assembly.FullName == null)
                throw new ArgumentException(@"Unable to obtain assembly full name.", nameof(assembly));

            _logger($"Obtaining assembly name from {assembly.FullName}");
            _programClassName = assembly.FullName.Split(',')[0] + ".Program";

            _logger($"Running 'dotnet watch' command for project {_programClassName}");
            _dotnetWatchBuildProcess = DotnetWatchBuild();

            _logger("Loading initial content and registering observable timer...");
            _timerSubscription = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(ReloadWindowContentIfChanged);
        }
        
        /// <summary>
        /// Kills the dependent processes, disposes the subscription.
        /// </summary>
        public void Dispose()
        {
            _logger($"Killing dotnet watch process {_dotnetWatchBuildProcess.Id}");
            _dotnetWatchBuildProcess.Kill();
            _dotnetWatchBuildProcess.Dispose();
            _timerSubscription.Dispose();
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
                
                _logger($"Searching the {_programClassName}.{CreateReloadableControlMethod} method...");
                var type = assembly.GetType(_programClassName);
                if (type == null)
                    throw new TypeLoadException($"Unable to load {_programClassName}");
                var method = type.GetMethod(CreateReloadableControlMethod);
                if (method == null)
                    throw new TypeLoadException(
                        $"Unable to find {CreateReloadableControlMethod} in {_programClassName}");
                
                _logger($"Invoking the {_programClassName}.{CreateReloadableControlMethod} method...");
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    Content = method.Invoke(null, new object[] {this});
                });
                
                _logger("Successfully reloaded the assembly!");
            }
            catch (Exception exception)
            {
                _logger($"Unable to reload assembly content: {exception}");
            }
        }
    
        private Process DotnetWatchBuild()
        {
            var binPath = FindParentDirectory(_assemblyPath, "bin");
            var projectDirectory = Path.GetDirectoryName(binPath);
            if (projectDirectory == null)
                throw new IOException($"Unable to obtain parent directory of {binPath}");
            
            _logger($"Running dotnet watch from {projectDirectory}");
            var process = new Process
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
            process.Start();
            return process;
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