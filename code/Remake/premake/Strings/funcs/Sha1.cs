using System.Security.Cryptography;
using System.Text;

namespace Remake.Modules;

internal static partial class StringFunctions
{
    [LuaFunction("sha1", "Compute a lowercase hex SHA-1 digest.")]
    public static string Sha1(string str)
    {
        using var sha1 = SHA1.Create();
        // Encoding.UTF8.GetBytes ensures we hash the UTF-8 stream
        byte[] bytes = Encoding.UTF8.GetBytes(str ?? "");
        byte[] digest = sha1.ComputeHash(bytes);

        var sb = new StringBuilder(digest.Length * 2);
        foreach (byte b in digest)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}