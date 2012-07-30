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
using ChemProV.Logic.OSBLE;
using ChemProV.Library.OsbleService;

namespace ChemProV.UI.OSBLE
{
    public partial class AssignmentChooserWindow : ChildWindow
    {
        private int m_refreshRemaining = 0;
        
        private bool m_saveMode;

        private OSBLEState m_state = null;

        private SolidColorBrush s_lightGreen = new SolidColorBrush(Color.FromArgb(255, 210, 255, 210));

        public AssignmentChooserWindow(OSBLEState state, bool showLoginMessage, bool saveMode)
        {
            InitializeComponent();

            m_saveMode = saveMode;
            if (saveMode)
            {
                OKButton.Content = "Save";
            }

            // Hide the "just-logged-in" message if need be
            if (!showLoginMessage)
            {
                JustLoggedInMsg.Visibility = System.Windows.Visibility.Collapsed;
            }

            m_state = state;
            if (null != m_state)
            {
                UpdateTree();

                m_state.OnDownloadComplete -= this.State_OnDownloadCompleteCrossThread;
                m_state.OnDownloadComplete += this.State_OnDownloadCompleteCrossThread;
            }
        }

        private void AssignmentGetComplete(object sender, EventArgs e)
        {
            RelevantAssignment.RelevantAssignmentEventArgs args =
                e as RelevantAssignment.RelevantAssignmentEventArgs;
            RelevantAssignment ra = sender as RelevantAssignment;

            // Start with the node for the course/assignment
            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = ra.CourseName + " : " + ra.Name;
            tvi.Tag = ra;

            // Now make child nodes for each file in the assignment
            foreach (RelevantAssignment.AssignmentStream stream in args.Files)
            {
                TreeViewItem tviChild = new TreeViewItem();
                tviChild.Header = stream.Name;
                tviChild.Tag = stream;
                tvi.Items.Add(tviChild);
            }
            tvi.IsExpanded = true;

            MainTreeView.Items.Add(tvi);

            if (0 == System.Threading.Interlocked.Decrement(ref m_refreshRemaining))
            {
                UpdateComplete();
            }
        }

        /// <summary>
        /// Called when the files for a relevant assignment have been updated and we can add a node to the 
        /// tree. It is (potentially) called from another thread so we need to call an update method on the 
        /// UI thread.
        /// </summary>
        private void AssignmentGetCompleteCrossThread(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new EventHandler(this.AssignmentGetComplete), sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != m_state)
            {
                // Detach listener and set the state reference to null
                m_state.OnDownloadComplete -= this.State_OnDownloadCompleteCrossThread;
                m_state = null;
            }
            
            this.DialogResult = false;
        }

        private void ChildWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != m_state)
            {
                // Detach listener and set the state reference to null
                m_state.OnDownloadComplete -= this.State_OnDownloadCompleteCrossThread;
                m_state = null;
            }
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem tvi = MainTreeView.SelectedItem as TreeViewItem;
            OKButton.IsEnabled = (null != tvi && null != tvi.Tag);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't do anything if there's nothing selected in the tree
            if (null == MainTreeView.SelectedItem || 
                null == (MainTreeView.SelectedItem as TreeViewItem).Tag)
            {
                return;
            }

            // Hide the buttons and show the progress bar
            OKButton.Visibility = System.Windows.Visibility.Collapsed;
            CancelButton.Visibility = System.Windows.Visibility.Collapsed;
            MainProgressBar.Visibility = System.Windows.Visibility.Visible;

            // Get a reference to the workspace
            Logic.Workspace ws = Core.App.Workspace.DrawingCanvasReference.GetWorkspace();

            // Get the selected tree view item
            TreeViewItem tvi = MainTreeView.SelectedItem as TreeViewItem;

            if (m_saveMode)
            {
                // Setup callback for save completion
                m_state.OnSaveComplete -= this.State_OnSaveComplete;
                m_state.OnSaveComplete += this.State_OnSaveComplete;

                RelevantAssignment.AssignmentStream assignmentObj =
                    tvi.Tag as RelevantAssignment.AssignmentStream;
                if (null == assignmentObj)
                {
                    m_state.CurrentAssignment = (tvi.Tag as RelevantAssignment);
                    
                    // If the selected item is the actual course item and not an assignment file beneath it, then 
                    // we can only save if there are 0 or 1 files. Otherwise it's ambiguous what the user wants 
                    // to save and we need to return.
                    if (0 == tvi.Items.Count)
                    {
                        (tvi.Tag as RelevantAssignment).SaveAsync(null, ws, this.SaveCompleteCrossThread);
                    }
                    else if (1 == tvi.Items.Count)
                    {
                        (tvi.Tag as RelevantAssignment).SaveAsync(
                            (tvi.Items[0] as TreeViewItem).Tag as RelevantAssignment.AssignmentStream,
                            ws, this.SaveCompleteCrossThread);
                    }
                    else
                    {
                        // Show the buttons and hide the progress bar
                        OKButton.Visibility = System.Windows.Visibility.Visible;
                        CancelButton.Visibility = System.Windows.Visibility.Visible;
                        MainProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                        
                        MessageBox.Show("You have selected a assignment that has multiple files within it. Please " +
                            "select a specific file to save to.");
                    }
                    
                    return;
                }
                assignmentObj.Parent.SaveAsync(assignmentObj, ws, this.SaveCompleteCrossThread);
                m_state.CurrentAssignment = assignmentObj.Parent;

                //RelevantAssignment a = (MainTreeView.SelectedItem as TreeViewItem).Tag as RelevantAssignment;
                //if (null == a)
                //{
                //    a = ((MainTreeView.SelectedItem as TreeViewItem).Tag as RelevantAssignment.AssignmentStream).Parent;
                //}

                //System.IO.MemoryStream ms = new System.IO.MemoryStream();
                //ws.Save(ms);
                //byte[] data = ms.ToArray();
                //ms.Dispose();
                //m_state.SaveAssignmentAsync(a, data);
            }
            else
            {
                // Get the assignment stream for the selected item
                RelevantAssignment.AssignmentStream ras = (MainTreeView.SelectedItem as TreeViewItem).Tag as
                    RelevantAssignment.AssignmentStream;
                if (null != ras)
                {
                    m_state.CurrentAssignment = ras.Parent;
                    if (0 == ras.Length)
                    {
                        MessageBox.Show("Assignment has yet to be submitted. You may save your work to OSBLE to " +
                            "submit the first version for this assignment.");
                    }
                    else
                    {
                        try
                        {
                            ws.Load(ras);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("File could not be loaded. It is recommended that you try downloading " + 
                                "the file from the OSBLE web interface.");
                        }
                    }
                }
                else
                {
                    RelevantAssignment ra = (MainTreeView.SelectedItem as TreeViewItem).Tag as RelevantAssignment;
                    m_state.CurrentAssignment = ra;
                    if (0 == (MainTreeView.SelectedItem as TreeViewItem).Items.Count)
                    {
                        MessageBox.Show("Assignment has yet to be submitted. You may save your work to OSBLE to " +
                            "submit the first version for this assignment.");
                    }
                    // Else we ignore it because they have to choose a child item
                }
                this.DialogResult = true;
            }
        }

        private void SaveComplete(object sender, EventArgs e)
        {
            this.DialogResult = true;
            
            RelevantAssignment.SaveEventArgs sea = e as RelevantAssignment.SaveEventArgs;
            if (sea.Success)
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

        private void SaveCompleteCrossThread(object sender, EventArgs e)
        {
            // Invoke on UI thread
            Dispatcher.BeginInvoke(new EventHandler(this.SaveComplete), sender, e);
        }

        private void State_OnDownloadComplete(object sender, EventArgs e)
        {
            OSBLEStateEventArgs osea = e as OSBLEStateEventArgs;
            if (!osea.Success)
            {
                // Show/hide interface components
                MainProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                OKButton.Visibility = System.Windows.Visibility.Visible;
                CancelButton.Visibility = System.Windows.Visibility.Visible;

                // Display an error message
                MessageBox.Show(osea.Message);
            }
            else
            {
                // If the stream is null then show the message
                if (null == osea.Stream)
                {
                    Core.App.Workspace.DrawingCanvasReference.GetWorkspace().Clear();
                    MessageBox.Show(osea.Message);
                }
                else
                {
                    Core.App.Workspace.DrawingCanvasReference.GetWorkspace().Load(osea.Stream);
                }

                // Dettach listener and set the state reference to null
                m_state.OnDownloadComplete -= this.State_OnDownloadCompleteCrossThread;
                m_state = null;

                this.DialogResult = true;
            }
        }

        private void State_OnDownloadCompleteCrossThread(object sender, EventArgs e)
        {
            // Invoke on UI thread
            Dispatcher.BeginInvoke(new EventHandler(this.State_OnDownloadComplete), sender, e);
        }

        private void State_OnSaveComplete(object sender, EventArgs e)
        {
            // Remove the event listener
            m_state.OnSaveComplete -= this.State_OnSaveComplete;
            
            this.DialogResult = true;
            
            OSBLEStateEventArgs osea = e as OSBLEStateEventArgs;
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

        private void UpdateComplete()
        {
            MainProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            OKButton.Visibility = System.Windows.Visibility.Visible;
            CancelButton.Visibility = System.Windows.Visibility.Visible;
            MainTreeView.IsEnabled = true;
        }

        private void UpdateTree()
        {
            MainTreeView.Items.Clear();

            // Special case if there are no relevant assignments
            if (0 == m_state.RelevantAssignments.Count)
            {
                TreeViewItem tvi = new TreeViewItem()
                {
                    Header = "(no courses found)"
                };
                MainTreeView.Items.Add(tvi);
                return;
            }

            // Disable the tree view until it is completely built
            MainTreeView.IsEnabled = false;

            // Hide buttons and show progress bar while we are refreshing
            OKButton.Visibility = System.Windows.Visibility.Collapsed;
            CancelButton.Visibility = System.Windows.Visibility.Collapsed;
            MainProgressBar.Visibility = System.Windows.Visibility.Visible;

            m_refreshRemaining = m_state.RelevantAssignments.Count;

            // Tell each assignment to get its files
            foreach (RelevantAssignment ra in m_state.RelevantAssignments)
            {
                ra.GetFilesAsync(this.AssignmentGetCompleteCrossThread);
            }

            //// Add a node for each course
            //foreach (Course course in m_state.Courses)
            //{
            //    TreeViewItem parentItem = new TreeViewItem()
            //    {
            //        Header = course.Name
            //    };

            //    if (0 != course.Assignments.Count)
            //    {
            //        // Add child nodes for each assignment
            //        foreach (Assignment assignment in course.Assignments)
            //        {
            //            // See if it's a discussion assignment on a ChemProV document
            //            if (AssignmentTypes.CriticalReview == assignment.Type)
            //            {
            //                TreeViewItem tvi = new TreeViewItem();
            //                tvi.Tag = assignment;

            //                // TODO
            //            }
            //            else
            //            {
            //                // See if it has ChemProV deliverables
            //                foreach (Deliverable d in assignment.Deliverables)
            //                {
            //                    if (DeliverableType.ChemProV == d.DeliverableType)
            //                    {
            //                        TreeViewItem tvi = new TreeViewItem();
            //                        tvi.Tag = assignment;

            //                        // Put the file name in the text after the assignment name
            //                        tvi.Header = string.Format("{0} ({1}.cpml)",
            //                            assignment.AssignmentName, d.Name);

            //                        // Add it to the parent item
            //                        parentItem.Items.Add(tvi);

            //                        break;
            //                    }
            //                }
            //            }
            //        }

            //        // Add the fully built parent item to the tree
            //        MainTreeView.Items.Add(parentItem);
            //    }
            //}
        }
    }
}

