/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

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
using ChemProV.Library.OSBLE;

namespace ChemProV.UI.OSBLE
{
    public partial class OSBLEOrDiskWindow : ChildWindow
    {
        /// <summary>
        /// True if we are in saving mode, false if we are in opening mode
        /// </summary>        
        private bool m_saveMode;

        private OSBLEState m_state;
        
        public OSBLEOrDiskWindow(OSBLEState state, bool isSaving = false)
        {
            InitializeComponent();

            m_state = state;
            m_saveMode = isSaving;

            OSBLECurrentAssignmentButton.Visibility = System.Windows.Visibility.Collapsed;
            LayoutRoot.RowDefinitions[0].Height = new GridLength(0.0);

            if (isSaving)
            {
                Title = "Save To...";
                OSBLEButton.Content = "Save to an OSBLE assignment...";
                DiskButton.Content = "Save to disk...";

                if (null != state.CurrentAssignment)
                {
                    // Build some components for nice info formatting within the button
                    StackPanel sp = new StackPanel();
                    sp.Children.Add(new TextBlock()
                    {
                        Text = "Save to current OSBLE assignment:",
                        FontWeight = FontWeights.Bold
                    });
                    sp.Children.Add(new TextBlock()
                    {
                        Text = state.CurrentAssignment.Name
                    });
                    sp.Children.Add(new TextBlock()
                    {
                        Text = "Course: " + state.CurrentAssignment.CourseName
                    });
                    OSBLECurrentAssignmentButton.Content = sp;
                    
                    LayoutRoot.RowDefinitions[0].Height = GridLength.Auto;
                    OSBLECurrentAssignmentButton.Visibility = System.Windows.Visibility.Visible;
                }
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
            AssignmentChooserWindow abw = new AssignmentChooserWindow(
                Core.App.OSBLEState, false, m_saveMode);
            abw.Show();
        }

        private void OSBLECurrentAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            // First save the workspace to a memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Core.App.Workspace.DrawingCanvas.GetWorkspace().Save(ms);

            // Tell the OSBLE state to save
            m_state.OnSaveComplete -= this.OsbleSaveComplete;
            m_state.OnSaveComplete += this.OsbleSaveComplete;
            m_state.SaveCurrentAssignmentAsync(ms.ToArray());

            // While it's saving we want a progress bar
            this.Height = 100.0;
            LayoutRoot.RowDefinitions.Clear();
            LayoutRoot.RowDefinitions.Add(new RowDefinition());
            ProgressBar pb = new ProgressBar();
            pb.IsIndeterminate = true;
            LayoutRoot.Children.Add(pb);
            pb.SetValue(Grid.RowProperty, 0);
            pb.SetValue(Grid.ColumnProperty, 0);
        }

        private void OsbleSaveComplete(object sender, EventArgs e)
        {
            this.DialogResult = true;
            
            OSBLEStateEventArgs osea = e as OSBLEStateEventArgs;
            
            // Make sure we do a state refresh
            (sender as OSBLEState).RefreshAsync();
            
            if (osea.Success)
            {
                MessageBox.Show("Save complete");
            }
            else
            {
                MessageBox.Show("The save operation could not be completed. It is recommended that you " +
                    "either try again or save your work to disk and then upload it to OSBLE through the " +
                    "web interface.");
            }
        }

        public event EventHandler OnChooseDiskOption = delegate { };
    }
}

