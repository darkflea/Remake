using Remake;
using System;
using System.Text;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("getpass", "Prompt the user for a password without echoing input.")]
        public static string GetPass(string prompt)
        {
            Console.Write(prompt);

            StringBuilder password = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                        password.Remove(password.Length - 1, 1);
                }
                else if (key.KeyChar != '\u0000')
                {
                    password.Append(key.KeyChar);
                }
            }

            return password.ToString();
        }
    }
}