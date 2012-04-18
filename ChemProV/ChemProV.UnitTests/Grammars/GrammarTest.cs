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

        /// <summary>
        /// This will return the following PFD:
        ///     Incoming Stream #1:
        ///         Overall label: M1
        ///             m11 compound: ammonia
        ///     Incoming Stream #2:
        ///         Overall label: M2, Qty: 200g
        ///             m21 compound: 33% acetic acid
        ///             m22 compound: 67% benzene
        ///     Incoming Streams #1 and #2 are fed into a mixer
        ///     Outgoing Mixer Stream:
        ///         Overall: Label 300g M3
        ///             m31: acetic acid
        ///             m32: ammonia
        ///             m33: benzene
        /// </summary>
        /// <returns></returns>
        public List<IPfdElement> GetSamplePfd()
        {
            //will hold the finished PFD graph
            List<IPfdElement> graph = new List<IPfdElement>();

            //process units
            IProcessUnit incoming1Source = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Source);
            graph.Add(incoming1Source);
            IProcessUnit incoming2Source = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Source);
            graph.Add(incoming2Source);
            IProcessUnit mixer = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Mixer);
            graph.Add(mixer);
            IProcessUnit endSink = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Sink);
            graph.Add(endSink);

            //Incoming Stream #1
            IStream incoming1 = StreamFactory.StreamFromStreamType(StreamType.Chemical);
            graph.Add(incoming1);
            ChemicalStreamPropertiesWindow stream1Window = (ChemicalStreamPropertiesWindow)PropertiesWindowFactory.TableFromStreamType(StreamType.Chemical, OptionDifficultySetting.MaterialBalance, false);
            incoming1.Table = stream1Window;
            graph.Add(stream1Window);
            stream1Window.ItemSource[0].Label = "M1";
            //Remove the default item at index 1
            stream1Window.ItemSource.RemoveAt(1);
            stream1Window.ItemSource.Add(new ChemicalStreamData()
                                            {
                                                Label = "m11",
                                                SelectedCompoundId = 1
                                            }
                                        );

            //Incoming Stream #2
            IStream incoming2 = StreamFactory.StreamFromStreamType(StreamType.Chemical);
            graph.Add(incoming2);
            ChemicalStreamPropertiesWindow stream2Window = (ChemicalStreamPropertiesWindow)PropertiesWindowFactory.TableFromStreamType(StreamType.Chemical, OptionDifficultySetting.MaterialBalance, false);
            incoming2.Table = stream2Window;
            graph.Add(stream2Window);            
            stream2Window.ItemSource[0].Label = "M2";
            //Remove the default item at index 1
            stream2Window.ItemSource.RemoveAt(1);
            stream2Window.ItemSource.Add(new ChemicalStreamData()
                                            {
                                                Label = "m21",
                                                SelectedCompoundId = 0,
                                                Quantity = "33"
                                            }
                                        );
            stream2Window.ItemSource.Add(new ChemicalStreamData()
                                            {
                                                Label = "m22",
                                                SelectedCompoundId = 2,
                                                Quantity = "67"
                                            }
                                        );

            //Outgoing Stream 
            IStream outgoing = StreamFactory.StreamFromStreamType(StreamType.Chemical);
            graph.Add(outgoing);
            ChemicalStreamPropertiesWindow outgoingWindow = (ChemicalStreamPropertiesWindow)PropertiesWindowFactory.TableFromStreamType(StreamType.Chemical, OptionDifficultySetting.MaterialBalance, false);
            outgoing.Table = outgoingWindow;
            graph.Add(outgoingWindow);
            outgoingWindow.ItemSource[0].Label = "M3";
            //Remove the default item at index 1
            outgoingWindow.ItemSource.RemoveAt(1);
            outgoingWindow.ItemSource[0].Quantity = "300";
            outgoingWindow.ItemSource.Add(new ChemicalStreamData()
                                            {
                                                Label = "m31",
                                                SelectedCompoundId = 0
                                            }
                                        );
            outgoingWindow.ItemSource.Add(new ChemicalStreamData()
                                            {
                                                Label = "m32",
                                                SelectedCompoundId = 1
                                            }
                                        );
            outgoingWindow.ItemSource.Add(new ChemicalStreamData()
                                            {
                                                Label = "m33",
                                                SelectedCompoundId = 2
                                            }
                                        );

            //attach process units to streams and vice versa 
            incoming1Source.OutgoingStreams.Add(incoming1);
            incoming2Source.OutgoingStreams.Add(incoming2);
            mixer.IncomingStreams.Add(incoming1);
            mixer.IncomingStreams.Add(incoming2);
            mixer.OutgoingStreams.Add(outgoing);
            endSink.IncomingStreams.Add(outgoing);

            incoming1.Source = incoming1Source;
            incoming1.Destination = mixer;

            incoming2.Source = incoming2Source;
            incoming2.Destination = mixer;

            outgoing.Source = mixer;
            outgoing.Destination = endSink;

            return graph;
        }
        
        [TestMethod]
        public void TestOverallEquation()
        {
            ChemProVTree tree = ParseText("M1 + M2 = M3");
            Assert.AreEqual(1, tree.Lines.Count);
            Assert.AreEqual(3, tree.Lines[0].Tokens.Keys.Count);
            GetSamplePfd();
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
