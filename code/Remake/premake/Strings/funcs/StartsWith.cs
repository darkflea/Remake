namespace Remake.Modules;

internal static partial class StringFunctions
{
    [LuaFunction("startswith", "Return true if a string starts with a prefix.")]
    public static bool StartsWith(string str, string prefix)
    {
        return (str ?? "").StartsWith(prefix ?? "", StringComparison.Ordinal);
    }
}