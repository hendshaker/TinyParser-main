using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TinyParser
{
    public partial class Parser : Form
    {
        public Parser()
        {
            InitializeComponent();
        }
        void PrintTokens()
        {
            for (int i = 0; i < Tiny_Compiler.TinyScanner.Tokens.Count; i++)
            {
                dataGridView1.Rows.Add(Tiny_Compiler.TinyScanner.Tokens.ElementAt(i).lex, Tiny_Compiler.TinyScanner.Tokens.ElementAt(i).token_type);
            }
        }
        void PrintErrors()
        {
            for (int i = 0; i < Errors.Error_List.Count; i++)
            {
                textBox2.Text += Errors.Error_List[i];
                textBox2.Text += "\r\n";
            }
            for (int i = 0; i < Errors.ParserError_List.Count; i++)
            {
                textBox3.Text += Errors.ParserError_List[i];
                textBox3.Text += "\r\n";
                textBox3.Text += "---------------------------------------------------------- \r\n";
            }
        }
       
        private void Compile_btn_Click_1(object sender, EventArgs e)
        {
            textBox2.Clear();
            //string Code=textBox1.Text.ToLower();
            string Code = textBox1.Text;
            Tiny_Compiler.Start_Compiling(Code);
            PrintTokens();
            //PrintLexemes();

            Node root = SyntaxAnalyser.Parse(Tiny_Compiler.TokenStream);
            ParseTree.Nodes.Add(SyntaxAnalyser.PrintParseTree(root));
            PrintErrors();

        }

        private void Clear_btn_Click_1(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            textBox2.Clear();
            textBox3.Clear();
            Errors.Error_List.Clear();
            Errors.ParserError_List.Clear();
            Tiny_Compiler.TokenStream.Clear();
            ParseTree.Nodes.Clear();
          
        }
    }
}
