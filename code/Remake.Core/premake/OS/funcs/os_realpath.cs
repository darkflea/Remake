using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("realpath", "Return the absolute, normalized path.")]
        public static object RealPath(string path)
        {
            try
            {
                string absolute = System.IO.Path.GetFullPath(path)
                                      .Replace('\\', '/');

                return absolute; // Lua receives a string
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"unable to fetch real path of '{path}': {ex.Message}"
                };
            }
        }
    }
}