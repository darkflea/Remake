using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net; // For WebUtility.HtmlEncode

namespace Remake.Logging
{
    /// <summary>
    /// Provides an implementation of an HTML log writer that outputs log messages to a file in HTML format. This class
    /// is sealed and cannot be inherited.
    /// </summary>
    /// <remarks>The HTML output includes color-coded log entries based on log level, using a customizable
    /// palette. The log file is written in UTF-8 encoding and includes a styled header for readability. This class is
    /// not thread-safe. Instances should be disposed of properly to ensure the log file is closed and all data is
    /// flushed.</remarks>
    public sealed class HtmlLogWriter : ILogWriter
    {
        private readonly string _path;
        private StreamWriter _file;
        private Dictionary<LogLevel, LogColor> _palette;

        public HtmlLogWriter(string path) => _path = path;

        public void SetPalette(Dictionary<LogLevel, LogColor> palette) => _palette = palette;

        public void Open()
        {
            _file = new StreamWriter(_path, false, Encoding.UTF8) { AutoFlush = true };
            WriteHeader();
        }

        public void Write(string msg, LogLevel level, string logName)
        {
            // Use WebUtility.HtmlEncode to replace your custom Escape method
            string encodedMsg = WebUtility.HtmlEncode(msg);
            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            _file?.WriteLine($"<div class='{level}'>[{timestamp}] {encodedMsg}</div>");
        }

        private void WriteHeader()
        {
            if (_file == null || _palette == null) return;

            _file.WriteLine($@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ background: #151515; color: #eaeaea; font-family: monospace; }}
                        .{LogLevel.Warning} {{ color: {_palette[LogLevel.Warning].html}; }}
                        .{LogLevel.Critical} {{ color: {_palette[LogLevel.Critical].html}; font-weight: bold; }}
                        .{LogLevel.Trivial}  {{ color: {_palette[LogLevel.Trivial].html}; }}
                        .{LogLevel.Normal}   {{ color: {_palette[LogLevel.Normal].html}; }}
                        .{LogLevel.Developer}{{ color: {_palette[LogLevel.Developer].html}; }}
                    </style>
                </head>
                <body>
                    <h1>Remake Log</h1>");
        }

        public void Close()
        {
            if (_file == null) return;
            _file.WriteLine("<br>Remake shutting down.</body></html>");
            _file.Dispose();
            _file = null;
        }

        public void Dispose() => Close();
    }
}