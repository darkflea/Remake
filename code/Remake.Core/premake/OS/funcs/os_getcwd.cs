using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("getcwd", "Get the current working directory.")]
        public static object? GetCwd()
        {
            try
            {
                string cwd = do_getcwd();
                cwd = cwd + "/";
                return cwd.Replace('\\', '/'); // Lua receives: "path"
            }
            catch
            {
                return null;
            }
        }

        public static string do_getcwd()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}