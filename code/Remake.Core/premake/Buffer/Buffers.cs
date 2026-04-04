using System.Text;

namespace Remake.Modules
{
    /// <summary>
    /// Represents a mutable text buffer for building and retrieving formatted string content, typically used for
    /// generating Premake scripts or similar output.
    /// </summary>
    /// <remarks>This class provides methods to append text and lines, retrieve the current buffer content,
    /// and clear the buffer. It is not thread-safe.</remarks>
    public class PremakeBuffer
    {
        private StringBuilder _sb = new StringBuilder();

        public void Write(string s) => _sb.Append(s);

        public void WriteLine(string s) => _sb.AppendLine(s);

        public string GetContent()
        {
            string content = _sb.ToString();
            // Premake's C code trims a single trailing newline/CR
            return content.TrimEnd('\r', '\n');
        }

        public void Clear() => _sb.Clear();
    }

    /// <summary>
    /// Provides static methods for creating and manipulating buffered string objects for use in Lua scripts.
    /// </summary>
    /// <remarks>This class exposes buffer-related functionality to Lua modules, enabling scripts to create,
    /// write to, convert, and close buffer objects. All methods are intended for use from Lua and operate on the
    /// PremakeBuffer type. This class is not intended to be instantiated.</remarks>
    [LuaModule("buffered")]
    internal static class BufferedFunctions
    {
        [LuaFunction("new", "Create a new buffer object.")]
        public static PremakeBuffer New() => new PremakeBuffer();

        [LuaFunction("write", "Write a string to the buffer.")]
        public static void Write(PremakeBuffer b, string s)
        {
            b?.Write(s);
        }

        [LuaFunction("writeln", "Write a string followed by a newline.")]
        public static void WriteLine(PremakeBuffer b, string? s = null)
        {
            if (b == null) return;
            if (s != null) b.Write(s);
            b.Write("\r\n"); // Premake specifically uses \r\n in this module
        }

        [LuaFunction("tostring", "Convert buffer content to a Lua string.")]
        public static string ToString(PremakeBuffer b)
        {
            return b?.GetContent() ?? "";
        }

        [LuaFunction("close", "Close and destroy the buffer.")]
        public static void Close(PremakeBuffer b)
        {
            b?.Clear();
        }
    }
}
