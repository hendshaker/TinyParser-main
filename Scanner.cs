using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Token_Class
{
    Number, Idenifier,
    Int, Float, String,
    Read, Write,
    Repeat, Until,
    If, ElseIf, Else, Then, main,
    Return, Endl, Dot, Semicolon, Comma, LParanthesis,
    RParanthesis, BooleanOp, LessThanOp, GreaterThanOp, NotEqualOp,
    PlusOp, MultiplyOp, DivideOp, MinusOp, DoubleQoute, End,
    LeftBraces, RightBraces,
    Assignment, AndOp, OR_Opeartor
}

namespace TinyParser
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;
        public Token() { }
        public Token(string lex, Token_Class token_type)
        {
            this.lex = lex;
            this.token_type = token_type;
        }
    }
    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {

            ReservedWords.Add("idenifier", Token_Class.Idenifier);
            ReservedWords.Add("number", Token_Class.Number);

            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);

            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);

            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("main", Token_Class.main);


            Operators.Add(".", Token_Class.Dot);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);

            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LeftBraces);
            Operators.Add("}", Token_Class.RightBraces);

            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OR_Opeartor);

            Operators.Add(":=", Token_Class.Assignment);
            Operators.Add("=", Token_Class.BooleanOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
        }
        public void StartScanning(string SourceCode)
        {

            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                bool ItwasAcomment = false;
                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar == '\t')
                    continue;

                if (CurrentChar == '/' && SourceCode[i + 1] == '*')
                {
                    j = i + 1;
                    while (j < SourceCode.Length)
                    {
                        if (SourceCode[j] == '*' && SourceCode[j + 1] == '/')
                        {
                            ItwasAcomment = true;
                            break;
                        }
                        j++;
                    }
                    if (ItwasAcomment)
                    {
                        i = j + 1; // 1 beacause SourceCode[j] == '*' and SourceCode[j+1] == '/' so the first charachter after this is in j+2
                        continue;
                    }

                    if (!ItwasAcomment && j == SourceCode.Length)
                    {
                        Errors.Error_List.Add("Unclosed COMMENT");
                        //Tokens.Clear();
                        i = j;
                    }
                }

                else if (CurrentChar == '\"')//Srting
                {
                    j++;
                    while (j <= SourceCode.Length - 1)
                    {
                        if (SourceCode[j] == '\"')
                        {
                            CurrentLexeme += SourceCode[j];
                            break;
                        }
                        CurrentLexeme += SourceCode[j];
                        j++;

                    }
                    i = j;
                    FindTokenClass(CurrentLexeme);
                }
                /************************************** character *********************************/
                else if (CurrentChar >= 'A' && CurrentChar <= 'z')
                {
                    j++;
                    while (j <= SourceCode.Length - 1)
                    {
                        if (/*SourceCode[j] != ',' &&*/ SourceCode[j] != '.' && (Operators.ContainsKey(SourceCode[j].ToString()) || SourceCode[j] == '\n' || SourceCode[j] == ' ' || SourceCode[j] == '\r'))
                            break;
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }
                /************************************** Numbers *********************************/
                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    j++;
                    while (j <= SourceCode.Length - 1)
                    {
                        if (SourceCode[j] != '.' && (Operators.ContainsKey(SourceCode[j].ToString()) || SourceCode[j] == '\n' || SourceCode[j] == ' ' || SourceCode[j] == '\r'))
                            break;

                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }
                else if ((CurrentChar == ':' && SourceCode[j + 1] == '=') || (CurrentChar == '<' && SourceCode[j + 1] == '>'))
                {
                    CurrentLexeme = CurrentChar.ToString() + SourceCode[j + 1];
                    i = j + 2;
                    FindTokenClass(CurrentLexeme);
                }
                else if (CurrentChar == '/' || CurrentChar == '+' || CurrentChar == '-' || CurrentChar == '*' || CurrentChar == '.' ||
                         CurrentChar == ';' || CurrentChar == ',' || CurrentChar == '(' || CurrentChar == ')' || CurrentChar == '<' ||
                         CurrentChar == '>' || CurrentChar == '{' || CurrentChar == '}' || CurrentChar == '=')
                {
                    FindTokenClass(CurrentChar.ToString());
                }
                else if (CurrentChar == '&' || CurrentChar == '|')
                {
                    j++;
                    while (j <= SourceCode.Length - 1)
                    {
                        if (SourceCode[j] != '&' && SourceCode[j] != '|' && (Operators.ContainsKey(SourceCode[j].ToString()) || SourceCode[j] == '\n' || SourceCode[j] == ' ' || SourceCode[j] == '\r'))
                            break;
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j;
                    FindTokenClass(CurrentLexeme);
                }
                else
                {
                    j++;
                    while (j <= SourceCode.Length - 1)
                    {
                        if (SourceCode[j] == '\n' || SourceCode[j] == ' ' || SourceCode[j] == '\r')
                            break;
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j;
                    FindTokenClass(CurrentLexeme);
                }
            }
            Tiny_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //if (!isDublicatedLexem(Lex))
            //{
            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }
            // Is it an operator
            else if (Operators.ContainsKey(Lex) && Lex[0] != '&' && Lex[0] != '|')
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            //Is it an identifier?
            else if (Lex[0] >= 'A' && Lex[0] <= 'z')
            {
                if (isIdentifier(Lex))
                {
                    Tok.token_type = ReservedWords["idenifier"];
                    Tokens.Add(Tok);
                }
                else
                {
                    Errors.Error_List.Add(Lex);
                    Errors.Error_List.Add("Identifier must start with letter then any combination of letters and digits");
                    Errors.Error_List.Add("--------------------------------------------------------------------------------------------");
                }
            }
            else if (Lex[0] >= '0' && Lex[0] <= '9')
            {
                if (isNumber(Lex))
                {
                    Tok.token_type = ReservedWords["number"];
                    Tokens.Add(Tok);
                }
                else
                {
                    Errors.Error_List.Add(Lex);
                    Errors.Error_List.Add("Wrong Number because it contains character or special character or many dots");
                    Errors.Error_List.Add("--------------------------------------------------------------------------------------------");
                }

            }

            //Is it a string ?
            
            else if (Lex[0] == '\"')
            {
                bool line = false;
                if (Lex[Lex.Length - 1] == '\"')
                {
                    for (int i = 1; i < Lex.Length - 2; i++)
                    {
                        if (Lex[i] == '\n')
                        {
                            line = true;
                            break;
                        }
                    }
                    if (!line)
                    {
                        Tok.token_type = ReservedWords["string"];
                        Tokens.Add(Tok);
                    }
                    else
                    {
                        Errors.Error_List.Add(Lex);
                        Errors.Error_List.Add("String must be in the same line");
                        Errors.Error_List.Add("--------------------------------------------------------------------------------------------");

                    }
                }
                else
                {
                    Errors.Error_List.Add(Lex);
                    Errors.Error_List.Add("Missing double qoutes in string");
                    Errors.Error_List.Add("--------------------------------------------------------------------------------------------");

                }
            }
            else if (Lex[0] == '&' && Lex.Length == 2)
            {
                if (Lex[1] == '&')
                {
                    Tok.token_type = Operators["&&"];
                    Tokens.Add(Tok);
                }
                else
                {
                    Errors.Error_List.Add(Lex);
                    Errors.Error_List.Add("Wrong Boolean Operator");
                    Errors.Error_List.Add("--------------------------------------------------------------------------------------------");
                }
            }
            else if (Lex[0] == '|' && Lex.Length == 2)
            {
                if (Lex[1] == '|')
                {
                    Tok.token_type = Operators["||"];
                    Tokens.Add(Tok);
                }
                else
                {
                    Errors.Error_List.Add(Lex);
                    Errors.Error_List.Add("Wrong Boolean Operator");
                    Errors.Error_List.Add("--------------------------------------------------------------------------------------------");
                }
            }
            else if ((Lex[0] == '|' || Lex[0] == '&') && (Lex.Length == 1 || Lex.Length > 2))
            {
                Errors.Error_List.Add(Lex);
                Errors.Error_List.Add("Wrong Boolean Operator");
                Errors.Error_List.Add("--------------------------------------------------------------------------------------------");
            }
            else
            {
                Errors.Error_List.Add(Lex);
                Errors.Error_List.Add("Unknown");
                Errors.Error_List.Add("--------------------------------------------------------------------------------------------");
            }

            //}

        }
        //bool isDublicatedLexem(string Lex)
        //{
        //    bool Duplicated = false;
        //    for (int i = 0; i < Tokens.Count; i++)
        //    {
        //        if (Tokens[i].lex == Lex)
        //        {
        //            Duplicated = true;
        //            break;
        //        }
        //    }
        //    return Duplicated;
        //}
        bool isIdentifier(string lex)
        {

            bool isValid = true;
            if (lex[0] >= 'A' && lex[0] <= 'z')
            {
                for (int i = 1; i < lex.Length; i++)
                {
                    if (lex[i] == '^' || lex[i] == '_' || lex[i] == '.' || lex[i] == ',')
                    {
                        isValid = false;
                        break;
                    }
                    else if (lex[i] >= 'A' && lex[i] <= 'z' || lex[i] >= '0' && lex[i] <= '9')
                    {
                        continue;
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            else
                isValid = false;
            return isValid;
        }
        bool isNumber(string lex)
        {
            bool isValid = true;
            bool isFirstPoint = false;
            for (int i = 0; i < lex.Length; i++)
            {
                if (lex[i] >= '0' && lex[i] <= '9')
                {
                    continue;
                }
                else if (lex[i] == '.')
                {
                    if (!isFirstPoint)
                        isFirstPoint = true;
                    else
                    {
                        isValid = false;
                        break;
                    }

                }
                else
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }
        void RemoveComments()
        {

        }
    }
}
