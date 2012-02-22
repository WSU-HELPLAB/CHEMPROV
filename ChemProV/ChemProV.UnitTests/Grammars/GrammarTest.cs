using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using Antlr.Runtime;
using ChemProV.Grammars;
using Antlr.Runtime.Tree;

namespace ChemProV.UnitTests.Grammars
{
    [TestClass]
    public class GrammarTest
    {
        [TestMethod]
        public void TestOverallEquation()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("M1 + M2 = M3");
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

            Assert.AreEqual(1, walker.Lines.Count);
            Assert.AreEqual(true, walker.Lines[0].Tokens.Contains("M1"));
            Assert.AreEqual(true, walker.Lines[0].Tokens.Contains("M2"));
            Assert.AreEqual(true, walker.Lines[0].Tokens.Contains("M3"));
        }
    }
}
