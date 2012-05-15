using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChemProV.UI;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;

namespace ChemProV.UnitTests.PFD
{
    /// <summary>
    /// Unit test for comment merging functionality. Creates two workspaces in memory with the same 
    /// process units but different sets of comments and then merges them, ensuring that that they 
    /// merged correctly.
    /// </summary>
    [TestClass]
    public class CommentMerge
    {
        [TestMethod]
        public void TestMergeInMemory()
        {
            // Create two MainPage objects
            MainPage mp1 = new MainPage();
            MainPage mp2 = new MainPage();

            // Get the workspaces
            WorkSpace w1 = mp1.WorkspaceReference;
            WorkSpace w2 = mp2.WorkspaceReference;
            
            // Add three process units to w1
            int puCount = 3;
            for (int i = 0; i < puCount; i++)
            {
                Mixer mixer1 = new Mixer();
                w1.DrawingCanvasReference.AddNewChild(mixer1);

                // Add a comment
                StickyNote sn1;
                StickyNote.CreateCommentNote(w1.DrawingCanvasReference, mixer1, null, out sn1);
                sn1.CommentText = "Comment from workspace 1";
            }

            // Ensure that the process units are on the drawing canvases
            Assert.IsTrue(puCount == w1.DrawingCanvasReference.CountChildrenOfType(typeof(Mixer)),
                "FAIL: Process units were NOT successfully added to drawing canvas 1");

            // Save the first workspace to a memory stream
            MemoryStream ms = new MemoryStream();
            mp1.SaveChemProVFile(ms);
            ms.Position = 0;

            // Have the second MainPage load it
            mp2.LoadChemProVFile(ms);

            // Make sure the load worked
            Assert.IsTrue(puCount == w2.DrawingCanvasReference.CountChildrenOfType(typeof(Mixer)),
                "FAIL: Process units were NOT successfully loaded to drawing canvas 2");

            // Delete all comments from workspace 2
            for (int i = 0; i < w2.DrawingCanvasReference.Children.Count; i++)
            {
                if (w2.DrawingCanvasReference.Children[i] is StickyNote)
                {
                    Core.DrawingCanvasCommands.DeleteElement(w2.DrawingCanvasReference, 
                        w2.DrawingCanvasReference.Children[i]);
                    i--;
                }
            }

            // Make sure all of the sticky notes are gone
            Assert.IsTrue(0 == w2.DrawingCanvasReference.CountChildrenOfType(typeof(StickyNote)),
                "FAIL: Drawing canvas from workspace 2 did not have all its comments deleted.");

            // Now add 1 comment to each process unit in workspace 2's drawing canvas
            for (int i = 0; i < w2.DrawingCanvasReference.Children.Count; i++)
            {
                LabeledProcessUnit lpu = w2.DrawingCanvasReference.Children[i] as LabeledProcessUnit;
                if (null != lpu)
                {
                    StickyNote sn2;
                    StickyNote.CreateCommentNote(w2.DrawingCanvasReference, lpu, null, out sn2);
                    sn2.CommentText = "Comment from workspace 2";
                }
            }

            // Save ws2 and then load to XDocument
            ms = new MemoryStream();
            mp2.SaveChemProVFile(ms);
            ms.Position = 0;
            XDocument doc = XDocument.Load(ms);
            Assert.IsNotNull(doc, "FAIL: XDocument loaded from memory stream is null");

            // Have the first workspace merge in comments from the second
            string userNameIfNotInXml = "who_cares";
            w1.DrawingCanvasReference.MergeCommentsFrom(doc.Element("ProcessFlowDiagram").Element("DrawingCanvas"),
                userNameIfNotInXml);

            // Dispose stream before validation
            ms.Dispose();

            // ---- Validation ----
            // Start by ensuring that there are still the same number of process units
            Assert.IsTrue(puCount == w1.DrawingCanvasReference.CountChildrenOfType(typeof(Mixer)),
                "FAIL: Comment merge changed the number of process units in the document");

            // This is a primitive test, but just make sure each one has two comments on it
            foreach (UIElement uie in w1.DrawingCanvasReference.Children)
            {
                if (!(uie is Mixer))
                {
                    continue;
                }

                Mixer mixer = uie as Mixer;
                Assert.IsTrue(2 == mixer.CommentCount, "FAIL: Process unit does not have 2 comments. It "+
                    "should have one from it's orginal state and another from the merge, but it has " +
                    mixer.CommentCount + " comments.");
            }
        }
    }
}
