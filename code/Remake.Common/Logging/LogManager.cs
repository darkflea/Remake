using System;
using System.Collections.Concurrent;

namespace Remake.Logging
{
    [Flags]
    public enum LogOutputType
    {
        None = 0,
        Text = 1 << 0,
        Html = 1 << 1,
        // Console = 1 << 2,
                        
        All = Text | Html // Helper for both
    }

    public sealed class LogManager
    {        public static LogManager Instance { get; } = new();
        private LogManager() { }

        private readonly ConcurrentDictionary<string, Log> _logs = new();
        public Log DefaultLog { get; set; }

        public Log CreateLog(string name, bool makeDefault = false)
        {
            var log = _logs.GetOrAdd(name, key => new Log(key));
            if (makeDefault || DefaultLog == null) DefaultLog = log;
            return log;
        }

        public Log GetLog(string name) => _logs.TryGetValue(name, out var log) ? log : null;

        public void LogMessage(string msg, LogLevel lvl) => DefaultLog?.LogMessage(msg, lvl);
        public void LogTrivial(string msg) => LogMessage(msg, LogLevel.Trivial);
        public void LogNormal(string msg) => LogMessage(msg, LogLevel.Normal);
        public void LogDeveloper(string msg) => LogMessage(msg, LogLevel.Developer);

        public Log CreateLog(string name, LogOutputType outputs = LogOutputType.All, bool makeDefault = false)
        {
            var log = _logs.GetOrAdd(name, key => new Log(key));
            if (makeDefault || DefaultLog == null) DefaultLog = log;

            // Automatically configure based on the selected types
            ConfigureBackendsFor(log, GetDefaultPalette(), outputs);

            return log;
        }

        public void ConfigureBackendsFor(Log log, Dictionary<LogLevel, LogColor> palette, LogOutputType outputs)
        {
            if (outputs.HasFlag(LogOutputType.Html))
                log.AddWriter(new HtmlLogWriter($"{log.Name}.html"));

            if (outputs.HasFlag(LogOutputType.Text))
                log.AddWriter(new TextLogWriter($"{log.Name}.txt"));

            log.SetPalette(palette);
            log.OpenWriters();
        }

        private Dictionary<LogLevel, LogColor> GetDefaultPalette() => new()
        {
            [LogLevel.Trivial] = new("#555555", "\u001b[90m"),
            [LogLevel.Normal] = new("#5A83CA", "\u001b[94m"),
            [LogLevel.Warning] = new("#FFB84D", "\u001b[33m"),
            [LogLevel.Critical] = new("#FF6A6A", "\u001b[91m"),
            [LogLevel.Developer] = new("#00FF66", "\u001b[92m")
        };
    }
}
