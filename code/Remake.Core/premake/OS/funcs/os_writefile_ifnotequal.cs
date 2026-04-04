using KeraLua;
using Remake;
using System;
using System.IO;
using System.Text;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("writefile_ifnotequal")]
        public static int WriteFileIfNotEqual(string content, string dst)
        {
            Console.WriteLine("I demanded equality");
            try
            {
                byte[] incoming = System.Text.Encoding.UTF8.GetBytes(content);

                if (File.Exists(dst))
                {
                    byte[] existing = File.ReadAllBytes(dst);

                    if (incoming.AsSpan().SequenceEqual(existing))
                        return 0; // unchanged
                }

                Console.WriteLine($"Out filename is: {dst}");
                File.WriteAllBytes(dst, incoming);
                return 1; // written
            }
            catch (Exception ex)
            {
                throw new Exception($"unable to write file to '{dst}': {ex.Message}");
            }
        }
    }
}