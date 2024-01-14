using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace CT
{
    enum Type
    {
        ID, BREAK, CHAR, DOUBLE, ELSE, FOR, IF, INT, RETURN, STRUCT, VOID, WHILE, CT_INT,
        EXP, CT_REAL, ESC, CT_CHAR, CT_STRING, COMMA, SEMICOLON, LPAR, RPAR, LBRACKET,
        RBRACKET, LACC, RACC, ADD, SUB, MUL, DIV, DOT, AND, OR, NOT, ASSIGN, EQUAL, NOTEQ,
        LESS, LESSEQ, GREATER, GREATEREQ, SPACE, LINECOMMENT, COMMENT, END
    }

    struct Token
    {
        public CT.Type Code;
        int Line;
        string Text;
        long i;
        double r;
        static List<CT.Type> StringValueTypes = new List<CT.Type>() { Type.ID, Type.BREAK, Type.CHAR, Type.DOUBLE, Type.ELSE, Type.FOR, Type.IF, Type.INT, Type.RETURN,
                Type.STRUCT, Type.VOID, Type.WHILE, Type.CT_STRING, Type.COMMA, Type.SEMICOLON, Type.LPAR, Type.RPAR, Type.LBRACKET,
                Type.RBRACKET, Type.LACC, Type.RACC, Type.ADD, Type.SUB, Type.MUL, Type.DIV, Type.DOT, Type.AND, Type.OR,
                Type.NOT, Type.ASSIGN, Type.EQUAL, Type.NOTEQ, Type.LESS, Type.LESSEQ, Type.GREATER, Type.GREATEREQ, Type.SPACE, Type.LINECOMMENT, Type.COMMENT };

        public Token() { }

        public Token(CT.Type Code, int Line, string Value)
        {
            this.Code = Code;
            this.Line = Line;
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            if (StringValueTypes.Contains(Code))
                this.Text = Value;
            else if (Code == Type.CT_INT)
            {
                bool octalFlag = false;
                if (Value.Length > 1 && (Value.Substring(0, 2) == "0x"))
                    this.i = Convert.ToInt64(Value, 16);
                else if (Value.Length > 1 && Value[0] == '0' && !Value.Contains('.'))
                {
                    foreach (char c in Value)
                    {
                        if (Value[0] == '0' && (c >= '0' && c <= '7'))
                            octalFlag = true;
                        else
                        {
                            octalFlag = false;
                            break;
                        }
                    }
                    if (octalFlag)
                    {
                        this.i = Convert.ToInt64(Value, 8);
                        octalFlag = false;
                    }
                    else
                        this.i = Convert.ToInt64(Value);
                }
                else
                    this.i = Convert.ToInt64(Value);
            }
            else if (Code == Type.CT_CHAR)
            {
                if (Value.Length == 3)
                    this.i = Value[1];
                else if (Value.Length == 4)
                {
                    if (Value[2] == '\'' || Value[2] == '\"' || Value[2] == '\\')
                        this.i = Value[2];
                    else if (Value[2] == 'a')
                        this.i = 7;
                    else if (Value[2] == 'b')
                        this.i = 8;
                    else if (Value[2] == 'f')
                        this.i = 12;
                    else if (Value[2] == 'n')
                        this.i = 10;
                    else if (Value[2] == 'r')
                        this.i = 13;
                    else if (Value[2] == 't')
                        this.i = 9;
                    else if (Value[2] == 'v')
                        this.i = 11;
                }
            }
            else if (Code == Type.CT_REAL)
            {
                if (Value[0] == '.')
                    Value = '0' + Value;
                if (Value.Contains('e') || Value.Contains('E'))
                {
                    string number = "";
                    double power = 0;
                    int expStart = 0;
                    foreach (char c in Value)
                    {
                        if (Char.IsLetter(c))
                        {
                            expStart = Value.IndexOf(c);
                            break;
                        }
                        else
                            number += c;
                    }
                    for (int index = expStart; index < Value.Length; index++)
                        if (Char.IsDigit(Value[index]))
                        {
                            if (power == 0)
                                power += Char.GetNumericValue(Value[index]);
                            else power = power * 10 + Char.GetNumericValue(Value[index]);
                        }
                    if (Value.Contains('-'))
                    {
                        this.r = (double)(Convert.ToDouble(number, provider) / Math.Pow(10, power));
                    }
                    else this.r = (double)(Convert.ToDouble(number, provider) * Math.Pow(10, power));
                }
                else
                    this.r = (double) Convert.ToDouble(Value, provider);
            }
                
        }

        public override string ToString()
        {
            string str = Code + " " + Line + " ";
            if (StringValueTypes.Contains(Code))
                str += Text;
            else if (Code == Type.CT_INT || Code == Type.CT_CHAR)
                str += i;
            else
                str += r;
            return str;
        }
    }

    internal class LexicalAnalyzer
    {
        private List<Token> TokensList;
        private StreamReader SourceFile;
        private List<Char> OperatorsChars = new List<Char>() { '+', '|', '=', '<', '>' };
        private List<Char> EscChars = new List<Char>() { 'a', 'b', 'f', 'n', 'r', 't', 'v', '\'', '?', '\"', '\\', '0' };
        private int commented_newlines = 0;

        public LexicalAnalyzer(StreamReader SourceFile)
        {
            TokensList = new List<Token>();
            if (SourceFile.Peek() >= 0)
            {
                this.SourceFile = SourceFile;
                GetTokens();
            }
        }

        private void GetTokens()
        {
            if (SourceFile.Peek() >= 0)
            {
                int state = 0;
                string value = "";
                int line = 1;
                char ch = (char)SourceFile.Read();

                while (!SourceFile.EndOfStream)
                {
                    switch (state)
                    {
                        case 0:
                            if ((Char.IsPunctuation(ch) && ch != '_' && ch != '\'' && ch != '\"' && ch != '\\' && ch != '/') ||
                                OperatorsChars.Contains(ch))
                                goto case 1;
                            else if (Char.IsLetter(ch) || ch == '_')
                                goto case 3;
                            else if (Char.IsDigit(ch) || ch == '.')
                                goto case 4;
                            else if (ch == '\'')
                                goto case 6;
                            else if (ch == '\"')
                                goto case 7;
                            else if (ch == '\n' || ch == '\r' || ch == '\t')
                                goto case 8;
                            else if (ch == '/')
                                goto case 9;
                            else goto case 10;
                        case 1:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (value == ",")
                                TokensList.Add(new Token(Type.COMMA, line, value));
                            else if (value == ";")
                                TokensList.Add(new Token(Type.SEMICOLON, line, value));
                            else if (value == "(")
                                TokensList.Add(new Token(Type.LPAR, line, value));
                            else if (value == ")")
                                TokensList.Add(new Token(Type.RPAR, line, value));
                            else if (value == "[")
                                TokensList.Add(new Token(Type.LBRACKET, line, value));
                            else if (value == "]")
                                TokensList.Add(new Token(Type.RBRACKET, line, value));
                            else if (value == "{")
                                TokensList.Add(new Token(Type.LACC, line, value));
                            else if (value == "}")
                                TokensList.Add(new Token(Type.RACC, line, value));
                            else if (value == "+")
                                TokensList.Add(new Token(Type.ADD, line, value));
                            else if (value == "-")
                                TokensList.Add(new Token(Type.SUB, line, value));
                            else if (value == "*")
                                TokensList.Add(new Token(Type.MUL, line, value));
                            else if (value == "/")
                                TokensList.Add(new Token(Type.DIV, line, value));
                            else if (value == ".")
                            {
                                if (Char.IsDigit((char)SourceFile.Peek()))
                                    goto case 4;
                                else TokensList.Add(new Token(Type.DOT, line, value));
                            }
                            else if (value == "&" && ch == '&')
                            {
                                value += ch;
                                TokensList.Add(new Token(Type.AND, line, value));
                                ch = (char)SourceFile.Read();
                            }
                            else if (value == "|" && ch == '|')
                            {
                                value += ch;
                                TokensList.Add(new Token(Type.OR, line, value));
                                ch = (char)SourceFile.Read();
                            }
                            else if (value == "!" && ch != '=')
                                TokensList.Add(new Token(Type.NOT, line, value));
                            else if (value == "=" && ch != '=')
                                TokensList.Add(new Token(Type.ASSIGN, line, value));
                            else if (value == "=" && ch == '=')
                            {
                                value += ch;
                                TokensList.Add(new Token(Type.EQUAL, line, value));
                                ch = (char)SourceFile.Read();
                            }
                            else if (value == "!" && ch == '=')
                            {
                                value += ch;
                                TokensList.Add(new Token(Type.NOTEQ, line, value));
                                ch = (char)SourceFile.Read();
                            }
                            else if (value == "<" && ch != '=')
                                TokensList.Add(new Token(Type.LESS, line, value));
                            else if (value == "<" && ch == '=')
                            {
                                value += ch;
                                TokensList.Add(new Token(Type.LESSEQ, line, value));
                                ch = (char)SourceFile.Read();
                            }
                            else if (value == ">" && ch != '=')
                                TokensList.Add(new Token(Type.GREATER, line, value));
                            else if (value == ">" && ch == '=')
                            {
                                value += ch;
                                TokensList.Add(new Token(Type.GREATEREQ, line, value));
                                ch = (char)SourceFile.Read();
                            }
                            else if (ch != ' ')
                                goto case 10;
                            value = "";
                            goto case 0;
                        case 2:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if ((Char.ToLower(value[value.Length-1]) == 'e' && (ch == '-' || ch == '+')) || (ch >= '0' && ch <= '9'))
                                goto case 2;
                            else goto case 22;
                        case 22:
                            if (value.Length > 1 && Char.ToLower(value[0]) != 'e')
                                TokensList.Add(new Token(Type.CT_REAL, line, value));
                            value = "";
                            break;
                        case 3:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (Char.IsLetterOrDigit(ch) || ch == '_')
                                goto case 3;
                            else
                            {
                                if (value == "break")
                                    TokensList.Add(new Token(Type.BREAK, line, value));
                                else if (value == "char")
                                    TokensList.Add(new Token(Type.CHAR, line, value));
                                else if (value == "double")
                                    TokensList.Add(new Token(Type.DOUBLE, line, value));
                                else if (value == "else")
                                    TokensList.Add(new Token(Type.ELSE, line, value));
                                else if (value == "for")
                                    TokensList.Add(new Token(Type.FOR, line, value));
                                else if (value == "if")
                                    TokensList.Add(new Token(Type.IF, line, value));
                                else if (value == "int")
                                    TokensList.Add(new Token(Type.INT, line, value));
                                else if (value == "return")
                                    TokensList.Add(new Token(Type.RETURN, line, value));
                                else if (value == "struct")
                                    TokensList.Add(new Token(Type.STRUCT, line, value));
                                else if (value == "void")
                                    TokensList.Add(new Token(Type.VOID, line, value));
                                else if (value == "while")
                                    TokensList.Add(new Token(Type.WHILE, line, value));
                                else
                                    TokensList.Add(new Token(Type.ID, line, value));
                                value = "";
                                break;
                            }
                        case 4:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if ((value[0] == '0' && ch == 'x') ||
                                (value.Length > 1 && (value.Substring(0, 2) == "0x") && Char.IsLetterOrDigit(ch)) ||
                                Char.IsDigit(ch) || (ch == '.' && !value.Contains('.')))
                                goto case 4;
                            else if (Char.ToLower(ch) == 'e')
                                goto case 2;
                            else
                            {
                                if (value.Contains('.'))
                                    TokensList.Add(new Token(Type.CT_REAL, line, value));
                                else if (!(value.Length == 2 && (value.Substring(0, 2) == "0x")))
                                    TokensList.Add(new Token(Type.CT_INT, line, value));
                                value = "";
                                break;
                            }
                        case 5:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (value.Length == 3 && ch == '\'')
                                goto case 6;
                            else if ((value[0] == '\"' && ch == '\"') || (!EscChars.Contains(ch) && ch != '\r'))
                                goto case 7;
                            else if ((EscChars.Contains(ch) && value[0] == '\'' && value.Length == 2) ||
                                (EscChars.Contains(ch) && value[0] == '\\' && value.Length == 1) ||
                                (EscChars.Contains(ch) && value[0] == '\"'))
                                goto case 5;
                            value = "";
                            break;
                        case 6:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if ((value.Length < 2 && ch != '\'' && ch != '\\') || (value.Length == 2 && ch == '\''))
                                goto case 6;
                            else if (value.Length == 1 && ch == '\\')
                                goto case 5;
                            else
                                TokensList.Add(new Token(Type.CT_CHAR, line, value));
                            value = "";
                            break;
                        case 7:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (ch == '\\')
                                goto case 5;
                            else if (value.Length > 1 && value.Last() == '\"')
                                TokensList.Add(new Token(Type.CT_STRING, line, value));
                            else if (value[0] == '\"' && ch != '\'')
                                goto case 7;
                            value = "";
                            break;
                        case 8:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (value == "\n")
                            {
                                line++;
                            }
                            value = "";
                            break;
                        case 9:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (value == "/" && ch == '*')
                                goto case 99;
                            else if (value == "/" && ch == '/' || value.Length > 1 && ch != '\n' && ch != '\uffff' && ch != '\r' && ch != '\0')
                                goto case 9;
                            else if(value == "/") 
                                TokensList.Add(new Token(Type.DIV, line, value));
                            value = "";
                            break;
                        case 99:
                            value += ch;
                            ch = (char)SourceFile.Read();
                            if (ch == '\n')
                                commented_newlines++;
                            if (ch == '/' && value.Length > 2 && value[value.Length - 1] == '*')
                            {
                                value += ch;
                                ch = (char)SourceFile.Read();
                                line += commented_newlines;
                                commented_newlines = 0;
                            }
                            else goto case 99;
                            value = "";
                            break;
                        case 10:
                            ch = (char)SourceFile.Read();
                            if (ch == '\uffff')
                                break;
                            goto case 0;
                    }
                }
            }
        }

        public LinkedList<Token> GetTokenList()
        {
            LinkedList<Token> tokens = new(TokensList);
            tokens.AddLast(new LinkedListNode<Token>(new Token(Type.END, 0, "")));
            return tokens;
        }
    }
}
