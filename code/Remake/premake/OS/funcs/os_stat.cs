using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("stat", "Return file metadata such as mtime and size.")]
        public static object Stat(string filename)
        {
            try
            {
                var info = new FileInfo(filename);

                if (!info.Exists)
                {
                    return new object?[]
                    {
                        null,
                        $"'{filename}' was not found"
                    };
                }

                long mtime = new DateTimeOffset(info.LastWriteTimeUtc)
                                .ToUnixTimeSeconds();

                return new Dictionary<string, object?>
                {
                    ["mtime"] = mtime,
                    ["size"] = (double)info.Length
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new object?[]
                {
                    null,
                    $"'{filename}' could not be accessed"
                };
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"An unknown error occurred while accessing '{filename}': {ex.Message}"
                };
            }
        }
    }
}