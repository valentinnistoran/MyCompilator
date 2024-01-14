using System;
using System.Collections.Generic;
using System.IO;

namespace CT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader(@"C:\Users\VALI\Desktop\tests\5.c");
            LexicalAnalyzer la = new LexicalAnalyzer(sr);
            foreach(Token t in la.GetTokenList())
            {
                Console.WriteLine(t.ToString());
            }
            SyntacticAnalyzer sa = new SyntacticAnalyzer(new LinkedList<Token>(la.GetTokenList()));
            sa.Unit();
            //DomainAnalyzer da = new DomainAnalyzer(sa);
            Console.ReadKey();
        }
    }
}
