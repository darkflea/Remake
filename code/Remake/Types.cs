using KeraLua;

namespace Remake
{
    /// <summary>
    /// Represents the result of a Premake execution, including status, exit code, error information, and diagnostic
    /// details.
    /// </summary>
    /// <remarks>This class encapsulates the outcome of running Premake, providing information about the Lua
    /// execution status, process exit code, error messages, and various diagnostic flags. It is typically used to
    /// inspect the results of a Premake run and to aid in troubleshooting or reporting errors.</remarks>
    public class PremakeResult
    {
        public LuaStatus LuaStatus { get; set; } = LuaStatus.OK;
        public int ExitCode { get; set; } = 0;
        public string? ErrorMessage { get; set; } = null;

        // Optional but extremely useful for diagnostics:
        public string[] Arguments { get; set; } = Array.Empty<string>();
        public bool HelpIntercepted { get; set; }
        public bool RanPremakeMain { get; set; }
        public bool RanUserScripts { get; set; }
        public string? LastLuaError { get; set; }
        public string? LastLuaTraceback { get; set; }
    }

    /// <summary>
    /// Represents errors that occur during the execution of a Premake process.
    /// </summary>
    /// <remarks>This exception is thrown when a Premake operation fails and provides access to the result
    /// details, including status, exit code, and error message. Use the Result property to inspect the specific outcome
    /// of the failed Premake execution.</remarks>
    public class PremakeException : Exception
    {
        public PremakeResult Result { get; }
        public PremakeException(PremakeResult result)
            : base($"Premake execution failed with status: {result.LuaStatus}, exit code: {result.ExitCode}, error message: {result.ErrorMessage}")
        {
            Result = result;
        }
    }

    /// <summary>
    /// Provides a singleton runtime environment for managing Premake execution state and results.
    /// </summary>
    /// <remarks>This class centralizes access to the current Premake execution result and related runtime
    /// operations. It is designed as a singleton; use the static Instance property to access the single instance.
    /// Thread safety is not guaranteed.</remarks>
    public class PremakeRuntime
    {
        private static readonly PremakeRuntime _instance = new PremakeRuntime();
        private PremakeResult m_result;

        public static PremakeRuntime Instance
        {
            get { return _instance; }
        }

        public static PremakeResult Result
        {
            get { return Instance.m_result; }
            set { Instance.m_result = value; }
        }

        public static void SetResultMessage(string message)
        {
            var result = Instance.m_result;
            result.ErrorMessage = message;
            Instance.m_result = result;
        }

        private PremakeRuntime()
        {
            m_result = new PremakeResult
            {
                LuaStatus = 0,
                ExitCode = 0,
                ErrorMessage = "Nothing to report"
            };
        }

        public void CheckResult(PremakeResult result)
        {
            if (result.LuaStatus != LuaStatus.OK)
            {
                throw new PremakeException(result);
            }

            m_result = result;
        }
    }

    public struct BuildinMapping
    {
        public string Name;      // logical script name (e.g. "xcode/xcode.lua")
        public byte[] Bytecode;  // embedded stripped/bytecode data
        public int Length;       // cached length for convenience

        public BuildinMapping(string name, byte[] bytecode)
        {
            Name = name;
            Bytecode = bytecode;
            Length = bytecode.Length;
        }
    }
}