using System.Text;

namespace Remake.Modules;

internal static partial class StringFunctions
{
    [LuaFunction("hash", "Compute a 32-bit FNV-1a hash of the UTF-8 representation.")]
    public static int Hash(string str)
    {
        if (str == null) return 0;

        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;

        uint hash = fnvOffset;

        // Get UTF-8 bytes to ensure the hash matches non-C# implementations
        byte[] bytes = Encoding.UTF8.GetBytes(str);

        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= fnvPrime;
        }

        return unchecked((int)hash);
    }
}