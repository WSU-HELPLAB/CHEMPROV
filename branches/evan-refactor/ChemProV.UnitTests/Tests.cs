using System.IO;
using System.Collections.Generic;
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
using ChemProV.Logic;
using ChemProV.Logic.Equations;

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
            Workspace ws = mp.GetLogicalWorkspace();

            // Add some equations. Just a reminder that one blank one is added by default
            ws.Equations.Add(new EquationModel()
            {
                Equation = "A*B+C",
            });
            ws.Equations.Add(new EquationModel()
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
        public void TestSaveLoad()
        {
            int i;
            Workspace ws1 = new Workspace();
            Random rand = new Random();

            // Add a random number of process units
            List<int> puIDs = new List<int>();
            int numPU = rand.Next(25);
            for (i = 0; i < numPU; i++)
            {
                AbstractProcessUnit pu = new Mixer();
                ws1.AddProcessUnit(pu);
                puIDs.Add(pu.Id);
            }

            // Add a random number of chemical streams
            int numStreams = rand.Next(10);
            for (i = 0; i < numStreams; i++)
            {
                AbstractStream stream = new ChemicalStream(AbstractStream.GetNextUID());
                ws1.AddStream(stream);

                // Don't forget that the properties table needs to be created separately
                stream.PropertiesTable = new StreamPropertiesTable(stream);

                // 50% chance of connecting a destination (attempting a connect that is)
                if (0 == (rand.Next() % 2))
                {
                    AbstractProcessUnit pu = ws1.ProcessUnits[rand.Next(ws1.ProcessUnitCount)];
                    if (pu.AttachIncomingStream(stream))
                    {
                        stream.Destination = pu;
                    }
                }
            }

            // Save the workspace to a memory stream
            MemoryStream ms = new MemoryStream();
            ws1.Save(ms, "TEST_VersionNA");

            // Load to a new workspace
            Workspace ws2 = new Workspace();
            ws2.Load(ms);

            // Make sure the number of process units and streams match
            Assert.IsTrue(numPU == ws2.ProcessUnitCount,
                "Number of process units between saved document (" + numPU.ToString() +
                ") and loaded document (" + ws2.ProcessUnitCount.ToString() + ") do not match");
            Assert.IsTrue(numStreams == ws2.Streams.Count,
                "Number of streams between saved document (" + numStreams.ToString() + 
                ") and loaded document (" + ws2.Streams.Count.ToString() + ") do not match");

            // Test that the incoming/outgoing streams for process units match
            foreach (AbstractProcessUnit pu1 in ws1.ProcessUnits)
            {
                AbstractProcessUnit pu2 = ws2.GetProcessUnit(pu1.Id);
                Assert.IsNotNull(pu2,
                    "Process unit with ID=" + pu1.Id.ToString() + " from workspace 1 (saved) was not " +
                    "found in workspace 2 (loaded).");

                // For now just compare outoging stream count
                Assert.IsTrue(pu1.OutgoingStreamCount == pu2.OutgoingStreamCount,
                    "Mis-match outgoing stream count");
            }
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

        /// <summary>
        /// Tests the ChemProV.Logic.Expression class.
        /// </summary>
        [TestMethod]
        public void ExpressionEvaluatorTests()
        {
            string errors;
            ChemProV.Logic.Expression exp = ChemProV.Logic.Expression.Create(
                "(-VarA + VarB) * VarC", out errors);
            Assert.IsNotNull(exp, errors);

            // Make sure 3 variables were recognized in the expression
            Assert.IsTrue(3 == exp.CountVariables(),
                "Expression parser did not generate the right number of variables");

            // Do 100 tests with random, non-negative integer values
            Random rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                double a = (double)rand.Next(1001);
                double b = (double)rand.Next(1001);
                double c = (double)rand.Next(1001);
                exp.SetSymbolValue("VarA", a);
                exp.SetSymbolValue("VarB", b);
                exp.SetSymbolValue("VarC", c);
                double eval = exp.Evaluate();
                Assert.IsTrue((-a + b) * c == eval,
                    "Expression evaluator returned " + eval.ToString() + " while compiled code returned " +
                    ((-a + b) * c).ToString() + ". If these values are very close it's probably just a " + 
                    "rounding error and not a bug.");
            }
        }
    }
}