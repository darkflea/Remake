using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("mkdir", "Create a directory and all parent directories.")]
        public static object Mkdir(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"unable to create directory '{path}': {ex.Message}"
                };
            }
        }
    }
}