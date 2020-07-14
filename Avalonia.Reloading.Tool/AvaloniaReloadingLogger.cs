using System;
using System.Linq;
using Microsoft.Build.Framework;

namespace Avalonia.Reloading.Tool
{
    internal sealed class AvaloniaReloadingLogger : ILogger
    {
        private readonly Action<string> _logger;
        private readonly string _projectName;
        private IEventSource _source;
            
        public AvaloniaReloadingLogger(string projectName, Action<string> logger)
        {
            _projectName = projectName;
            _logger = logger;
        }

        public void Initialize(IEventSource source)
        {
            _source = source;
            _source.AnyEventRaised += OnAnyEventRaised;
        }
            
        public string LatestDllPath { get; private set; }

        public void Shutdown() => _source.AnyEventRaised -= OnAnyEventRaised;

        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Minimal;
        
        public string Parameters { get; set; }
            
        private void OnAnyEventRaised(object sender, BuildEventArgs args)
        {
            var message = args.Message;
            if (!message.EndsWith($"{_projectName}.dll"))
                return;

            _logger(message);
            var splitPointer = args.Message.Split(' ');
            var rightHandSide = splitPointer.Last();
            var trimmedPath = rightHandSide.Trim();
            LatestDllPath = trimmedPath;
        }
    }
}