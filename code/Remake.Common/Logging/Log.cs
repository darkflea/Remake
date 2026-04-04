using System;
using System.Collections.Generic;
using System.Linq;

namespace Remake.Logging
{
    public sealed class Log
    {
        private readonly List<ILogWriter> _writers = new();
        private readonly object _lock = new();

        // Use auto-properties for cleaner code
        public string Name { get; }
        public LogLevel DetailLevel { get; set; } = LogLevel.Normal;
        public Dictionary<LogLevel, LogColor> Palette { get; private set; }

        public Log(string name) => Name = name;

        public void SetPalette(Dictionary<LogLevel, LogColor> palette)
        {
            lock (_lock)
            {
                Palette = palette;
                _writers.ForEach(w => w.SetPalette(palette));
            }
        }

        public void AddWriter(ILogWriter writer)
        {
            lock (_lock)
            {
                _writers.Add(writer);
                // Ensure the new writer gets the current palette immediately
                if (Palette != null) writer.SetPalette(Palette);
            }
        }

        public void OpenWriters()
        {
            lock (_lock) _writers.ForEach(w => w.Open());
        }

        public void LogMessage(string message, LogLevel messageLevel)
        {
            // Simple check before locking to improve performance
            if (messageLevel < DetailLevel) return;

            lock (_lock)
            {
                foreach (var w in _writers)
                    w.Write(message, messageLevel, Name);
            }
        }
    }
}