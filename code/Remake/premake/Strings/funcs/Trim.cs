namespace Remake.Modules;

internal static partial class StringFunctions
{
    [LuaFunction("trim", "Trim whitespace from both ends of a string.")]
    public static string Trim(string s)
    {
        return s?.Trim() ?? "";
    }
}