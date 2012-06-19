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
        public void TestMergeBehindTheScenes()
        {
            // "Behind the scenes" code will eventually exist in OSBLE to do comment merges on uploaded 
            // files. There is some UI-independent logic that exists for this and it is tested here. We 
            // still need to use Silverlight stuff to build the documents, but we can save them to 
            // memory streams and then test the merge logic on those.

            MainPage mp1 = new MainPage();
            MainPage mp2 = new MainPage();

            // Get references to the logical workspaces
            Core.Workspace lws1 = mp1.GetLogicalWorkspace();
            Core.Workspace lws2 = mp2.GetLogicalWorkspace();

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
                StickyNoteControl sn;
                StickyNoteControl.CreateCommentNote(mp1.WorkspaceReference.DrawingCanvasReference,
                    lpu, null, out sn);
                sn.CommentUserName = "mp1User";
                sn.CommentText = commentString;
            }
            // Add 3 equations, 1 and 3 with comments
            EquationModel em11 = new EquationModel() {Equation = "Eq1 in both, comments in both"};
            em11.Comments.Add(new Core.BasicComment("Comment from MP1", null));
            lws1.Equations.Add(em11);

            EquationModel em12 = new EquationModel() { Equation = "Eq2 in both, comment from MP2 only" };
            lws1.Equations.Add(em12);

            EquationModel em13 = new EquationModel() { Equation = "Eq3 in MP1 only" };
            em13.Comments.Add(new Core.BasicComment("This comment (annotation) "+
                "is on an equation that is only in MP1. It should remain after the merge.", null));
            lws1.Equations.Add(em13);

            //mp1.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
            //    new EquationType(), new EquationScope(), "Eq1 in both, comments in both", "Comment from MP1");
            //mp1.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
            //    new EquationType(), new EquationScope(), "Eq2 in both, comment from MP2 only", null);
            //mp1.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
            //    new EquationType(), new EquationScope(), "Eq3 in MP1 only", "This comment (annotation) "+
            //    "is on an equation that is only in MP1. It should remain after the merge.");
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
                StickyNoteControl sn;
                StickyNoteControl.CreateCommentNote(mp2.WorkspaceReference.DrawingCanvasReference,
                    lpu, null, out sn);
                sn.CommentUserName = "mp2User";
                sn.CommentText = commentString;
            }
            // Add 2 matching equations, both with comments
            EquationModel em2 = new EquationModel() { Equation = "Eq1 in both, comments in both" };
            em2.Comments.Add(new Core.BasicComment("Comment from MP2 (should get merged in)", null));
            lws2.Equations.Add(em2);
            em2 = new EquationModel() { Equation = "Eq2 in both, comment from MP2 only" };
            em2.Comments.Add(new Core.BasicComment("This comment comes from MP2", null));
            lws2.Equations.Add(em2);

            //mp2.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
            //    new EquationType(), new EquationScope(), "Eq1 in both, comments in both",
            //        "Comment from MP2 (should get merged in)");
            //mp2.WorkspaceReference.EquationEditorReference.AddNewEquationRow(
            //    new EquationType(), new EquationScope(), "Eq2 in both, comment from MP2 only",
            //        "This comment comes from MP2");
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
