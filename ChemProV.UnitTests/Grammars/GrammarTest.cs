using System;
using System.Collections.Generic;
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
using System.Linq;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.UnitTests.Grammars
{
    [TestClass]
    public class GrammarTest
    {
        private ChemProVTree ParseText(string text)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(text);
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
            return walker;
        }

        [TestMethod]
        public List<IPfdElement> GetSamplePfd()
        {
            List<IPfdElement> graph = new List<IPfdElement>();

            IProcessUnit source = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Source);
            IProcessUnit mixer = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Mixer);
            IStream incomingStream = StreamFactory.StreamFromStreamType(StreamType.Chemical);

            incomingStream.Destination = mixer;
            incomingStream.Source = source;
            
            //populate the incoming stream's data
            ChemicalStreamData d = new ChemicalStreamData();
            d.Label = "SampleTable";
            d.Quantity = "42";
            d.Units = 5;
            d.Compound = 1;
            d.TempUnits = 1;
            d.Temperature = "32";

            incomingStream.Table = PropertiesWindowFactory.TableFromStreamType(StreamType.Chemical, OptionDifficultySetting.MaterialBalance, false);
            //add ChemicalStreamData to the first (not header) row
            (incomingStream.Table as ChemicalStreamPropertiesWindow).ItemSource[1] = d;
            
            //populate additional row data
            //add additional ChemicalStreamData rows
            (incomingStream.Table as ChemicalStreamPropertiesWindow).ItemSource.Add(d);

            mixer.AttachIncomingStream(incomingStream);
            source.AttachOutgoingStream(incomingStream);

            return graph;
        }
        
        [TestMethod]
        public void TestOverallEquation()
        {
            ChemProVTree tree = ParseText("M1 + M2 = M3");
            Assert.AreEqual(1, tree.Lines.Count);
            Assert.AreEqual(3, tree.Lines[0].Tokens.Keys.Count);
        }

        [TestMethod]
        public void TestPercent()
        {
            ChemProVTree tree = ParseText("M1 / 100 = 0");
            Dictionary<int, Variable>.ValueCollection values = tree.Lines[0].Tokens.Values;
            Assert.AreEqual(true, values.First().IsPercent);
        }
    }
}
