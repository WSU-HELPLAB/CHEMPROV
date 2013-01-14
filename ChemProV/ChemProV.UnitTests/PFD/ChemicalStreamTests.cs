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
using System.Diagnostics;
using ChemProV.Core;
using ChemProV.Logic;

namespace ChemProV.UnitTests.PFD
{
    [TestClass]
    public class ChemicalStreamTests
    {
        [TestMethod]
        public void TestMoleUnitChange()
        {
            MainPage mp1 = new MainPage();
            Workspace w = mp1.GetLogicalWorkspace();

            // Add a new stream with a table to the workspace
            ChemicalStream cs1 = new ChemicalStream(1);
            cs1.PropertiesTable = new StreamPropertiesTable(cs1);
            w.AddStream(cs1);

            // The DrawingCanvas control should have added a stream control
            ChemProV.PFD.Streams.StreamControl streamControl =
                mp1.WorkspaceReference.DrawingCanvasReference.GetStreamControl(cs1);
            Assert.IsNotNull(streamControl, "DrawingCanvas control did not correctly create a " +
                "stream control for a stream that was added to the workspace.");

            // From the stream control we can get the properties window control
            UI.StreamTableControl props = streamControl.Table;
            Assert.IsNotNull(props, "Stream control had a null table control.");

            // Add a default data row with label "M1" if it's not already there
            if (0 == cs1.PropertiesTable.RowCount)
            {
                ChemicalStreamData csd = cs1.PropertiesTable.AddNewRow() as ChemicalStreamData;
                csd.SelectedCompound = "Overall";
                csd.Label = "M1";
                csd.UserHasRenamed = false;
            }

            // Find the text box in the properties window with the text "M1"
            TextBox tbLabel = props.GetControl(cs1.PropertiesTable.Rows[0], "Label") as TextBox;
            Assert.IsNotNull(tbLabel, "Could not find text box for row label. If the code has changed " +
                "such that there is no longer a default row in chemical streams tables or the default " +
                "row has different units, then this test needs to be altered.");
            Assert.IsTrue(tbLabel.Text.Equals("M1"), "Default label was not M1");

            // Find the combo box for the units
            ComboBox cbUnits = props.GetControl(cs1.PropertiesTable.Rows[0], "SelectedUnits") as ComboBox;
            Assert.IsNotNull(cbUnits, "Could not find combo box control for selected units");
            
            // Select mole %, which should change the label from M1 to N1
            cbUnits.SelectedItem = "mol %";

            // Verify that the label changed to "N1"
            Assert.IsTrue(tbLabel.Text.Equals("N1"), "Test Failed: After unit change, the label " +
                "did not change from M1 to N1");
            
            // What would be nice to add to this test in the future:
            // Change the text in tbLabel which simulates the user manually renaming the row. Change it to 
            // something like "nn1". Then change the units again to something like fractions, which would 
            // normally change 'n' to 'x', but shouldn't after a manual rename.
        }
    }
}
