/*using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CT.DomainAnalyzer;

namespace CT
{
    internal class DomainAnalyzer
    {
        private LinkedList<Token> Tokens;
        private LinkedListNode<Token> consumedTk, currentTk;
        private SyntacticAnalyzer SyntacticAnalyzer;

        enum TypeBase { TB_INT, TB_DOUBLE, TB_CHAR, TB_STRUCT, TB_VOID };
        enum Class { CLS_VAR, CLS_FUNC, CLS_EXTFUNC, CLS_STRUCT };
        enum Visibility { MEM_GLOBAL, MEM_ARG, MEM_LOCAL };

        private class Type
        {
            public int TypeBase;
            Symbol S;
            public int nElements;
        }

        public class Symbol
        {
            public string Name;
            public int Class;
            int Visibility;
            Type Type;
            public int Depth;
            List<Symbol> Args;
            List<Symbol> Members;
        }

        public class Symbols
        {
            Symbol Begin;
            Symbol End;
            List<Symbol> SymbolsList;

            public Symbols() { initSymbols(this); }

            void initSymbols(Symbols symbols)
            {
                symbols.Begin = null;
                symbols.End = null;
                SymbolsList = new List<Symbol> ();
            }

            public void AddSymbol(Symbol s)
            {
                this.SymbolsList.Add(s);
            }

            public List<Symbol> GetSymbols()
            {
                return SymbolsList;
            }
        }

        public DomainAnalyzer(SyntacticAnalyzer sa)
        {
            SyntacticAnalyzer Analyzer = sa;
        }

        private bool consume(CT.Type Code)
        {
            if (currentTk != null && currentTk.Value.Code == Code)
            {
                consumedTk = currentTk;
                currentTk = currentTk.Next;
                return true;
            }
            return false;
        }


        Symbol AddSymbol(Symbols Symbols, string Name, int Class)
        {
            Symbol s = new Symbol();
            s.Name = Name;
            s.Class = Class;
            Symbols.AddSymbol(s);
            return s;
        }

        Symbol FindSymbol(Symbols Symbols, string Name)
        {
            List<Symbol> SymbolsList = Symbols.GetSymbols();
            Symbol Symbol = null;

            for (int i = SymbolsList.Count - 1; i >= 0; i--)
            {
                if (SymbolsList[i].Name == Name)
                {
                    Symbol = SymbolsList[i];
                    break;
                }
            }
            return Symbol;
        }

        bool arrayDecl(Type ret)
        {
            if (consume(CT.Type.LBRACKET))
            {
                if(SyntacticAnalyzer.Expr())
                {
                    ret.nElements = 0;
                }

                if (consume(CT.Type.RBRACKET))
                {
                    return true;
                }
                else Resources.tkerr(new Token(), "Missing']'");
            }
            return false;
        }

        bool typeBase(Type ret)
        {
            if (consume(CT.Type.INT))
            {
                ret.TypeBase = 0;
            }
            else if (consume(CT.Type.DOUBLE))
            {
                ret.TypeBase = 1;
            }
            else if (consume(CT.Type.CHAR))
            {
                ret.TypeBase = 2;
            }
            else if (consume(CT.Type.STRUCT))
            {
                Token tkName = consume(CT.Type.ID);
                if (tkName == NULL)
                {
                    tkerr(crtTk, "Missing struct identifier");
                }

                Symbol* s = findSymbol(&symbols, tkName->text);
                if (s == NULL)
                {
                    tkerr(crtTk, "Undefined symbol: %s", tkName->text);
                }
                if (s->cls != CLS_STRUCT)
                {
                    tkerr(crtTk, "%s is not a struct", tkName->text);
                }

                ret->typeBase = TB_STRUCT;
                ret->s = s;
            }
            else
            {
                return false;
            }
            return true;
        }


    }
}
*/