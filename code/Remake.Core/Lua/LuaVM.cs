using KeraLua;

namespace Remake
{
    /// <summary>
    /// Singleton class that manages the Lua state and provides a safe execution environment with error handling.
    /// </summary>
    public sealed class LuaVM : IDisposable
    {
        private static readonly Lazy<LuaVM> _instance = new Lazy<LuaVM>(() => new LuaVM());
        public static LuaVM Instance => _instance.Value;

        public Lua State { get; }
        public LuaBinder Binder { get; }

        // Store the stack index of the error handler
        private int _errorHandlerIndex;

        public LuaVM()
        {
            State = new Lua();
            Binder = new LuaBinder(State);
            State.OpenLibs();
        }

        public void Dispose()
        {
            // Unref the error handler to prevent memory leaks
            State.Unref(LuaRegistry.Index, _errorHandlerIndex);
            State?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}