using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("islink", "Return true if the given path is a symbolic link.")]
        public static bool IsLink(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);

                // Matches Premake semantics: symlinks and junctions both count
                return attr.HasFlag(FileAttributes.ReparsePoint);
            }
            catch
            {
                // Missing file, permission denied, invalid path → false
                return false;
            }
        }
    }
}