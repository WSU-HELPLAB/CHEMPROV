using System.Reflection;

/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;

namespace ChemProV.UI
{
    public partial class AboutWindow : ChildWindow
    {
        private string versionNumber = null;

        public AboutWindow()
        {
            InitializeComponent();

            //find our version number
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm.FullName != null)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                versionNumber = assemblyName.Version.ToString();
            }
            ChemProVText.Text = "Version " + versionNumber;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}