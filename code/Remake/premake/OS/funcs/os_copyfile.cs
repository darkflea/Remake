using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("copyfile", "Copy a file from one location to another.")]
        public static object CopyFile(string src, string dst)
        {
            try
            {
                File.Copy(src, dst, true);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"unable to copy file to '{dst}', reason: '{ex.Message}'"
                };
            }
        }
    }
}