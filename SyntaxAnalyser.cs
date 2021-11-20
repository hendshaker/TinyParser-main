using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace TinyParser
{
    public class Node
    {
        public List<Node> children = new List<Node>();
        public string Name;
        public Node() { }
        public Node(string Name)
        {
            this.Name = Name;
        }
    }
    class SyntaxAnalyser
    {
        public static List<String> tempParameters = new List<String>();
        public static Dictionary<string, int> FunctionsData = new Dictionary<string, int>();

        static int TokenIndex = 0;
        static List<Token> TokenStream;
        public static Node root;
        public static Node Parse(List<Token> Tokens)
        {
            TokenIndex = 0;
            TokenStream = Tokens;

            root = Program();

            return root;
        }

        //------------ Hajer Start --------------
        private static Node Condition_Operator()
        {
            Node node = new Node("Condition_Operator");

            if (TokenStream[TokenIndex].token_type == Token_Class.LessThanOp)
            {
                node.children.Add(match(Token_Class.LessThanOp));
                return node;
            }
            else if (TokenStream[TokenIndex].token_type == Token_Class.GreaterThanOp)
            {
                node.children.Add(match(Token_Class.GreaterThanOp));
                return node;
            }
            else if (TokenStream[TokenIndex].token_type == Token_Class.NotEqualOp)
            {
                node.children.Add(match(Token_Class.NotEqualOp));
                return node;
            }
            else if (TokenStream[TokenIndex].token_type == Token_Class.BooleanOp)
            {
                node.children.Add(match(Token_Class.BooleanOp));
                return node;

            }

            //if error
            return null;
        }

        public static Node Declaration_Statement()
        {
            Node node = new Node("Declaration_Statement");
            try
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Float || TokenStream[TokenIndex].token_type == Token_Class.Int || TokenStream[TokenIndex].token_type == Token_Class.String)
                {
                    node.children.Add(DataType());
                }
                else
                {
                    Errors.ParserError_List.Add("Datatype Missing in Declaration Statement");
                }
                if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                {
                    node.children.Add(match(Token_Class.Idenifier));
                    node.children.Add(DeclareRest2());
                    node.children.Add(DeclareRest1());
                    // x := 2 + 1;                   
                }

                else
                {
                    Errors.ParserError_List.Add("Identifier Missing in Declaration Statement");
                    if (TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                    {
                        TokenIndex++;
                    }
                }
                if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon )
                {
                    node.children.Add(match(Token_Class.Semicolon));
                }
                else 
                {
                    Errors.ParserError_List.Add("Semicolon Missing in Declaration Statement");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Errors.ParserError_List.Add("Semicolon Missing");
            }

            return node;
        }
        private static Node DeclareRest1()
        {
            List<string> Error = new List<string>();
            int count = 0;
            
            Node node = new Node("DeclareRest1");
            if (TokenStream[TokenIndex].token_type == Token_Class.Comma)
            {
                node.children.Add(match(Token_Class.Comma));
            }
            else
            {
                Error.Add("Comma Missing in Declaration Statement");
                count++;
            }
            if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {
                node.children.Add(match(Token_Class.Idenifier));
                node.children.Add(DeclareRest2());
                node.children.Add(DeclareRest1());
            }
            else
            {
                Error.Add("Identifier Missing in Declaration Statement");
                count++;
            }

            //MessageBox.Show(Error);
            if (count == 2)
                return null;
            foreach (string e in Error)
            {
                Errors.ParserError_List.Add(e);
            }

            return node;
        }

        private static Node DeclareRest2()
        {
            Node node = new Node("DeclareRest2");
            if (TokenStream[TokenIndex].token_type == Token_Class.Assignment || TokenStream[TokenIndex].token_type == Token_Class.BooleanOp)
            {
                TokenIndex--;
                node.children.Add(Assignment_Statement(true));
                //TokenIndex++;
                return node;
            }
           
            return null;
        }

        private static Node Write_Statement()
        {
            Node node = new Node("Write_Statement");
            List<string> Error = new List<string>();
            if (TokenStream[TokenIndex].token_type == Token_Class.Write)
            {
                node.children.Add(match(Token_Class.Write));
                try
                {
                    node.children.Add(WriteRest());
                }
                catch (ArgumentOutOfRangeException)
                {
                    Errors.ParserError_List.Add("Must be an endl or Expression after Write Statement");
                }
                try
                {
                    if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                    {
                        node.children.Add(match(Token_Class.Semicolon));
                        return node;
                    }
                    else
                    {
                        Errors.ParserError_List.Add("Missing Semicolon");
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    Errors.ParserError_List.Add("Semicolon Missing");
                }
            }

            return node;
        }

        private static Node WriteRest()
        {
            int tempindex = TokenIndex;
            List<string> Error = new List<string>();
            Node node = new Node("WriteRest");
            if (TokenStream[TokenIndex].token_type == Token_Class.Endl)
            {
                node.children.Add(match(Token_Class.Endl));
                return node;
            }
            else
            {
                tempindex = TokenIndex;
                Node tmp = Expression();
                if (tmp != null)
                {
                    node.children.Add(tmp);
                    return node;
                }
                TokenIndex = tempindex;
            }

            Errors.ParserError_List.Add("Must be an endl or Expression after Write Statement");
            MessageBox.Show(Error[0]);
            return null;
        }
        private static Node Return_Statement()
        {
            int tempindex = TokenIndex;
            Node node = new Node("Return_Statement");
            if (TokenStream[TokenIndex].token_type == Token_Class.Return)
            {
                node.children.Add(match(Token_Class.Return));
                Node tmp = Expression();

                if (tmp != null)
                {
                    node.children.Add(tmp);
                    if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                    {
                        node.children.Add(match(Token_Class.Semicolon));
                        return node;
                    }
                    else
                    {
                        Errors.ParserError_List.Add("Semicolon Missing");
                    }
                }
                else
                {
                    Errors.ParserError_List.Add("Expression Missing");
                    
                    if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                    {
                        node.children.Add(match(Token_Class.Semicolon));
                        return node;
                    }
                    else
                    {
                        Errors.ParserError_List.Add("Semicolon Missing");
                    }
                }
                TokenIndex = tempindex;
            }
            return null;
        }

        private static Node Read_Statement()
        {
            Node node = new Node("Read_Statement");
            if (TokenStream[TokenIndex].token_type == Token_Class.Read)
            {
                node.children.Add(match(Token_Class.Read));
                
            }
            if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {
                node.children.Add(match(Token_Class.Idenifier));

            }
            else
            {
                Errors.ParserError_List.Add("Identifier Missing");
            }
            if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
            {
                node.children.Add(match(Token_Class.Semicolon));
                //return node;
            }
            else
            {
                Errors.ParserError_List.Add("Semicolon Missing");
            }
            return node;
        }
        //---------- Hajer End ------------------------

        //----------- Youssef Start ---------------
        private static Node FunctionCall(bool isTerm)
        {
            Node node = new Node("FunctionCall");
            if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {
                node.children.Add(match(Token_Class.Idenifier));
                string Error = "No Error";
                if (TokenStream[TokenIndex].token_type != Token_Class.LParanthesis)
                {
                    Error = "Missing Left Paranthesis";
                    Errors.ParserError_List.Add(Error);
                    node.children.Add(Parameters());
                    if (TokenIndex == TokenStream.Count || TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                    {
                        Error = "Missing RParanthesis";
                        Errors.ParserError_List.Add(Error);
                        if (!isTerm)
                        {
                            if (TokenIndex == TokenStream.Count || TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                            {
                                Error = "Missing Semicolon";
                                Errors.ParserError_List.Add(Error);
                            }
                            else
                                node.children.Add(match(Token_Class.Semicolon));
                        }
                    }
                    else
                    {
                        node.children.Add(match(Token_Class.RParanthesis));
                        if (!isTerm)
                        {
                            if (TokenIndex == TokenStream.Count || TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                            {
                                Error = "Missing Semicolon";
                                Errors.ParserError_List.Add(Error);
                            }
                            else
                                node.children.Add(match(Token_Class.Semicolon));
                        }
                    }
                }
                else
                {
                    node.children.Add(match(Token_Class.LParanthesis));
                    node.children.Add(Parameters());
                    if (TokenIndex == TokenStream.Count || TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                    {
                        Error = "Missing RParanthesis";
                        Errors.ParserError_List.Add(Error);
                        if (!isTerm)
                        {
                            if (TokenIndex == TokenStream.Count || TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                            {
                                Error = "Missing Semicolon";
                                Errors.ParserError_List.Add(Error);
                            }
                            else
                                node.children.Add(match(Token_Class.Semicolon));
                        }
                    }
                    else
                    {
                        node.children.Add(match(Token_Class.RParanthesis));
                        if (!isTerm)
                        {
                            if (TokenIndex == TokenStream.Count || TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                            {
                                Error = "Missing Semicolon";
                                Errors.ParserError_List.Add(Error);
                            }
                            else
                                node.children.Add(match(Token_Class.Semicolon));
                        }
                    }
                }
                return node;
            }
            else
            {
                Errors.ParserError_List.Add("Missing Function Name");
                return null;
            }
        }

        private static Node Parameters()
        {
            Node node = new Node("Parameters");
            int tmpindex = TokenIndex;
            //use Node node = new Node(""); To Make "Parameters" hidden
            Node temp = Expression();
            if (temp == null)
            {
                TokenIndex = tmpindex;
                if (TokenIndex != TokenStream.Count && TokenStream[TokenIndex].token_type != Token_Class.RParanthesis && TokenStream[TokenIndex].token_type != Token_Class.Idenifier)
                {
                    Errors.ParserError_List.Add("Invalid Character ' " + TokenStream[TokenIndex].token_type.ToString() + "'");
                    TokenIndex++;
                    if (TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                        node.children.Add(Parameters());
                    return node;
                }
            }
            else
            {
                node.children.Add(temp);
                node.children.Add(MoreParameters());
                return node;
            }
            TokenIndex = tmpindex;
            if (TokenIndex != TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {
                node.children.Add(match(Token_Class.Idenifier));
                node.children.Add(MoreParameters());
                return node;
            }
           

            return node;
        }
        private static Node MoreParameters()
        {
            Node node = new Node("MoreParameters");
            int tmpindex = TokenIndex;
            if (TokenIndex != TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Comma)
            {
                node.children.Add(match(Token_Class.Comma));
                Node temp = Expression();
                if (temp == null)
                {
                    TokenIndex = tmpindex;
                    if (TokenIndex != TokenStream.Count && (TokenStream[TokenIndex].token_type != Token_Class.RParanthesis && TokenStream[TokenIndex].token_type != Token_Class.Idenifier))
                    {
                        Errors.ParserError_List.Add("Missing Parameter Name");
                        TokenIndex++;
                        if (TokenStream[TokenIndex].token_type != Token_Class.Semicolon)
                            node.children.Add(Parameters());
                        return node;
                    }
                }
                else
                {
                    node.children.Add(temp);
                    node.children.Add(MoreParameters());
                    return node;
                }
                TokenIndex = tmpindex;

                if (TokenIndex != TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                {
                    node.children.Add(match(Token_Class.Idenifier));
                    node.children.Add(MoreParameters());
                    return node;
                }
            }
            else if (TokenIndex != TokenStream.Count && (TokenStream[TokenIndex].token_type == Token_Class.RParanthesis || TokenStream[TokenIndex].token_type == Token_Class.Semicolon))
                return node;

            else
            {
                Node temp = Expression();
                if (TokenIndex != TokenStream.Count && (TokenStream[TokenIndex].token_type == Token_Class.Idenifier || temp != null))
                {
                    Errors.ParserError_List.Add("Missing Separator ',' ");
                    if (temp == null)
                    {
                        node.children.Add(match(Token_Class.Idenifier));
                    }
                    else
                    {
                        node.children.Add(temp);
                    }
                    if (TokenStream[TokenIndex].token_type != Token_Class.Comma || TokenStream[TokenIndex].token_type != Token_Class.Idenifier || temp == null)
                        node.children.Add(MoreParameters());
                    else if (TokenStream[TokenIndex].token_type != Token_Class.Semicolon || TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                        return node;
                    else
                    {
                        Errors.ParserError_List.Add("Invalid '" + TokenStream[TokenIndex].token_type.ToString() + "'");
                        TokenIndex++;
                        node.children.Add(MoreParameters());
                    }
                }
                else
                {
                    Errors.ParserError_List.Add("Error: unexpectable '" + TokenStream[TokenIndex].token_type.ToString() + "'");
                }
                return node;
            }
            return node;
        }
        private static Node Main_Function()
        {
            Node node = new Node("Main Function");
            string Error = "No Error";
            if (TokenStream[TokenIndex].token_type == Token_Class.Int)
            {
                node.children.Add(match(Token_Class.Int));
                if (TokenStream[TokenIndex].token_type == Token_Class.main)
                {
                    node.children.Add(match(Token_Class.main));
                    if (TokenStream[TokenIndex].token_type != Token_Class.LParanthesis)
                    {
                        Error = "Missing Left Paranthesis";
                        Errors.ParserError_List.Add(Error);
                        if (TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                        {
                            Error = "Missing Right Paranthesis";
                            Errors.ParserError_List.Add(Error);
                            node.children.Add(Function_Body());
                        }
                        else
                        {
                            node.children.Add(match(Token_Class.RParanthesis));
                            node.children.Add(Function_Body());
                        }
                    }
                    else
                    {
                        node.children.Add(match(Token_Class.LParanthesis));
                        if (TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                        {
                            Error = "Missing Right Paranthesis";
                            Errors.ParserError_List.Add(Error);
                            node.children.Add(Function_Body());
                        }
                        else
                        {
                            node.children.Add(match(Token_Class.RParanthesis));
                            node.children.Add(Function_Body());
                        }
                    }
                    return node;
                }
                else
                {
                    Errors.ParserError_List.Add("Missing Main Function");
                    return node;
                }

            }
            else
            {
                Errors.ParserError_List.Add("Missing Datatype");
                if (TokenStream[TokenIndex].token_type == Token_Class.main)
                {
                    node.children.Add(match(Token_Class.main));
                    if (TokenStream[TokenIndex].token_type != Token_Class.LParanthesis)
                    {
                        Error = "Missing Left Paranthesis";
                        Errors.ParserError_List.Add(Error);
                        if (TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                        {
                            Error = "Missing Right Paranthesis";
                            Errors.ParserError_List.Add(Error);
                            node.children.Add(Function_Body());
                        }
                        else
                        {
                            node.children.Add(match(Token_Class.RParanthesis));
                            node.children.Add(Function_Body());
                        }
                    }
                    else
                    {
                        node.children.Add(match(Token_Class.LParanthesis));
                        if (TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                        {
                            Error = "Missing Right Paranthesis";
                            Errors.ParserError_List.Add(Error);
                            node.children.Add(Function_Body());
                        }
                        else
                        {
                            node.children.Add(match(Token_Class.RParanthesis));
                            node.children.Add(Function_Body());
                        }
                    }
                    return node;
                }
                else
                {
                    Errors.ParserError_List.Add("Missing Main Function");
                    if (TokenStream[TokenIndex].token_type != Token_Class.LParanthesis)
                    {
                        Error = "Missing Left Paranthesis";
                        Errors.ParserError_List.Add(Error);
                    }
                    else
                    {
                        node.children.Add(match(Token_Class.LParanthesis));
                        if (TokenStream[TokenIndex].token_type != Token_Class.RParanthesis)
                        {
                            Error = "Missing Right Paranthesis";
                            Errors.ParserError_List.Add(Error);
                        }
                        else
                        {
                            node.children.Add(match(Token_Class.RParanthesis));
                            node.children.Add(Function_Body());
                        }
                    }
                    return node;
                }

            }
        }

        private static Node Assignment_Statement(bool isdeclared)
        {
            Node node = new Node("Assignment Statement");

            if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {
                node.children.Add(match(Token_Class.Idenifier));
                if (TokenStream[TokenIndex].token_type == Token_Class.Assignment)
                {
                    node.children.Add(match(Token_Class.Assignment));
                    node.children.Add(Expression());
                    if (!isdeclared)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                            node.children.Add(match(Token_Class.Semicolon));
                        else
                            Errors.ParserError_List.Add("Missing Semicolon");
                    }
                    return node;
                }
                else
                {
                    Errors.ParserError_List.Add("Missing Assignment");
                    if (TokenStream[TokenIndex].token_type == Token_Class.BooleanOp)
                        TokenIndex++;
                    node.children.Add(Expression());
                    if (!isdeclared)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                            node.children.Add(match(Token_Class.Semicolon));
                        else
                            Errors.ParserError_List.Add("Missing Semicolon");
                    }
                    return node;
                }
            }
            else
            {
                Errors.ParserError_List.Add("Missing Variable Name");
                if (TokenStream[TokenIndex].token_type == Token_Class.Assignment)
                {
                    node.children.Add(match(Token_Class.Assignment));
                    node.children.Add(Expression());
                    if (!isdeclared)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                            node.children.Add(match(Token_Class.Semicolon));
                        else
                            Errors.ParserError_List.Add("Missing Semicolon");
                    }

                    return node;
                }
                else
                {
                    Errors.ParserError_List.Add("Missing Assignment Operator  ' := '");
                    if (TokenStream[TokenIndex].token_type == Token_Class.BooleanOp)
                        TokenIndex++;
                    node.children.Add(Expression());
                    if (!isdeclared)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.Semicolon)
                            node.children.Add(match(Token_Class.Semicolon));
                        else
                            Errors.ParserError_List.Add("Missing Semicolon");
                    }
                    return node;
                }
            }
        }
        //------------- Youssef End ------------------

        //--------------- Nada Start ----------------
        private static Node If_Statement()
        {
            List<string> error = new List<string>();
            Node node = new Node("If_Statement");

            if (TokenStream[TokenIndex].token_type == Token_Class.If)
            {

                node.children.Add(match(Token_Class.If));

                //error handling
                int tmpindx = TokenIndex;
                Node tmp_node = Condition_Statement();
                if (tmp_node.children[0] == null)
                {
                    TokenIndex = tmpindx;
                    Errors.ParserError_List.Add("missing Condition Statement");
                }
                else
                {
                    node.children.Add(tmp_node);
                }

                tmpindx = TokenIndex;
                if (TokenStream[TokenIndex].token_type == Token_Class.Then)
                {
                    node.children.Add(match(Token_Class.Then));

                    node.children.Add(Statements());

                    //error handling
                    tmpindx = TokenIndex;
                    tmp_node = Other_Conditions();
                    if (tmp_node == null)
                    {
                        TokenIndex = tmpindx;
                        Errors.ParserError_List.Add("missing else if or else or End statement");
                    }
                    else
                    {
                        node.children.Add(tmp_node);

                    }
                }
                //error handling 

                else
                {
                    int tempidx = TokenIndex;
                    Errors.ParserError_List.Add("missing then");
                    //TokenIndex++;
                    tmp_node = Statements();
                    //node.children.Add(Statements());
                    if (tmp_node != null)
                    {
                        node.children.Add(tmp_node);
                    }
                    else
                    {
                        TokenIndex = tempidx;
                        TokenIndex++;
                        node.children.Add(Statements());
                    }
                    //error handling
                    tmpindx = TokenIndex;
                    tmp_node = Other_Conditions();
                    if (tmp_node == null)
                    {
                        TokenIndex = tmpindx;
                        Errors.ParserError_List.Add("missing else if or else or End statement");
                    }
                    else
                    {
                        node.children.Add(tmp_node);

                    }
                }

            }
            //foreach (string e in error)
            //{
            //    MessageBox.Show(e);
            //}

            return node;
        }
        //else if and else conditions and end statement
        private static Node Other_Conditions()
        {
            Node node = new Node("Other_Conditions");
            if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.ElseIf)
            {
                node.children.Add(Else_If_Statement());
            }
            else if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Else)
            {
                node.children.Add(Else_Statement());
            }
            else if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.End)
            {
                node.children.Add(match(Token_Class.End));
            }
            else
            {
                Errors.ParserError_List.Add("missing end");
            }
            return node;
        }

        //condition statement 
        private static Node Condition_Statement()
        {
            Node node = new Node("Condition_Statement");
            node.children.Add(Condition());
            return node;
        }
        // condition statement with one or more condition
        private static Node More_Conditions()
        {
            List<string> error = new List<string>();
            Node node = new Node("More_Conditions");
            Node tempNode = null;
            if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.AndOp)
            {
                node.children.Add(match(Token_Class.AndOp));
                tempNode = Condition();
                node.children.Add(tempNode);

            }
            else if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.OR_Opeartor)
            {
                node.children.Add(match(Token_Class.OR_Opeartor));
                tempNode = Condition();
                node.children.Add(tempNode);

            }

            
            return node;
        }
        //condition
        private static Node Condition()
        {
            List<string> error = new List<string>();
            Node node = new Node("Condition");

            if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {

                node.children.Add(match(Token_Class.Idenifier));

            }
            //error handling
            else
            {
                Errors.ParserError_List.Add("missing identifier");
            }
            //error handling
            int tmpindx = TokenIndex;
            Node tmpnode = Condition_Operator();
            if (tmpnode == null)
            {
                TokenIndex = tmpindx;
                Errors.ParserError_List.Add("missing condition operator");
            }
            else
            {
                node.children.Add(tmpnode);

            }
            //error handling
            tmpindx = TokenIndex;
            tmpnode = Term();
            if (tmpnode == null)
            {
                TokenIndex = tmpindx;
                Errors.ParserError_List.Add("missing term");
            }
            else
            {
                node.children.Add(tmpnode);
            }
            node.children.Add(More_Conditions());
            //foreach (string e in error)
            //{
            //    MessageBox.Show(e);
            //}
            return node;
        }
        //else if statement
        private static Node Else_If_Statement()
        {
            List<string> error = new List<string>();
            Node node = new Node("Else_If_Statement");

            if (TokenStream[TokenIndex].token_type == Token_Class.ElseIf)
            {
                node.children.Add(match(Token_Class.ElseIf));
                int tmpindx = TokenIndex;
                Node tmp_node = Condition_Statement();
                if (tmp_node.children[0] == null)
                {
                    TokenIndex = tmpindx;
                    Errors.ParserError_List.Add("missing Condition Statement");
                }
                else
                {
                    node.children.Add(tmp_node);
                }

                tmpindx = TokenIndex;
                if (TokenStream[TokenIndex].token_type == Token_Class.Then)
                {
                    node.children.Add(match(Token_Class.Then));

                    node.children.Add(Statements());

                    //error handling
                    tmpindx = TokenIndex;
                    tmp_node = Other_Conditions();
                    if (tmp_node == null)
                    {
                        TokenIndex = tmpindx;
                        Errors.ParserError_List.Add("missing else if or else or End statement");
                    }
                    else
                    {
                        node.children.Add(tmp_node);

                    }
                }
                //error handling 

                else
                {
                    int tempidx = TokenIndex;
                    Errors.ParserError_List.Add("missing then");
                    //TokenIndex++;
                    tmp_node = Statements();
                    //node.children.Add(Statements());
                    if (tmp_node != null)
                    {
                        node.children.Add(tmp_node);
                    }
                    else
                    {
                        TokenIndex = tempidx;
                        TokenIndex++;
                        node.children.Add(Statements());
                    }
                    //error handling
                    tmpindx = TokenIndex;
                    tmp_node = Other_Conditions();
                    if (tmp_node == null)
                    {
                        TokenIndex = tmpindx;
                        Errors.ParserError_List.Add("missing else if or else or End statement");
                    }
                    else
                    {
                        node.children.Add(tmp_node);

                    }
                }

            }
            //foreach (string e in error)
            //{
            //    MessageBox.Show(e);
            //}

            return node;

        }
        //else statement
        private static Node Else_Statement()
        {
            List<string> error = new List<string>();
            Node node = new Node("Else_Statement");
            if (TokenStream[TokenIndex].token_type == Token_Class.Else)
            {
                node.children.Add(match(Token_Class.Else));
                node.children.Add(Statements());
                if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.End)
                {
                    node.children.Add(match(Token_Class.End));
                }
                else
                {
                    Errors.ParserError_List.Add("missing End");
                }
            }
            return node;
        }
        // repeate (loop with condition)
        private static Node Repeate_Statement()
        {
            List<string> error = new List<string>();
            Node node = new Node("Repeate_Statement");
            if (TokenStream[TokenIndex].token_type == Token_Class.Repeat)
            {
                node.children.Add(match(Token_Class.Repeat));

                node.children.Add(Statements());

                int tempindx = TokenIndex;

                if (TokenStream[TokenIndex].token_type == Token_Class.Until)
                {
                    node.children.Add(match(Token_Class.Until));
                }
                //error handling
                else
                {
                    TokenIndex = tempindx;
                    Errors.ParserError_List.Add("missing until");
                }

                //error handling
                Node tmpnode = Condition_Statement();
                if (tmpnode.children[0] == null)
                {
                    Errors.ParserError_List.Add("missing Condition Statement");
                }
                else
                {
                    node.children.Add(tmpnode);

                }
            }
            //foreach (string e in error)
            //{
            //    MessageBox.Show(e);
            //}
            return node;
        }

        //--------------- Nada End ------------------

        //--------------- Hend Start ----------------
        public static Node Expression()
        {
            Node node = new Node("Expression");
            //start
            if (TokenIndex < TokenStream.Count)
            {
                Node tmp1;
                //----------------- -----String----------------------------
                if (TokenStream[TokenIndex].token_type == Token_Class.String)
                {
                    node.children.Add(match(Token_Class.String));
                    return node;
                }
                //-----------------------Equation & Term--------------------------
                tmp1 = Equation();
                if (tmp1 != null)
                {
                    if (tmp1.children[0].Name == "Term" && tmp1.children.Count == 2 && tmp1.children[1] == null)
                    {
                        node.children.Add(tmp1.children[0]);
                    }
                    else
                        node.children.Add(tmp1);
                    return node;
                }
            }
            else
            {
                Errors.ParserError_List.Add("error");
            }
            //end
            return null;
        }

        //Term Function-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static Node Term()
        {
            Node node = new Node("Term");
            if (TokenStream[TokenIndex].token_type == Token_Class.Number)
            {
                node.children.Add(match(Token_Class.Number));
                return node;
            }
            else if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
            {
                if (TokenIndex + 1 < TokenStream.Count && (TokenStream[TokenIndex + 1].token_type == Token_Class.LParanthesis))    // Sum );
                {
                    node.children.Add(FunctionCall(true));
                    return node;
                }
                //else if (TokenIndex + 1 < TokenStream.Count && (TokenStream[TokenIndex + 1].token_type == Token_Class.Comma || TokenStream[TokenIndex + 1].token_type == Token_Class.Idenifier
                //    || TokenStream[TokenIndex + 1].token_type == Token_Class.RParanthesis))
                //{
                //    node.children.Add(FunctionCall(true));
                //    return node;
                //}
                else
                {
                    node.children.Add(match(Token_Class.Idenifier));
                    return node;
                }
            }
            return null;
        }
        //Equation Function-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Node Equation()
        {
            Node node = new Node("Equation");
            //(Equation) EquationRestDash -----------------------------------------------
            if (TokenStream[TokenIndex].token_type == Token_Class.LParanthesis)
            {
                node.children.Add(match(Token_Class.LParanthesis));
                node.children.Add(Equation());
                if (TokenStream[TokenIndex].token_type == Token_Class.RParanthesis)
                {
                    node.children.Add(match(Token_Class.RParanthesis));
                    node.children.Add(EquationRestDash());
                    return node;
                }
                else
                {
                    Errors.ParserError_List.Add("Missing ) in Equation .");
                    node.children.Add(EquationRestDash());
                    return node;
                }
            }
            //Term EquationRestDash -----------------------------------------------------
            else if (TokenStream[TokenIndex].token_type != Token_Class.LParanthesis)
            {
                Node temp = Term();
                if (temp != null)
                {

                    node.children.Add(temp);
                    try
                    {
                        node.children.Add(EquationRestDash());
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        node.children.Add(null);
                    }
                    //if(TokenStream[TokenIndex - 1].token_type != Token_Class.Semicolon)

                    return node;
                }
            }
            return null;
        }
        public static Node EquationRestDash()
        {
            Node node = new Node("EquationRestDash");

            //            int TempIndex = TokenIndex;
            if (TokenIndex < TokenStream.Count && (TokenStream[TokenIndex].token_type == Token_Class.PlusOp || TokenStream[TokenIndex].token_type == Token_Class.MinusOp || TokenStream[TokenIndex].token_type == Token_Class.MultiplyOp || TokenStream[TokenIndex].token_type == Token_Class.DivideOp))
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.PlusOp)
                {
                    node.children.Add(match(Token_Class.PlusOp));
                    Node temp = Equation();
                    if (temp != null)
                    {
                        node.children.Add(temp);
                        node.children.Add(EquationRestDash());
                    }

                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.MinusOp)
                {
                    node.children.Add(match(Token_Class.MinusOp));
                    Node temp = Equation();
                    if (temp != null)
                    {
                        node.children.Add(temp);
                        node.children.Add(EquationRestDash());
                    }
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.MultiplyOp)
                {
                    node.children.Add(match(Token_Class.MultiplyOp));
                    Node temp = Equation();
                    if (temp != null)
                    {
                        node.children.Add(temp);
                        node.children.Add(EquationRestDash());
                    }
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.DivideOp)
                {
                    node.children.Add(match(Token_Class.DivideOp));
                    Node temp = Equation();
                    if (temp != null)
                    {
                        node.children.Add(temp);
                        node.children.Add(EquationRestDash());
                    }
                }
                return node;
            }
            //TokenIndex = TempIndex;
            return null;
        }

        //--------------- Hend End ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        // -------------- Nermeen Start --------------
        private static Node DataType()
        {
            Node node = new Node("DataType");
            if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Int)
                {
                    //match int
                    node.children.Add(match(Token_Class.Int));
                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.Float)
                {
                    //match float
                    node.children.Add(match(Token_Class.Float));
                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.String)
                {
                    //match string
                    node.children.Add(match(Token_Class.String));
                    return node;
                }
            }
            else
            {
                Errors.ParserError_List.Add("error");
            }
            //if error
            return null;
        }
        private static Node Program()
        {
            Node node = new Node("Program");
            node.children.Add(ProgFunctionStatement());
            if (TokenIndex < TokenStream.Count)
                node.children.Add(Main_Function());
            else
                Errors.ParserError_List.Add("missing main function");
            return node;
        }
        private static Node ProgFunctionStatement()
        {
            Node node = new Node("ProgFunctionStatement");

            int TempIndex = TokenIndex;
            Node TempResult = Function_Statement();

            if (TempResult != null)
            {
                node.children.Add(TempResult);
                if (TokenIndex < TokenStream.Count)
                {
                    node.children.Add(ProgFunctionStatement());
                }
                return node;
            }
            TokenIndex = TempIndex;
            return node;
        }
        private static Node Function_Statement()
        {
            Node node = new Node("Function_Statement");
            Node Temp = Function_Declaration();
            if (Temp != null)
            {
                node.children.Add(Temp);
                if (TokenIndex < TokenStream.Count)
                {
                    node.children.Add(Function_Body());
                }
                else
                {
                    Errors.ParserError_List.Add("missing function body");
                }
                return node;
            }
            return null;
        }
        private static Node Function_Declaration()
        {
            Node node = new Node("Function_Declaration");
            Node TempReturn = DataType();
            if (TempReturn == null)
            {
                if (TokenIndex < TokenStream.Count)
                {
                    if (TokenStream[TokenIndex].token_type == Token_Class.main)
                    {
                        return null;
                    }
                }
                else
                {
                    Errors.ParserError_List.Add("error");
                }

                Errors.ParserError_List.Add("missing Data Type in function declaration");

            }
            else if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.main)
                {
                    return null;
                }
                else
                {
                    node.children.Add(TempReturn);
                }
            }

            TempReturn = Function_Name();

            if (TempReturn == null)
            {
                Errors.ParserError_List.Add("missing function name in function declaration");
                tempParameters.Add("0");
            }
            else
            {
                node.children.Add(TempReturn);
                //add the function name 
                tempParameters.Add(TempReturn.children[0].Name);
            }
            if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.LParanthesis)
                {
                    node.children.Add(match(Token_Class.LParanthesis));
                    node.children.Add(FnParameters());

                    //parameters.Add(tempParameters);

                    if (TokenStream[TokenIndex].token_type == Token_Class.RParanthesis)
                    {
                        node.children.Add(match(Token_Class.RParanthesis));
                        return node;
                    }

                    else
                    {
                        Errors.ParserError_List.Add("missing Right Paranthesis in function declaration");
                    }

                }
                else
                {
                    Errors.ParserError_List.Add("missing Left Paranthesis in function declaration");

                    node.children.Add(FnParameters());

                    //parameters.Add(tempParameters);

                    if (TokenStream[TokenIndex].token_type == Token_Class.RParanthesis)
                    {
                        node.children.Add(match(Token_Class.RParanthesis));
                        return node;
                    }

                    else
                    {
                        Errors.ParserError_List.Add("missing Right Paranthesis in function declaration");
                    }
                }

            }
            else
            {
                Errors.ParserError_List.Add("missing the rest of the program");
            }
            return node;
        }
        private static Node Function_Name()
        {
            Node node = new Node("Function_Name");
            if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                {
                    //match identifier
                    node.children.Add(match(Token_Class.Idenifier));
                    return node;
                }
            }
            else
            {
                Errors.ParserError_List.Add("error");
            }

            //if error
            return null;
        }
        private static Node FnParameters()
        {
            Node node = new Node("FnParameters");

            int TempIndex = TokenIndex;
            Node TempResult = DataType();

            if (TempResult != null)
            {
                tempParameters.Add("1");
                node.children.Add(TempResult);
                if (TokenIndex < TokenStream.Count)
                {
                    if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                    {
                        //match identifier
                        node.children.Add(match(Token_Class.Idenifier));
                    }
                    else
                    {
                        Errors.ParserError_List.Add("missing parameter in function declaration parameters");
                    }

                    node.children.Add(MoreParametersDeclare());

                    return node;
                }
                else
                {
                    Errors.ParserError_List.Add("error");
                }

            }
            else if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                {
                    Errors.ParserError_List.Add("missing Data Type in function declaration parameters");
                    tempParameters.Add("1");
                    node.children.Add(match(Token_Class.Idenifier));
                    node.children.Add(MoreParametersDeclare());

                    return node;
                }
            }
            else if (TokenIndex >= TokenStream.Count)
            {
                Errors.ParserError_List.Add("error");
            }

            TokenIndex = TempIndex;
            return node;
        }
        private static Node MoreParametersDeclare()
        {
            Node node = new Node("MoreParametersDeclare");

            int TempIndex = TokenIndex;
            if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Comma)
                {
                    tempParameters.Add("1");
                    node.children.Add(match(Token_Class.Comma));
                    Node temp = DataType();
                    if (node == null)
                    {
                        Errors.ParserError_List.Add("missing data type in function declaration parameters");
                    }
                    else
                    {
                        node.children.Add(temp);
                    }

                    if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                    {
                        //match identifier
                        node.children.Add(match(Token_Class.Idenifier));
                    }
                    else
                    {
                        Errors.ParserError_List.Add("missing parameter in function declaration parameters");
                    }

                    node.children.Add(MoreParametersDeclare());

                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.Int || TokenStream[TokenIndex].token_type == Token_Class.Float
                        || TokenStream[TokenIndex].token_type == Token_Class.String)
                {
                    Errors.ParserError_List.Add("missing comma in function declaration parameters");
                    tempParameters.Add("1");

                    Node temp = DataType();
                    if (node == null)
                    {
                        Errors.ParserError_List.Add("missing data type in function declaration parameters");
                    }
                    else
                    {
                        node.children.Add(temp);
                    }

                    if (TokenIndex < TokenStream.Count)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                        {
                            //match identifier
                            node.children.Add(match(Token_Class.Idenifier));
                        }
                        else
                        {
                            Errors.ParserError_List.Add("missing parameter in function declaration parameters");
                        }
                    }
                    else
                    {
                        Errors.ParserError_List.Add("error");
                    }
                    node.children.Add(MoreParametersDeclare());

                    return node;
                }
            }
            else
            {
                Errors.ParserError_List.Add("error");
            }
            TokenIndex = TempIndex;
            return node;
        }
        private static Node Function_Body()
        {
            Node node = new Node("Function_Body");

            if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.LeftBraces)
                {
                    node.children.Add(match(Token_Class.LeftBraces));
                    node.children.Add(Statements());
                    if (TokenStream[TokenIndex].token_type == Token_Class.Return)
                    {
                        node.children.Add(Return_Statement());
                    }
                    else
                    {
                        Errors.ParserError_List.Add("Missing Return in Function Body");
                    }

                    if (TokenIndex < TokenStream.Count)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.RightBraces)
                        {
                            node.children.Add(match(Token_Class.RightBraces));
                            return node;
                        }
                        else
                        {
                            Errors.ParserError_List.Add("missing right braces in the end of function body");
                        }
                    }
                    else
                    {
                        Errors.ParserError_List.Add("missing right braces in the end of function body");
                    }

                }
                else
                {
                    Errors.ParserError_List.Add("missing left braces in the begining of function body");
                    node.children.Add(Statements());
                    if (TokenStream[TokenIndex].token_type == Token_Class.Return)
                    {
                        node.children.Add(Return_Statement());
                    }
                    else
                    {
                        Errors.ParserError_List.Add("Missing Return in Function Body");
                    }

                    if (TokenIndex < TokenStream.Count)
                    {
                        if (TokenStream[TokenIndex].token_type == Token_Class.RightBraces)
                        {
                            node.children.Add(match(Token_Class.RightBraces));
                            return node;
                        }
                        else
                        {
                            Errors.ParserError_List.Add("missing right braces in the end of function body");
                        }
                    }
                    else
                    {
                        Errors.ParserError_List.Add("missing right braces in the end of function body");
                    }

                }
            }
            else
            {
                Errors.ParserError_List.Add("error");
            }
            return node;
        }
        private static Node Statements()
        {
            Node node = new Node("Statements");

            int TempIndex = TokenIndex;
            Node TempResult = State();
            if (TempResult != null)
            {
                if (TempResult.children[0] != null)
                {
                    node.children.Add(TempResult);
                    //TokenIndex++;
                    node.children.Add(Statements());

                    return node;
                }
            }

            TokenIndex = TempIndex;
            return null;
        }
        private static Node State()
        {
            Node node = new Node("State");

            if (TokenIndex < TokenStream.Count)
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                {
                    if (TokenStream[TokenIndex + 1].token_type == Token_Class.LParanthesis)
                    {
                        node.children.Add(FunctionCall(false));
                        return node;
                    }
                    else if (TokenStream[TokenIndex + 1].token_type == Token_Class.Assignment || TokenStream[TokenIndex +1 ].token_type == Token_Class.BooleanOp)
                    {
                        node.children.Add(Assignment_Statement(false));
                        return node;
                    }
                    //else
                    //{
                    //    //error
                    //    MessageBox.Show("wrong statement in function body (function call OR assignment statement)");
                    //}
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.Int || TokenStream[TokenIndex].token_type == Token_Class.Float
                    || TokenStream[TokenIndex].token_type == Token_Class.String)
                {
                    node.children.Add(Declaration_Statement());
                    if (node == null)
                    {
                        TokenIndex++;
                    }
                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.Write)
                {
                    node.children.Add(Write_Statement());
                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.Read)
                {
                    node.children.Add(Read_Statement());
                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.If)
                {
                    node.children.Add(If_Statement());
                    return node;
                }
                else if (TokenStream[TokenIndex].token_type == Token_Class.Repeat)
                {
                    node.children.Add(Repeate_Statement());
                    return node;
                }              

            }
            //else
            //{
            //    MessageBox.Show("error");
            //}

            //no statement
            return null;
        }
        // -------------- Nermeen End ----------------

        public static Node match(Token_Class ExpectedToken)
        {
            Token currentToken = TokenStream[TokenIndex];
            if (currentToken.token_type == ExpectedToken)
            {
                Node node = new Node(currentToken.lex);
                TokenIndex++;
                return node;
            }
            return null;
        }


        //use this function to print the parse tree in TreeView Toolbox
        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.children.Count == 0)
                return tree;
            foreach (Node child in root.children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
