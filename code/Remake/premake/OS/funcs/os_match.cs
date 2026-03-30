using KeraLua;
using System.IO;
using System.Runtime.InteropServices;

namespace Remake.Modules
{
    // hold the search state
    public class MatchHandle
    {
        public string[] Entries = Array.Empty<string>();
        public int Index = -1;
    }

    internal partial class OSFunctions
    {
        [LuaFunction("matchstart", "Start a file match search.")]
        public static MatchHandle MatchStart(string pattern)
        {
            var handle = new MatchHandle();

            try
            {
                string dir = Path.GetDirectoryName(pattern);
                if (string.IsNullOrEmpty(dir))
                    dir = ".";

                if (!Path.IsPathRooted(dir))
                    dir = Path.GetFullPath(dir, OSFunctions.do_getcwd());

                if (Directory.Exists(dir))
                {
                    // Premake: enumerate EVERYTHING, no filtering here
                    handle.Entries = Directory.GetFileSystemEntries(dir);
                }
            }
            catch
            {
                // ignore
            }

            return handle;
        }

        [LuaFunction("matchnext", "Move to the next match.")]
        public static bool MatchNext(MatchHandle handle)
        {
            handle.Index++;
            return handle.Index < handle.Entries.Length;
        }

        [LuaFunction("matchname", "Get the name of the current match.")]
        public static string MatchName(MatchHandle handle)
        {
            if (handle.Index < 0 || handle.Index >= handle.Entries.Length) return "";
            return System.IO.Path.GetFileName(handle.Entries[handle.Index]);
        }

        [LuaFunction("matchisfile", "Is the current match a file?")]
        public static bool MatchIsFile(MatchHandle handle)
        {
            if (handle.Index < 0 || handle.Index >= handle.Entries.Length) return false;
            return File.Exists(handle.Entries[handle.Index]);
        }

        [LuaFunction("matchdone", "Clean up.")]
        public static void MatchDone(MatchHandle handle)
        {
            handle.Entries = Array.Empty<string>();
        }
    }
}