namespace Remake.Modules;

internal static partial class StringFunctions
{
    [LuaFunction("endswith", "Return true if a string ends with a suffix.")]
    public static bool EndsWith(string str, string suffix)
    {
        // Standard string comparison works fine here as the logic is equivalent
        return (str ?? "").EndsWith(suffix ?? "", StringComparison.Ordinal);
    }
}