﻿using KeraLua;
using Remake.Modules;
using static Remake.RemakeHelp;

namespace Remake
{
    public class PremakeHost
    {
        private string ScriptsPath;

        /// <summary>
        /// Port of premake_init.
        /// Inits the Premake Lua env, overrides standard loaders, 
        /// and sets up global metadta stuff.
        /// </summary>
        public PremakeResult Init(string rootdir)
        {
            var result = new PremakeResult
            {
                LuaStatus = LuaStatus.OK,
                ExitCode = 0,
                ErrorMessage = "Nothing"
            };

            var l = LuaVM.Instance.State;
            l.PushCFunction(OverrideFunctions.LuaBLoadFileDelegate);
            l.SetGlobal("loadfile");
            l.PushCFunction(OverrideFunctions.LuaBDofileDelegate);
            l.SetGlobal("dofile");
            l.GetGlobal("package");        // [package]
            l.GetField(-1, "searchers");   // [package, searchers]
            l.PushInteger(2);              // [package, searchers, 2]
            l.PushValue(-3);               // [package, searchers, 2, package] (as upvalue)
            l.PushCClosure(PremakeFinder.SearcherLuaDelegate, 1); // [package, searchers, 2, closure]
            l.SetTable(-3);                // searchers[2] = closure
            l.Pop(2);                      // Clean stack

            // Register Modules (LuaL_register equivalent)
            LuaModule.RegisterAll(l);

            l.PushString(BuildInfo.REMAKE_VERSION);
            l.SetGlobal("_REMAKE_VERSION");
            l.PushString(BuildInfo.LUA_COPYRIGHT);
            l.SetGlobal("_COPYRIGHT");
            l.PushString(BuildInfo.PREMAKE_VERSION);
            l.SetGlobal("_PREMAKE_VERSION");
            l.PushString(BuildInfo.PREMAKE_COPYRIGHT);
            l.SetGlobal("_PREMAKE_COPYRIGHT");
            l.PushString(BuildInfo.PREMAKE_PROJECT_URL);
            l.SetGlobal("_PREMAKE_URL");

            // Find the user's home directory
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(homeDir)) homeDir = "~";
            l.PushString(PathFunctions.EnsureTrailingSlash(homeDir.Replace('\\', '/')));
            l.SetGlobal("_USER_HOME_DIR");

            // initial working directory
            l.PushString(rootdir);
            l.SetGlobal("_WORKING_DIR");       

            return result;
        }

        /// <summary>
        /// Port of premake_execute.
        /// Manages init sequence and hands control to Lua main entry point.
        /// </summary>
        /// <param name="args">Cmd line args (from Main).</param>
        /// <param name="script">The bootstrap script name (usually "_premake_main.lua").</param>
        /// <returns>Exit code (0 is gud).</returns>
        public PremakeResult Execute(string[] args, string script)
        {
            bool isHelp = args.Contains("--help");
            bool isVersion = args.Contains("--version");
            HelpDisplayMode? mode = null;

            if (isHelp) mode = HelpDisplayMode.Help;
            if (isVersion) mode = HelpDisplayMode.Version;

            RemakeHelp helpSystem = null;

            if (isHelp || isVersion)
            {
                helpSystem = new RemakeHelp();
                OverrideFunctions.CurrentHelpContext = helpSystem;
            }

            var result = new PremakeResult
            {
                LuaStatus = LuaStatus.OK,
                ExitCode = 0,
                ErrorMessage = "Nothing to report"
            };

            var l = LuaVM.Instance.State;

            l.PushString(PathFunctions.LocateExecutable());
            l.SetGlobal("_PREMAKE_COMMAND");

            // Process command line args and fill _ARGV global
            var status = ProcessArguments(args);
            if (status != LuaStatus.OK)
            {
                result.LuaStatus = LuaStatus.ErrArguments;
                result.ErrorMessage = $"Error processing arguments: {status}";
                return result;
            }

            BuildPremakePath();

            // run bootstrap
            if (RunPremakeMain(l, script) != 0)
            {
                result.LuaStatus = LuaStatus.ErrSyntax;
                result.ErrorMessage = $"Error running: {status}";
                return result;
            }

            try
            {
                result = RunStage(l, "_premake_premain");

                if (isHelp)
                {
                    l.PushCFunction(OverrideFunctions.LuaPrintFDelegate);
                    l.SetGlobal("printf");
                }

                result = RunStage(l, "_premake_main");

                if ((isHelp || isVersion) && helpSystem != null)
                {
                    helpSystem.Display(mode.Value);
                }
                else
                {
                    result = RunStage(l, "_premake_postmain");
                }

                // extract the integer exit code
                PremakeRuntime.Result.ExitCode = (int)l.ToNumber(-1);
                l.Pop(1);
            }
            finally
            {
                OverrideFunctions.CurrentHelpContext = null; // prevent a memory leak :)
            }

            return result;
        }

        private PremakeResult RunStage(Lua l, string function)
        {
            var result = new PremakeResult
            {
                LuaStatus = LuaStatus.OK,
                ExitCode = 0,
                ErrorMessage = "Nothing to report"
            };

            /// Call the stage function (e.g. premake._stage2())
            l.GetGlobal(function);
            result.LuaStatus = l.PCall(0, 1, 0);

            if (result.LuaStatus != LuaStatus.OK)
            {
                result.LuaStatus = LuaStatus.ErrRun;
                result.ErrorMessage = $"Error running _premake_main: {result.LuaStatus}";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Port of process_arguments
        /// Copy all command line args into script-side _ARGV global.
        /// </summary>
        /// <param name="l">Lua state</param>
        /// <param name="args">Args from Main(string[] args)</param>
        /// <returns>0 (OKAY)</returns>
        private LuaStatus ProcessArguments(string[] args)
        {
            var l = LuaVM.Instance.State;
            // lua_newtable(L);
            l.NewTable();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                // Push the string value to the stack
                l.PushString(arg);
                long nextIndex = l.Length(-2) + 1;
                l.RawSetInteger(-2, nextIndex);
                ReadOnlySpan<char> argSpan = arg.AsSpan();

                if (argSpan.StartsWith("/scripts="))
                {
                    SetScriptsPath(arg[9..]);
                }
                else if (argSpan.StartsWith("--scripts="))
                {
                    SetScriptsPath(arg[10..]);
                }
            }

            l.SetGlobal("_ARGV");

            return LuaStatus.OK; // Equivalent to OKAY
        }

        /// <summary>
        /// Helper to mimic side-effect of set_scripts_path in C code.
        /// Usually converts path to absolute path.
        /// </summary>
        private void SetScriptsPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                // Normalize to absolute path and ensure forward slashes for Lua
                ScriptsPath = PathFunctions.EnsureTrailingSlash(Path.GetFullPath(path).Replace('\\', '/'));
                Console.WriteLine($"Script path = {ScriptsPath}");
            }
            catch
            {
                ScriptsPath = path;
            }
        }

        /// <summary>
        /// Builds and sets the Lua global variable 'premake.path' by combining script, environment, and standard search
        /// paths.
        /// </summary>
        /// <remarks>This method constructs the 'premake.path' variable used by the Lua environment to
        /// locate Premake scripts and modules. It combines the current directory, an optional scripts path, the
        /// PREMAKE_PATH environment variable, the user's home directory, and standard system directories. This ensures
        /// that Premake can locate resources in a variety of common locations. Call this method before executing Lua
        /// scripts that depend on 'premake.path'.</remarks>
        private void BuildPremakePath()
        {
            var l = LuaVM.Instance.State;

            l.GetGlobal("premake");
            int top = l.GetTop();

            // Start by searching the current working directory
            l.PushString(".");

            // The --scripts arg
            if (!string.IsNullOrEmpty(ScriptsPath))
            {
                l.PushString(";");
                l.PushString(ScriptsPath);
            }

            // PREMAKE_PATH env var
            string? envValue = Environment.GetEnvironmentVariable("PREMAKE_PATH");
            if (!string.IsNullOrEmpty(envValue))
            {
                l.PushString(";");
                l.PushString(envValue);
            }

            // ~/.premake
            l.PushString(";");
            l.GetGlobal("_USER_HOME_DIR");
            l.PushString(".premake");
            l.Concat(2); // Join _USER_HOME_DIR and /.premake
            l.PushString(";/usr/local/share/premake;/usr/share/premake");
            l.Concat(l.GetTop() - top);
            l.SetField(-2, "path");
            l.Pop(1);
        }

        /// <summary>
        /// Port of run_premake_main. 
        /// Locates and executes initial bootstrap script.
        /// </summary>
        private LuaStatus RunPremakeMain(Lua L, string script)
        {
            // 1. Locate the script (embedded or real file)
            var searchMask =
                FileSearchMask.SearchScripts |
                FileSearchMask.SearchEmbedded;

            // Later you can add: if (--localscripts) searchMask |= SearchLocal;
            var z = PremakeFinder.PremakeLocateFile(L, script, searchMask);

            // Fallback: local filesystem + PREMAKE_PATH
            if (z != LuaStatus.OK)
            {
                z = PremakeFinder.PremakeLocateFile(
                    L,
                    script,
                    FileSearchMask.SearchLocal |
                    FileSearchMask.SearchScripts |
                    FileSearchMask.SearchPath |
                    FileSearchMask.SearchEmbedded
                );
            }

            if (z != LuaStatus.OK)
                return z;

            // 2. Extract the located filename
            string located = L.ToString(-1);
            L.Pop(1);

            // 3. Embedded script?
            if (located.StartsWith("$/"))
            {
                string embeddedName = located.Substring(2);
                var embedded = new LoadEmbeddedScript();

                var loadStatus = embedded.Load(L, embeddedName);
                if (loadStatus != LuaStatus.OK)
                    return loadStatus;

                // Execute the chunk
                return L.PCall(0, Lua.MultiRet, 0);
            }

            // 4. Real file on disk
            return OverrideFunctions.PremakeLuaLDoFile(L, located);
        }
    }
}