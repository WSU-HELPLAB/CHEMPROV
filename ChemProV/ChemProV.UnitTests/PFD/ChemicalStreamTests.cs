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
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using System.Diagnostics;

namespace ChemProV.UnitTests.PFD
{
    [TestClass]
    public class ChemicalStreamTests
    {
        [TestMethod]
        public void TestMoleUnitChange()
        {
            //create test window, make sure that we started in Mass units
            ChemicalStreamPropertiesWindow window = new ChemicalStreamPropertiesWindow();
            Assert.AreEqual(window.TableName, "M1");

            //switch to moles, make sure that things got changed over
            window.ItemSource[0].Units = (int)Units.Moles - 1;
            Assert.AreEqual(window.TableName, "N1");
        }
    }
}
