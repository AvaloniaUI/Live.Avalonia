using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;

namespace Live.Avalonia
{
    public sealed class LiveAssemblyExtractor
    {
        private readonly Action<string> _logger;

        public LiveAssemblyExtractor(Action<string> logger) => _logger = logger;

        public Func<Window, object> ExtractCreateViewMethod(string assemblyPath)
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
                var method = liveViewType.GetMethod(nameof(ILiveView.CreateView));
                if (method == null)
                    throw new TypeLoadException($"Unable to obtain {nameof(ILiveView.CreateView)} method!");

                _logger($"Successfully managed to obtain the method {nameof(ILiveView.CreateView)}");
                return window => method.Invoke(instance, new object[] {window});
            }
            catch (Exception exception)
            {
                return window => new TextBlock
                {
                    Text = exception.ToString()
                };
            }
        }
    }
}