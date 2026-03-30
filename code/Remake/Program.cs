using KeraLua;
using System.Reflection.Metadata.Ecma335;

namespace Remake
{
    internal static class Program
    {
        public static string PremakeRootDirectory = AppContext.BaseDirectory;

        public static int Main(string[] args)
        {
            Directory.SetCurrentDirectory(PremakeRootDirectory);

            string path = System.IO.Directory.GetCurrentDirectory();
            LuaVM vm = new LuaVM();

            Console.WriteLine($"Premake {BuildInfo.PREMAKE_VERSION}");

            var host = new PremakeHost();  
            PremakeRuntime.Instance.CheckResult(host.Init());

            if (PremakeRuntime.Result.ExitCode != 0)
            {
                PremakeRuntime.SetResultMessage("Failed to initialize PremakeHost");
                return PremakeRuntime.Result.ExitCode;
            }
            else
            {
                PremakeRuntime.SetResultMessage("Successfully initialized PremakeHost");
                PremakeRuntime.Instance.CheckResult(host.Execute(args, "src/_remake_main.lua"));
            }
            return PremakeRuntime.Result.ExitCode;
        }
    }
}