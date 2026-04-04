using Remake;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Remake.Modules
{
    internal static partial class PathFunctions
    {
        private static string DoTranslate(string value, char sep)
        {
            return value.Replace('/', sep).Replace('\\', sep);
        }

        [LuaFunction("translate", "Translate slashes in a path or list of paths.")]
        public static object Translate(object value, char? sep = null)
        {
            // Premake ALWAYS uses '/' as the default separator
            char separator = sep ?? '/';

            // table -> return array
            if (value is IEnumerable<object> list && value is not string)
            {
                return list
                    .Select(v => DoTranslate(v?.ToString() ?? string.Empty, separator))
                    .ToArray();
            }

            // single value -> treat as string
            return DoTranslate(value?.ToString() ?? string.Empty, separator);
        }
    }
}