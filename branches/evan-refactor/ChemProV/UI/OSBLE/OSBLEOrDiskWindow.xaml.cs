using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.UI.OSBLE
{
    public partial class OSBLEOrDiskWindow : ChildWindow
    {
        /// <summary>
        /// True if we are in saving mode, false if we are in opening mode
        /// </summary>        
        private bool m_saveMode;
        
        public OSBLEOrDiskWindow(bool isSaving = false)
        {
            InitializeComponent();

            m_saveMode = isSaving;

            if (isSaving)
            {
                Title = "Save To...";
                OSBLEButton.Content = "Save to an OSBLE assignment...";
                DiskButton.Content = "Save to disk...";
            }
        }

        private void DiskButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            OnChooseDiskOption(this, EventArgs.Empty);
        }

        private void OSBLEButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide this window first
            this.DialogResult = true;
            
            // Launch the assignment browser
            ChemProV.Library.OSBLE.Views.AssignmentBrowserWindow abw = new Library.OSBLE.Views.AssignmentBrowserWindow(
                Core.App.OSBLEState, false, m_saveMode);
            abw.Show();
        }

        public event EventHandler OnChooseDiskOption = delegate { };
    }
}

