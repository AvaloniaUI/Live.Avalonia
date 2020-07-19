using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Live.Avalonia
{
    internal sealed class LiveControlLoader
    {
        private readonly Action<string> _logger;

        public LiveControlLoader(Action<string> logger) => _logger = logger;

        public object LoadControl(string assemblyPath, Window window)
        {
            try
            {
                _logger($"Loading assembly from {assemblyPath}");
                var assemblyBytes = File.ReadAllBytes(assemblyPath);
                var assembly = Assembly.Load(assemblyBytes);

                _logger("Obtaining ILiveView interface implementation...");
                var interfaceType = typeof(ILiveView);
                var allImplementations = assembly.GetTypes()
                    .Where(type => interfaceType.IsAssignableFrom(type))
                    .ToList();

                if (allImplementations.Count == 0)
                    throw new TypeLoadException($"No ILiveView interface implementations found in {assemblyPath}");
                if (allImplementations.Count > 1)
                    throw new TypeLoadException(
                        $"Multiple ILiveView interface implementations found in {assemblyPath}");

                _logger("Successfully managed to obtain ILiveView interface implementation, activating...");
                var liveViewType = allImplementations.First();
                var instance = Activator.CreateInstance(liveViewType);
                var name = nameof(ILiveView.CreateView);

                var method = liveViewType.GetMethod(name) ?? interfaceType.GetMethod(name);

                if (method == null)
                    throw new TypeLoadException($"Unable to obtain {nameof(ILiveView.CreateView)} method!");
                
                _logger($"Successfully managed to obtain the method {nameof(ILiveView.CreateView)}, creating control.");
                return method.Invoke(instance, new object[] {window});
            }
            catch (Exception exception)
            {
                return new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = exception.ToString()
                };
            }
        }
    }
}