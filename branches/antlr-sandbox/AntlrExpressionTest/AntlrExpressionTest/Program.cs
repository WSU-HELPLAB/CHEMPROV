using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using System.IO;
using System.Collections;
using Antlr.Runtime.Tree;

namespace AntlrExpressionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            string inputText = "";
            while ( (inputText = Console.ReadLine()) != "q")
            {
                writer.WriteLine(inputText);
            }
            writer.Flush();
            stream.Position = 0;
            
            ANTLRInputStream input = new ANTLRInputStream(stream);
            ChemProVLexer lexer = new ChemProVLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            ChemProVParser parser = new ChemProVParser(tokens);
            
            AstParserRuleReturnScope<CommonTree, IToken> result = parser.program();
            CommonTree tree = result.Tree;
            CommonTreeNodeStream nodes = new CommonTreeNodeStream(tree);
            nodes.TokenStream = tokens;
            ChemProVTree walker = new ChemProVTree(nodes);
            walker.program();

            /*
            List<ChemProVLine> lines = new List<ChemProVLine>();
            int currentLineNumber = 0;
            if (lines.Count < currentLineNumber)
            {
                lines.Add(new ChemProVLine());
            }
            lines[currentLineNumber].VariablesUsed.Add("foo");
             * */

        }
    }
}
