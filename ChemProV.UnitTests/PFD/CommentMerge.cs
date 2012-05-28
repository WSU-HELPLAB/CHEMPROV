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
using ChemProV.PFD.EquationEditor.Models;

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

        [TestMethod]
        public void TestMergeBehindTheScenes()
        {
            // "Behind the scenes" code will eventually exist in OSBLE to do comment merges on uploaded 
            // files. There is some UI-independent logic that exists for this and it is tested here. We 
            // still need to use Silverlight stuff to build the documents, but we can save them to 
            // memory streams and then test the merge logic on those.

            MainPage mp1 = new MainPage();
            MainPage mp2 = new MainPage();

            // Create a process unit on page 1
            LabeledProcessUnit lpu = CreateRandomProcessUnit();
            mp1.WorkspaceReference.DrawingCanvasReference.AddNewChild(lpu);
            // Add some comments to it
            string[] comments = new string[]{
                "This is a test comment #1",
                "This is a second test comment",
                "This is a third test comment"};
            foreach (string commentString in comments)
            {
                StickyNote sn;
                StickyNote.CreateCommentNote(mp1.WorkspaceReference.DrawingCanvasReference,
                    lpu, null, out sn);
                sn.CommentUserName = "mp1User";
                sn.CommentText = commentString;
            }
            // Add 3 equations, 1 and 3 with annotations
            mp1.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
                new EquationType(), new EquationScope(), "Eq1 in both, comments in both", "Comment from MP1");
            mp1.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
                new EquationType(), new EquationScope(), "Eq2 in both, comment from MP2 only", null);
            mp1.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
                new EquationType(), new EquationScope(), "Eq3 in MP1 only", "This comment (annotation) "+
                "is on an equation that is only in MP1. It should remain after the merge.");
            // Save to memory stream
            MemoryStream ms1 = new MemoryStream();
            mp1.SaveChemProVFile(ms1);

            // Use the same process unit, but remove all comments from it and add it to page 2
            mp1.WorkspaceReference.DrawingCanvasReference.RemoveChild(lpu);
            while (lpu.CommentCount > 0)
            {
                lpu.RemoveCommentAt(0);
            }
            Assert.IsTrue(0 == lpu.CommentCount,
                "FAIL: Expected comment count=0, actual=" + lpu.CommentCount.ToString());
            mp2.WorkspaceReference.DrawingCanvasReference.AddNewChild(lpu);
            // Add some new comments to it
            comments = new string[]{
                "This is a test comment #1", // Keep 1 the same...
                // ... and add 2 new
                "This comment that is only in the second document",
                "Another comment only in the second document"};
            foreach (string commentString in comments)
            {
                StickyNote sn;
                StickyNote.CreateCommentNote(mp2.WorkspaceReference.DrawingCanvasReference,
                    lpu, null, out sn);
                sn.CommentUserName = "mp2User";
                sn.CommentText = commentString;
            }
            // Add 2 matching equations, both with annotations
            mp2.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
                new EquationType(), new EquationScope(), "Eq1 in both, comments in both",
                    "Comment from MP2 (should get merged in)");
            mp2.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
                new EquationType(), new EquationScope(), "Eq2 in both, comment from MP2 only",
                    "This comment comes from MP2");
            // Make sure the IDs match up
            mp2.WorkspaceReference.EquationEditorReference.GetEquationModel(0).Id =
                mp1.WorkspaceReference.EquationEditorReference.GetEquationModel(0).Id;
            mp2.WorkspaceReference.EquationEditorReference.GetEquationModel(1).Id =
                mp1.WorkspaceReference.EquationEditorReference.GetEquationModel(1).Id;
            // Save to memory stream
            MemoryStream ms2 = new MemoryStream();
            mp2.SaveChemProVFile(ms2);

            // We now have the two documents saved (to memory). Create a third memory stream and 
            // save the merged version to it
            MemoryStream ms3 = new MemoryStream();
            ChemProV.Core.CommentMerger.Merge(ms1, null, ms2, null, ms3);

            // Dispose some stuff
            mp1.Dispose();
            mp2.Dispose();
            ms1.Dispose();
            ms2.Dispose();

            // Create a third MainPage and load
            MainPage mp3 = new MainPage();
            ms3.Position = 0;
            mp3.LoadChemProVFile(ms3);

            // Get the process unit
            lpu = mp3.WorkspaceReference.DrawingCanvasReference.GetProcessUnitById(lpu.Id);
            Assert.IsNotNull(lpu);

            // It should have 5 comments
            Assert.IsTrue(5 == lpu.CommentCount, 
                "FAIL: Process unit should have 5 comments but it has " + lpu.CommentCount.ToString());

            // It should have 3 non-empty equations
            int eqCount = 0;
            for (int i = 0; i < mp3.WorkspaceReference.EquationEditorReference.EquationRowCount; i++)
            {
                if (!string.IsNullOrEmpty(mp3.WorkspaceReference.EquationEditorReference.GetEquationModel(i).Equation))
                {
                    eqCount++;
                }
            }
            Assert.IsTrue(3 == eqCount,
                "Merged documents should result in 3 non-empty equations, instead there are " +
                eqCount.ToString());
        }

        #region Helper functions

        private static Random s_rand = new Random();
        
        private static LabeledProcessUnit CreateRandomProcessUnit()
        {
            switch (s_rand.Next(4))
            {
                case 0:
                    return new HeatExchangerNoUtility();

                case 1:
                    return new Mixer();

                case 2:
                    return new Reactor();

                default:
                    return new Separator();
            }
        }

        #endregion

        #region Debug functions

        private static string ReadStreamAsString(Stream s)
        {
            s.Position = 0;
            byte[] buf = new byte[s.Length];
            s.Read(buf, 0, buf.Length);
            return System.Text.Encoding.UTF8.GetString(buf, 0, buf.Length);
        }

        #endregion
    }
}
