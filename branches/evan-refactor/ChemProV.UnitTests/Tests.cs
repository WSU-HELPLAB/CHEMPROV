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
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChemProV.UnitTests
{
    [TestClass]
    public class Tests
    {
        /// <summary>
        /// Test to make sure that the equation editor UI properly subscribes to Core.Workspace object 
        /// events and updates the UI as the logical workspace is changed.
        /// </summary>
        [TestMethod]
        public void EquationEditorUIFromWorkspace()
        {
            MainPage mp = new MainPage();

            // Get a reference to the logical workspace. Modifying it should update the interface.
            Core.Workspace ws = mp.GetLogicalWorkspace();

            // Add some equations. Just a reminder that one blank one is added by default
            ws.Equations.Add(new ChemProV.PFD.EquationEditor.Models.EquationModel()
            {
                Equation = "A*B+C",
            });
            ws.Equations.Add(new ChemProV.PFD.EquationEditor.Models.EquationModel()
            {
                Equation = "(X+Y)*Z",
            });

            // Query the number of equation rows in the equation editor. The equation editor counts the 
            // number of controls, which is what we want. That is, we want to make sure it created a 
            // control for each equation that was added to the workspace.
            Assert.IsTrue(
                ws.Equations.Count == mp.WorkspaceReference.EquationEditorReference.EquationRowCount,
                string.Format("FAIL: Expected equation count after addition={0}, actual={1}",
                    ws.Equations.Count, mp.WorkspaceReference.EquationEditorReference.EquationRowCount));

            // Now remove an equation and re-test
            ws.Equations.Remove(ws.Equations[0]);
            Assert.IsTrue(
                ws.Equations.Count == mp.WorkspaceReference.EquationEditorReference.EquationRowCount,
                string.Format("FAIL: Expected equation count after removal={0}, actual={1}",
                    ws.Equations.Count, mp.WorkspaceReference.EquationEditorReference.EquationRowCount));
        }
    }
}