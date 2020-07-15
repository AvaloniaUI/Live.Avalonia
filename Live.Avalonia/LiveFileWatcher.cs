using System;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Live.Avalonia
{
    internal sealed class LiveFileWatcher : IDisposable
    {
        private readonly Subject<string> _fileChanged = new Subject<string>();
        private readonly Action<string> _logger;
        
        private IDisposable _timerSubscription;
        private string _latestSignature;

        public LiveFileWatcher(Action<string> logger) => _logger = logger;

        public IObservable<string> FileChanged => _fileChanged;

        public void StartWatchingFileCreation(string filePath)
        {
            _logger($"Registering observable timer-based watcher for file at: {filePath}");
            _timerSubscription = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(check => HandlePeriodicCheck(check, filePath));
        }

        private void HandlePeriodicCheck(long checkNumber, string filePath)
        {
            if (!File.Exists(filePath))
                return;
            
            var hash = FileHash(filePath);
            if (_latestSignature == hash)
                return;
            
            _logger($"File version has changed! (check #{checkNumber})");
            _latestSignature = hash;
            _fileChanged.OnNext(filePath);
        }

        public void Dispose()
        {
            _logger("Stopping the file creation watcher timer...");
            _fileChanged.Dispose();
            _timerSubscription?.Dispose();
        }

        private static string FileHash(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var fileBytes = File.ReadAllBytes(filePath);
            return BitConverter.ToString(md5.ComputeHash(fileBytes));
        }
    }
}