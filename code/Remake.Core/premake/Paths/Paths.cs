using KeraLua;
using System.Runtime.InteropServices;
using System.Text;

namespace Remake.Modules
{
    [LuaModule("path")]
    internal partial class PathFunctions
    {
        /// <summary>
        /// Defines the join modes for path joining operations.
        /// </summary>
        public enum JoinMode
        {
            Relative = 0,
            Absolute = 1,
            MaybeAbsolute = 2
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string EnsureTrailingSlash(string path)
        {
            return path.EndsWith("/") ? path : path + "/";
        }

        /// <summary>
        /// P/Invoke declaration for GetModuleFileNameW, which retrieves the full path of the executable.
        /// </summary>
        /// <param name="hModule"></param>
        /// <param name="lpFilename"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint GetModuleFileNameW(IntPtr hModule, StringBuilder lpFilename, int nSize);

        public static string LocateExecutable()
        {
            var buffer = new StringBuilder(1024);
            uint len = GetModuleFileNameW(IntPtr.Zero, buffer, buffer.Capacity);

            string path;

            if (len > 0)
            {
                path = buffer.ToString();
            }
            else
            {
                // fallback to argv[0]
                path = Environment.GetCommandLineArgs()[0];
            }

            // If still relative, make absolute using the same base directory
            // Premake uses everywhere else (the initial working directory),
            // not the current process directory which may have been changed.
            if (!System.IO.Path.IsPathRooted(path))
                path = System.IO.Path.Combine(OSFunctions.do_getcwd(), path);

            // Normalize using host-level getabsolute
            return DoGetAbsolute(path, null);
        }
    }
}