using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;

namespace Avalonia.Reloading.Tool
{
    internal sealed class AvaloniaReloadingBuilder
    {
        private readonly string _projectDirectory;
        private readonly Action<string> _logger;

        public AvaloniaReloadingBuilder(string projectDirectory, Action<string> logger)
        {
            _projectDirectory = projectDirectory;
            _logger = logger;
        }

        public string BuildDll()
        {
            _logger($"Discovering csproj file in {_projectDirectory}");
            var directoryInfo = new DirectoryInfo(_projectDirectory);
            var allFiles = directoryInfo.GetFiles();
            var projectFile = allFiles.First(file => file.Extension == ".csproj");
            UpdateMsBuildExecutablePath();
            
            _logger($"Building project: {projectFile.FullName}");
            var currentProject = new Project(projectFile.FullName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(projectFile.Name);
            var logger = new AvaloniaReloadingLogger(nameWithoutExtension, _logger);
            if (!currentProject.Build(logger)) 
                throw new IOException($"Unable to build {projectFile.Name}");
            _logger($"Successfully managed to build the project {projectFile.Name}");
            
            var producedDllFullPath = logger.LatestDllPath;
            _logger($"Obtained output .dll file path is: {producedDllFullPath}");
            return producedDllFullPath;
        }

        private void UpdateMsBuildExecutablePath()
        {
            _logger("Adjusting MSBUILD_EXE_PATH variable...");
            var startInfo = new ProcessStartInfo("dotnet", "--list-sdks") 
            { 
                RedirectStandardOutput = true
            };

            var process = Process.Start(startInfo);
            process!.WaitForExit(1000);

            var output = process.StandardOutput.ReadToEnd();
            var sdkPaths = Regex
                .Matches(output, "([0-9]+.[0-9]+.[0-9]+) \\[(.*)\\]")
                .Select(m => Path.Combine(m.Groups[2].Value, m.Groups[1].Value, "MSBuild.dll"));

            var sdkPath = sdkPaths.Last();
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", sdkPath);
        }
    }
}