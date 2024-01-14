using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CT
{
    internal static class Resources
    {
        public static bool consume(Type Code, LinkedListNode<Token> currentTk)
        {
            if (currentTk != null && currentTk.Value.Code == Code)
            {
                return true;
            }
            return false;
        }

        public static void tkerr(Token tk, string Message)
        {
            Console.WriteLine(tk.ToString() + " " + Message);
        }

    }
}
