using System;
using System.Collections.Generic;

namespace Remake.Logging
{
    public enum LogLevel { Trivial, Normal, Warning, Critical, Developer }

    public record LogColor(string html, string ansi);

    public interface ILogWriter : IDisposable
    {
        void Open();
        void Close();
        void SetPalette(Dictionary<LogLevel, LogColor> palette);
        void Write(string message, LogLevel level, string logName);
    }
}