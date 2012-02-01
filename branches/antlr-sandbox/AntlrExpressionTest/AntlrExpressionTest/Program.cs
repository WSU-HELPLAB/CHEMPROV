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
            EasyExprLexer lexer = new EasyExprLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            EasyExprParser parser = new EasyExprParser(tokens);
            
            AstParserRuleReturnScope<CommonTree, IToken> result = parser.prog();
            CommonTree tree = result.Tree;
            CommonTreeNodeStream nodes = new CommonTreeNodeStream(tree);
            Eval walker = new Eval(nodes);
            walker.prog();            
        }
    }
}
