using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Remake.Logging
{
    /// <summary>
    /// Provides a log writer that outputs log entries as plain text to a file using UTF-8 encoding.
    /// </summary>
    /// <remarks>This class is intended for use with logging frameworks that require writing log messages to a
    /// text file. It is not thread-safe; callers should ensure that access to an instance is properly synchronized if
    /// used from multiple threads. The log file is overwritten each time the writer is opened.</remarks>
    public sealed class TextLogWriter : ILogWriter
    {
        private readonly string _path;
        private StreamWriter _file;

        public TextLogWriter(string path) => _path = path;

        public void SetPalette(Dictionary<LogLevel, LogColor> palette) { }

        public void Open()
        {
            _file = new StreamWriter(_path, false, Encoding.UTF8) { AutoFlush = true };
        }

        public void Write(string msg, LogLevel level, string logName)
        {
            _file?.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {msg}");
        }

        public void Close()
        {
            _file?.Dispose();
            _file = null;
        }

        public void Dispose() => Close();
    }
}
