using Remake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remake.Modules
{
    internal static partial class PathFunctions
    {
        private const char DeferredJoinDelimiter = '\a';
        private const int BufferSize = 0x4000;

        [LuaFunction("join", "Join path segments into a single normalized path.")]
        public static string Join(params object[] parts)
        {
            return PathJoinInternal(ToStrings(parts), allowDeferredJoin: false);
        }

        [LuaFunction("deferredjoin", "Join path segments, marking maybe-absolute parts for deferred resolution.")]
        public static string DeferredJoin(params object[] parts)
        {
            return PathJoinInternal(ToStrings(parts), allowDeferredJoin: true);
        }

        [LuaFunction("hasdeferredjoin", "Return true if the path contains a deferred join marker.")]
        public static bool HasDeferredJoin(string path)
        {
            return path != null && path.Contains(DeferredJoinDelimiter);
        }

        [LuaFunction("resolvedeferredjoin", "Resolve a deferred-join path into a concrete path.")]
        public static string ResolveDeferredJoin(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.Contains(DeferredJoinDelimiter))
                return path ?? string.Empty;

            string[] parts = path.Split(DeferredJoinDelimiter);
            var sb = new StringBuilder(BufferSize);

            foreach (var part in parts)
                PathJoinSingle(sb, part, allowDeferredJoin: false);

            return sb.ToString();
        }

        private static IEnumerable<string> ToStrings(IEnumerable<object> parts)
        {
            if (parts == null)
                yield break;

            foreach (var p in parts)
                yield return p?.ToString() ?? string.Empty;
        }

        private static string PathJoinInternal(IEnumerable<string> parts, bool allowDeferredJoin)
        {
            var sb = new StringBuilder(BufferSize);

            foreach (var raw in parts)
            {
                if (raw == null)
                    continue;

                PathJoinSingle(sb, raw, allowDeferredJoin);
            }

            var result = sb.ToString();
            return string.IsNullOrEmpty(result) ? "." : result;
        }

        private static void PathJoinSingle(StringBuilder sb, string part, bool allowDeferredJoin)
        {
            if (string.IsNullOrEmpty(part))
                return;

            // Normalize slashes to match Premake behavior
            part = part.Replace('\\', '/');

            if (part == ".")
                return;

            // Remove leading "./"
            while (part.StartsWith("./", StringComparison.Ordinal))
                part = part.Substring(2);

            // Remove trailing "/" safely
            while (part.Length > 1 && part[^1] == '/')
                part = part[..^1];

            JoinMode absoluteType = AbsoluteType(part);
            if (!allowDeferredJoin && absoluteType == JoinMode.MaybeAbsolute)
                absoluteType = JoinMode.Relative;

            switch (absoluteType)
            {
                case JoinMode.Absolute:
                    sb.Clear();
                    break;

                case JoinMode.Relative:
                    // Handle ".." prefix by trimming last segment
                    while (part.StartsWith("..", StringComparison.Ordinal) && sb.Length > 0)
                    {
                        string current = sb.ToString();
                        int lastSlash = current.LastIndexOf('/');
                        string lastSegment = lastSlash == -1
                            ? current
                            : current.Substring(lastSlash + 1);

                        // Mirror C logic: stop if segment is not trimmable
                        if (lastSegment == ".." ||
                            lastSegment == "." ||
                            lastSegment.Contains("**") ||
                            lastSegment.Contains('$'))
                        {
                            break;
                        }

                        if (lastSlash == -1)
                            sb.Clear();
                        else
                            sb.Length = lastSlash;

                        part = part.Substring(2);
                        if (part.StartsWith("/", StringComparison.Ordinal))
                            part = part.Substring(1);
                    }

                    if (sb.Length > 0 && sb[sb.Length - 1] != '/')
                        sb.Append('/');
                    break;

                case JoinMode.MaybeAbsolute:
                    sb.Append(DeferredJoinDelimiter);
                    break;
            }

            sb.Append(part);
        }
    }
}