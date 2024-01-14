using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CT
{

    internal class SyntacticAnalyzer
    {
        private LinkedList<Token> Tokens;
        private LinkedListNode<Token> consumedTk, currentTk;

        void tkerr(Token tk, string Message)
        {
            Resources.tkerr(tk, Message);
        }

        private bool consume(Type Code)
        {
            if (Resources.consume(Code, currentTk))
            {
                consumedTk = currentTk;
                currentTk = currentTk.Next;
                return true;
            }
            return false;
        }

        public SyntacticAnalyzer(LinkedList<Token> Tokens)
        {
            this.Tokens = Tokens;
            currentTk = Tokens.First;
        }

        public bool Unit()
        {
            while (DeclStruct() || DeclFunc() || DeclVar()) { }
            return consume(Type.END);
        }

        bool DeclStruct()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.STRUCT))
            {
                if (consume(Type.ID))
                {
                    if (consume(Type.LACC))
                    {
                        while (DeclVar()) { }

                        if (consume(Type.RACC))
                        {
                            if (consume(Type.SEMICOLON))
                                return true;
                            else tkerr(currentTk.Value, "Missing ';' after struct declaration");
                        }
                        else tkerr(currentTk.Value, "Missing '}' for struct declaration");
                    }
                    else tkerr(currentTk.Value, "Missing '{' for struct declaration");
                }
                else tkerr(currentTk.Value, "Missing ID for struct declaration");
            }
            currentTk = startTk;
            return false;
        }

        bool DeclVar()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (TypeBase() && consume(Type.ID))
            {
                ArrayDecl();
                while (consume(Type.COMMA) && consume(Type.ID))
                {
                    ArrayDecl();
                }
                return consume(Type.SEMICOLON);
            }
            currentTk = startTk;
            return false;
        }

        bool TypeBase()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.INT) || consume(Type.DOUBLE) || consume(Type.CHAR))
            {
                return true;
            }
            else if(consume(Type.STRUCT) && consume(Type.ID))
            {
                    return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ArrayDecl()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.LBRACKET))
            {
                Expr();
                return consume(Type.RBRACKET);
            }
            currentTk = startTk;
            return false;
        }

        bool TypeName()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (TypeBase())
            {
                ArrayDecl();
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool DeclFunc()
        {
            LinkedListNode<Token> startTk = currentTk;
            if ((TypeBase() && (consume(Type.MUL) || true) || consume(Type.VOID)) && consume(Type.ID) && consume(Type.LPAR))
            {
                if (FuncArg())
                {
                    while (consume(Type.COMMA) && FuncArg()) { }
                }

                if (consume(Type.RPAR))
                {
                    if (StmCompound())
                        return true;
                }
                else tkerr(currentTk.Value, "Missing ')' for function declaration");
            }
            currentTk = startTk;
            return false;
        }

        bool FuncArg()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (TypeBase())
            {
                if (consume(Type.ID))
                {
                    ArrayDecl();
                    return true;
                }
                else tkerr(currentTk.Value, "Missing ID for function argument");
            }
            currentTk = startTk;
            return false;
        }

        bool Stm()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (StmCompound() || RuleIF() || RuleWHILE() || RuleFOR() || RuleRETURN())
            {
                return true;
            }
            else if (consume(Type.BREAK) && consume(Type.SEMICOLON))
            {
                return true;
            }
            else
            {
                Expr();
                if (consume(Type.SEMICOLON))
                {
                    return true;
                }
            }
            currentTk = startTk;
            return false;
        }

        bool RuleIF()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.IF))
            {
                if (consume(Type.LPAR))
                {
                    if (Expr())
                    {
                        if (consume(Type.RPAR))
                        {
                            if (Stm())
                            {
                                if (consume(Type.ELSE))
                                {
                                    return Stm();
                                }
                            }
                        }
                        else tkerr(currentTk.Value, "Missing ')' for IF condition");
                    }
                    else tkerr(currentTk.Value, "Invalid expression for IF condition");
                }
                else tkerr(currentTk.Value, "Missing '(' for IF condition");
            }
            currentTk = startTk;
            return false;
        }

        bool RuleWHILE()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.WHILE))
            {
                if (consume(Type.LPAR))
                {
                    if (Expr())
                    {
                        if (consume(Type.RPAR))
                        {
                            if (Stm())
                            {
                                return true;
                            }
                            else tkerr(currentTk.Value, "Missing WHILE statement");
                        }
                        else tkerr(currentTk.Value, "Missing ')' for WHILE condition");
                    }
                    else tkerr(currentTk.Value, "Invalid expression for WHILE condition");
                }
                else tkerr(currentTk.Value, "Missing '(' for WHILE condition");
            }
            currentTk = startTk;
            return false;
        }

        bool RuleFOR()
        {
            LinkedListNode<Token> startTk = currentTk;
            if(consume(Type.FOR))
            {
                if(consume(Type.LPAR))
                {
                    Expr();
                    if(consume(Type.SEMICOLON))
                    {
                        Expr();
                        if(consume(Type.SEMICOLON))
                        {
                            Expr();
                            if (consume(Type.RPAR))
                            {
                                return Stm();
                            }
                            else tkerr(currentTk.Value, "Missing ')' for FOR condition");
                        }
                    }
                }
                else tkerr(currentTk.Value, "Missing '(' for FOR condition");
            }
            currentTk = startTk;
            return false;
        }

        bool RuleRETURN()
        {
            LinkedListNode<Token> startTk = currentTk;
            if(consume(Type.RETURN))
            {
                Expr();
                if(consume(Type.SEMICOLON))
                {
                    return true;
                }
                tkerr(currentTk.Value, "Missing ';' after RETURN statement");
            }
            currentTk = startTk;
            return false;
        }

        bool StmCompound()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.LACC))
            {
                while (DeclVar() || Stm()) { }

                if (consume(Type.RACC))
                {
                    return true;
                }
                else tkerr(currentTk.Value, "Missing closing brace '}'");
            }
            currentTk = startTk;
            return false;
        }

        public bool Expr()
        {
            return ExprAssign();
        }

        bool ExprAssign()
        {
            if (ExprOr() && ExprAssignPrime())
                return true;
            return false;
        }

        bool ExprAssignPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.ASSIGN))
            {
                if (ExprAssign())
                {
                    return true;
                }
                tkerr(currentTk.Value, "Missing expression after assignment operator");
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprOr()
        {
            if (ExprAnd() && ExprOrPrime())
                return true;
            return false;
        }

        bool ExprOrPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.OR))
            {
                if (ExprAnd())
                {
                    if (ExprOrPrime())
                        return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprAnd()
        {
            if (ExprEq() && ExprAndPrime())
                return true;
            return false;
        }

        bool ExprAndPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if(consume(Type.AND))
            {
                if(ExprEq())
                {
                    if (ExprAndPrime())
                        return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprEq()
        {
            if (ExprRel() && ExprEqPrime())
                return true;
            return false;
        }

        bool ExprEqPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.EQUAL) || consume(Type.NOTEQ))
            {
                if (ExprRel())
                {
                    if(ExprEqPrime())
                       return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprRel()
        {
            if (ExprAdd() && ExprRelPrime())
                return true;
            return false;
        }

        bool ExprRelPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.LESS) || consume(Type.LESSEQ) || consume(Type.GREATER) || consume(Type.GREATEREQ))
            {
                if (ExprAdd())
                {
                    if(ExprRelPrime())
                        return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprAdd()
        {
            if (ExprMul() && ExprAddPrime())
                return true;
            return false;
        }

        bool ExprAddPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.ADD) || consume(Type.SUB))
            {
                if (ExprMul())
                {
                    if(ExprAddPrime())
                        return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprMul()
        {
            if (ExprCast() && ExprMulPrime())
                return true;
            return false;
        }

        bool ExprMulPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.MUL) || consume(Type.DIV))
            {
                if (ExprCast())
                {
                    if(ExprMulPrime())
                        return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprCast()
        {
            if (ExprUnary() && ExprCastPrime())
                return true;
            return false;
        }

        bool ExprCastPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.LPAR))
            {
                if (TypeName())
                {
                    if (consume(Type.RPAR))
                    {
                        if (ExprCastPrime())
                            return true;
                    }
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprUnary()
        {
            if (ExprPostfix() && ExprUnaryPrime())
                return true;
            return false;
        }

        bool ExprUnaryPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.SUB) || consume(Type.NOT))
            {
                if (ExprUnaryPrime())
                    return true;
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprPostfix()
        {
            if(ExprPrimary() && ExprPostfixPrime())
                return true;
            return false;
        }

        bool ExprPostfixPrime()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.LBRACKET))
            {
                if (Expr())
                {
                    if (consume(Type.RBRACKET))
                    {
                        if(ExprPostfixPrime())
                            return true;
                    }
                }
            }
            else if (consume(Type.DOT))
            {
                if (consume(Type.ID))
                {
                    if (ExprPostfixPrime())
                        return true;
                }
            }
            else
            {
                return true;
            }
            currentTk = startTk;
            return false;
        }

        bool ExprPrimary()
        {
            LinkedListNode<Token> startTk = currentTk;
            if (consume(Type.ID))
            {
                if(consume(Type.LPAR))
                {
                    if (Expr())
                    {
                        while (consume(Type.COMMA))
                        {
                            Expr();
                        }
                    }
                    if (consume(Type.RPAR))
                    {
                        return true;
                    }
                    tkerr(currentTk.Value, "Missing closing bracket ']'");
                }
                return true;
            }
            else if (consume(Type.CT_INT) || consume(Type.CT_REAL) || consume(Type.CT_CHAR) || consume(Type.CT_STRING))
            {
                return true;
            }
            else if (consume(Type.LPAR))
            {
                if(Expr())
                {
                    if(consume(Type.RPAR))
                        return true;
                }
            }
            currentTk = startTk;
            return false;
        }

    }
}
