using Remake;
using System;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("getnumcpus", "Return the number of logical CPU cores.")]
        public static int GetNumCpus()
        {
            return Environment.ProcessorCount;
        }
    }
}