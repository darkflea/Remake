﻿﻿﻿﻿﻿using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remake.Modules
{
    internal partial class OverrideFunctions
    {
        // Ensure these delegates are static so they aren't garbage collected
        public static readonly LuaFunction LuaBLoadFileDelegate = LuaCallbackRegistry.CreateCallback("loadfile", PremakeLuaBLoadFile);

        /// <summary>
        /// Port of premake_luaB_loadfile.
        /// Replaces the standard Lua 'loadfile' function.
        /// </summary>
        private static int PremakeLuaBLoadFile(IntPtr state)
        {
            var l = Lua.FromIntPtr(state);
            string? fname = l.OptString(1, null);
            string? mode = l.OptString(2, null);
            int envIdx = !l.IsNone(3) ? 3 : 0;
            LuaStatus status = PremakeLuaLLoadFileX(l, fname, mode);

            // Returns the function chunk on success, or (nil, error_message) on failure.
            if (status == LuaStatus.OK)
            {
                if (envIdx != 0)
                {
                    l.PushValue(envIdx);
                    // Set the environment table as the 1st upvalue of the loaded chunk
                    if (l.SetUpValue(-2, 1) == null)
                        l.Pop(1);
                }
                return 1; // Number of results returned to Lua
            }
            else
            {
                l.PushNil();
                l.Insert(-2); // Put nil before the error message
                return 2; // Returns (nil, error)
            }
        }

        /// <summary>
        /// C# Port of premake_luaL_loadfilex.
        /// Priority: Explicit Virtual ($/) -> Relative Virtual -> Local Filesystem -> Embedded Fallback.
        /// </summary>
        public static LuaStatus PremakeLuaLLoadFileX(Lua l, string filename, string? mode)
        {
            int bottom = l.GetTop();
            bool isRequire = bottom >= 4;
            bool hasEnv = !isRequire && !l.IsNone(3);

            // Initialize with a failure state (mimicking !OKAY)
            LuaStatus status = LuaStatus.ErrSyntax;

            // Explicit Virtual Path Check
            if (filename.StartsWith("$"))
            {
                status = PremakeLoader.PremakeLoadEmbeddedScript(l, filename.Substring(2));
            }

            // Relative Virtual Path Check
            if (status != LuaStatus.OK)
            {
                l.GetGlobal("_SCRIPT_DIR");
                string? scriptDir = l.ToString(-1);
                if (!string.IsNullOrEmpty(scriptDir) && scriptDir.StartsWith("$"))
                {
                    string combined = scriptDir;
                    if (!combined.EndsWith("/")) combined += "/";
                    combined += filename;

                    // Remove potential double slash if filename started with one (though unlikely for relative)
                    if (filename.StartsWith("/")) combined = scriptDir + filename;

                    status = PremakeLoader.PremakeLoadEmbeddedScript(l, combined);
                }
                l.Pop(1);
            }

            // Local Filesystem Check
            if (status != LuaStatus.OK)
            {
                l.GetGlobal("os");
                l.GetField(-1, "locate");
                l.Remove(-2);

                l.PushString(filename);
                l.Call(1, 1);

                string test_name = l.ToString(-1);

                if (string.IsNullOrEmpty(test_name) && status != LuaStatus.OK)
                {
                    Console.WriteLine($"Failed to load {test_name} from filesystem: {status}");
                    return LuaStatus.ErrFile;
                }

                if (status != LuaStatus.OK && status != LuaStatus.ErrFile)
                {
                    // Locate succeeded, now load the file
                    status = l.LoadFile(test_name, mode);

                    if (status != LuaStatus.OK)
                    {
                        // Load failed (syntax error, etc). 
                        l.Remove(-2);
                        return status;
                    }
                }
            }

            // Final Embedded Fallback
            if (status != LuaStatus.OK)
            {
                status = PremakeLoader.PremakeLoadEmbeddedScript(l,filename);
                if (status == LuaStatus.OK) { l.PushString(filename); l.Insert(-2); }
            }

            // Finalization and Wrapping
            if (status == LuaStatus.OK)
            {
                if (hasEnv)
                {
                    l.PushNil();
                    l.Insert(-3);
                }

                // Wrap the chunk in the proxy closure
                l.PushCClosure(ChunkWrapperDelegate, 2 + (hasEnv ? 1 : 0));
            }
            else if (status == LuaStatus.ErrFile)
            {
                l.PushString($"cannot open {filename}: No such file or directory");
            }

            return status;
        }
    }
}