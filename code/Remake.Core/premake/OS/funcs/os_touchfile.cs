using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("touchfile", "Update the timestamp of a file or create it if missing.")]
        public static object TouchFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    // Update timestamp
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                    return 0; // Premake: 0 = updated
                }
                else
                {
                    // Create empty file
                    using (File.Create(path)) { }
                    return 1; // Premake: 1 = created
                }
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    -1,
                    $"unable to touch file '{path}': {ex.Message}"
                };
            }
        }
    }
}