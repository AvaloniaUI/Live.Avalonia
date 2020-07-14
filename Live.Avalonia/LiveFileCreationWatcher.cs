using System;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Live.Avalonia
{
    public sealed class LiveFileCreationWatcher : IDisposable
    {
        private readonly Subject<Unit> _fileChanged = new Subject<Unit>();
        private readonly Action<string> _logger;
        
        private IDisposable _timerSubscription;
        private string _latestVersion;

        public LiveFileCreationWatcher(Action<string> logger) => _logger = logger;

        public IObservable<Unit> FileChanged => _fileChanged;

        public void StartWatchingFileCreation(string filePath)
        {
            _logger($"Registering observable timer-based watcher for file at: {filePath}");
            _timerSubscription = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(check => HandlePeriodicCheck(check, filePath));
        }

        private void HandlePeriodicCheck(long checkNumber, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var latestFileVersion = fileInfo.CreationTime.ToString(CultureInfo.InvariantCulture);
            if (_latestVersion == latestFileVersion) 
                return;

            _logger($"File version has changed, new version is: {_latestVersion} (check #{checkNumber})");
            _latestVersion = latestFileVersion;
            _fileChanged.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _fileChanged.Dispose();
            _timerSubscription?.Dispose();
        }
    }
}