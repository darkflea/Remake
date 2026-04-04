using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("comparefiles", "Compare two files for byte-for-byte equality.")]
        public static object CompareFiles(string firstPath, string secondPath)
        {
            try
            {
                FileInfo f1 = new FileInfo(firstPath);
                FileInfo f2 = new FileInfo(secondPath);

                // Quick length check
                if (f1.Length != f2.Length)
                    return false;

                // Byte-by-byte comparison
                using (FileStream fs1 = f1.OpenRead())
                using (FileStream fs2 = f2.OpenRead())
                {
                    byte[] buffer1 = new byte[4096];
                    byte[] buffer2 = new byte[4096];

                    int bytesRead;
                    while ((bytesRead = fs1.Read(buffer1, 0, buffer1.Length)) > 0)
                    {
                        fs2.Read(buffer2, 0, buffer2.Length);

                        if (!buffer1.AsSpan(0, bytesRead)
                                    .SequenceEqual(buffer2.AsSpan(0, bytesRead)))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (FileNotFoundException)
            {
                return new object?[] { null, "failed to open file (not found)" };
            }
            catch (Exception ex)
            {
                return new object?[] { null, ex.Message };
            }
        }
    }
}