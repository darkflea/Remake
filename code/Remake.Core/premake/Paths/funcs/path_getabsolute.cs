using KeraLua;

namespace Remake.Modules
{
    internal partial class PathFunctions
    {
        [LuaFunction("getabsolute")]
        public static string GetAbsolute(string value, string? relativeTo = null)
        {
            if (string.IsNullOrEmpty(value))
                value = ".";

            string baseDir = relativeTo ?? Directory.GetCurrentDirectory();

            // Combine manually to match Premake behavior
            string combined;
            if (DoIsAbsolute(value))
                combined = value;
            else
                combined = baseDir.TrimEnd('/', '\\') + "/" + value;

            // Normalize using your existing Normalize() function
            string normalized = Normalize(combined);

            return normalized;
        }

        private static string DoGetAbsolute(string value, string? relativeTo)
        {
            string before = value ?? "<null>";
            string baseDir = relativeTo ?? OSFunctions.do_getcwd();
            bool isAbs = DoIsAbsolute(value);

            if (!isAbs)
            {
                value = System.IO.Path.Combine(baseDir, value);
            }

            string fullPath = System.IO.Path.GetFullPath(value).Replace('\\', '/');
            fullPath = fullPath.TrimEnd('/');
            return fullPath;
        }
    }
}