using Remake;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
       [LuaFunction("isfile", "Return true if the given path is a file.")]
        public static bool IsFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Normalize slashes for Windows .NET
            string normalizedPath = path.Replace('/', System.IO.Path.DirectorySeparatorChar);

            // Resolve relative paths against Premake's initial working directory,
            // not the current (possibly changed) process directory.
            if (!System.IO.Path.IsPathRooted(normalizedPath))
            {
                normalizedPath = System.IO.Path.GetFullPath(
                    normalizedPath,
                    OSFunctions.do_getcwd()
                );
            }
            return File.Exists(normalizedPath);
        }
    }
}