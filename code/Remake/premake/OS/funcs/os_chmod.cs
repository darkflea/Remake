using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("chmod", "Set file permissions using an octal mode string.")]
        public static object Chmod(string path, string modeStr)
        {
            try
            {
                int mode = Convert.ToInt32(modeStr, 8);
                mode &= 0xFFFF;
                File.SetUnixFileMode(path, (UnixFileMode)mode);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"unable to set mode {modeStr} on '{path}': {ex.Message}"
                };
            }
        }
    }
}