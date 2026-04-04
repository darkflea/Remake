using Remake;
using System;

namespace Remake.Modules
{
    internal static partial class PathFunctions
    {
        [LuaFunction("isabsolute", "Return true if the given path is absolute.")]
        public static bool IsAbsolute(string path)
        {
            return DoIsAbsolute(path);
        }

        [LuaFunction("absolutetype", "Return the absolute-type classification of a path.")]
        public static JoinMode AbsoluteType(string path)
        {
            return DoAbsoluteType(path);
        }

        private static bool DoIsAbsolute(string path)
        {
            return DoAbsoluteType(path) == JoinMode.Absolute;
        }

        private static JoinMode DoAbsoluteType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return JoinMode.Relative;

            int len = path.Length;
            int idx = 0;

            // Skip leading quotes or exclamation marks
            while (idx < len && (path[idx] == '"' || path[idx] == '!'))
                idx++;

            if (idx >= len)
                return JoinMode.Relative;

            char c = path[idx];

            // Unix-style separator or Windows-style separator
            if (c == '/' || c == '\\')
                return JoinMode.Absolute;

            // Windows drive letter (C:)
            if (idx + 1 < len && path[idx + 1] == ':')
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    return JoinMode.Absolute;
            }

            // $(foo) and %(foo)
            if (idx + 1 < len && (c == '$' || c == '%') && path[idx + 1] == '(')
            {
                int close = path.IndexOf(')', idx + 2);
                if (close == -1) return JoinMode.Relative;

                if (c == '%')
                {
                    string content = path.Substring(idx + 2, close - (idx + 2));
                    if (string.Equals(content, "Filename", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(content, "Extension", StringComparison.OrdinalIgnoreCase))
                    {
                        return JoinMode.Relative;
                    }
                }

                // If content has separators, it is relative
                for (int i = idx + 2; i < close; i++)
                {
                    char ic = path[i];
                    if (ic == '/' || ic == '\\') return JoinMode.Relative;
                }
                return JoinMode.Absolute;
            }

            // $ORIGIN
            if (c == '$')
                return JoinMode.Absolute;

            // %ORIGIN% or %{lua}
            if (c == '%')
            {
                // %{lua}
                if (idx + 1 < len && path[idx + 1] == '{')
                    return JoinMode.MaybeAbsolute;

                // %VAR%
                int close = path.IndexOf('%', idx + 1);
                if (close > idx + 1)
                {
                    // Check contents
                    for (int i = idx + 1; i < close; i++)
                    {
                        char ic = path[i];
                        if (ic == '/' || ic == '\\') return JoinMode.Relative;
                    }
                    return JoinMode.Absolute;
                }
            }

            return JoinMode.Relative;
        }
    }
}