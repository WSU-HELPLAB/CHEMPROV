using System;
using System.Net;
using System.Text;
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

        [TestMethod]
        public void TestOSBLELoginEncDec()
        {
            Random rand = new Random();
            
            // Outer loop is for string lengths in the range [1, 50]
            for (int i = 1; i <= 50; i++)
            {
                // Inner loop is for the number of random tests for this length
                for (int j = 0; j < 10; j++)
                {
                    string s = BuildRandomString(i, rand);
                    byte[] enc = ChemProV.Library.OSBLE.Views.LoginWindow.Enc(s);
                    string s2 = ChemProV.Library.OSBLE.Views.LoginWindow.Dec(enc);
                    Assert.IsTrue(s.Equals(s2),
                        "Encryption/decryption failed on string: " + s);
                }
            }
        }

        private static string BuildRandomString(int length, Random rand)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append((char)(rand.Next(255 - 32) + 32));
            }
            return sb.ToString();
        }
    }
}