using KeraLua;
using Remake.Modules;
using System;
using System.IO;

namespace Remake
{
    [Flags]
    public enum FileSearchMask
    {
        SearchLocal     = 0x01,   // SEARCH_LOCAL
        SearchScripts   = 0x02,   // SEARCH_SCRIPTS
        SearchPath      = 0x04,   // SEARCH_PATH
        SearchEmbedded  = 0x08    // SEARCH_EMBEDDED
    }

    public static class PremakeFinder
    {
        // Make sure all delegates are static so GC dont collect em
        public static readonly LuaFunction SearcherLuaDelegate = PremakeFinder.PremakeSearcherLua;

        // This corresponds to the global scripts_path (e.g. from --scripts cmd line arg)
        public static string? ScriptsPath { get; set; }

        /// <summary>
        /// Port of premake_locate_file.
        /// Searches for a script based on the provided bitmask.
        /// </summary>
        public static LuaStatus PremakeLocateFile(Lua l, string filename, FileSearchMask searchMask)
        {
            // 1. SearchLocal: Check current working directory
            if (searchMask.HasFlag(FileSearchMask.SearchLocal))
            {
                if (File.Exists(filename))
                {
                    // Equivalent to: path.getabsolute(filename)
                    string absolutePath = Path.GetFullPath(filename).Replace('\\', '/');
                    l.PushString(absolutePath);
                    return LuaStatus.OK;
                }
            }

            // 2. SearchScripts: Check the path provided via --scripts
            if (!string.IsNullOrEmpty(ScriptsPath) && searchMask.HasFlag(FileSearchMask.SearchScripts))
            {
                if (DoLocate(l, filename, ScriptsPath))
                    return LuaStatus.OK;
            }

            // 3. SearchPath: Check the PREMAKE_PATH environment variable
            if (searchMask.HasFlag(FileSearchMask.SearchPath))
            {
                string? envPath = Environment.GetEnvironmentVariable("PREMAKE_PATH");
                if (!string.IsNullOrEmpty(envPath))
                {
                    if (DoLocate(l, filename, envPath))
                        return LuaStatus.OK;
                }
            }

            // 4. SearchEmbedded: Check the BuiltinScripts Map
            if (searchMask.HasFlag(FileSearchMask.SearchEmbedded))
            {
                var embedded = new LoadEmbeddedScript();

                // First check if the script is embedded with the original filename
                var mapping = embedded.Find(filename);

                if (mapping != null)
                {
                    l.PushString("$/" + filename);
                    return LuaStatus.OK;
                }
            }

            return (LuaStatus)(-1); // !OKAY
        }

        /// <summary>
        /// Helper to find a module path using package.path.
        /// Replaces the missing or undefined l.L_SearchPath.
        /// </summary>
        private static string? SearchPath(Lua l, string name, string path)
        {
            // We use the Lua-side package.searchpath function
            l.GetGlobal("package");
            l.GetField(-1, "searchpath"); // [package, searchpath]

            l.PushString(name);           // [package, searchpath, name]
            l.PushString(path);           // [package, searchpath, name, path]

            // Call package.searchpath(name, path)
            if (l.PCall(2, 1, 0) != LuaStatus.OK)
            {
                l.Pop(1); // Pop error message if searchpath failed
                return null;
            }

            // searchpath returns the path on success, or nil on failure
            string? result = l.ToString(-1);
            l.Pop(2); // Pop result and package table

            return result;
        }

        /// <summary>
        /// Custom Lua searcher function to be added to package.searchers.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static int PremakeSearcherLua(IntPtr state)
        {
            var l = Lua.FromIntPtr(state);
            string name = l.CheckString(1);
            l.GetField(Lua.UpValueIndex(1), "path");
            string? path = l.ToString(-1);
            l.Pop(1);

            if (string.IsNullOrEmpty(path))
                return 0;

            string? filename = SearchPath(l, name, path);

            if (filename != null)
            {
                LuaStatus status = OverrideFunctions.PremakeLuaLLoadFileX(l, filename, null);
                if (status == LuaStatus.OK)
                {
                    l.PushString(filename);
                    return 2;
                }
                return (int)l.Error();
            }

            var embeddedPath = name.Replace('.', '/') + ".lua";
            var embedded = new LoadEmbeddedScript();

            if (embedded.Find(embeddedPath) != null)
            {
                LuaStatus status = embedded.Load(l, embeddedPath);
                if (status == LuaStatus.OK)
                {
                    l.PushString("$/" + embeddedPath);
                    return 2;
                }
            }

            l.PushString($"\n\tno file '{name}' in Premake search paths");
            return 1;
        }

        /// <summary>
        /// Helper to search through a delimited path list (like PATH or PREMAKE_PATH)
        /// </summary>
        private static bool DoLocate(Lua l, string filename, string pathList)
        {
            // Split by semicolon (Windows style) or colon (Unix style)
            string[] paths = pathList.Split(new[] { ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string path in paths)
            {
                string fullPath = Path.Combine(path, filename);
                if (File.Exists(fullPath))
                {
                    l.PushString(Path.GetFullPath(fullPath).Replace('\\', '/'));
                    return true;
                }
            }
            return false;
        }
    }
}