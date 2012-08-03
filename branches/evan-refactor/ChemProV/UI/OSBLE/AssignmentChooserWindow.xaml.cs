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
        /// <summary>
        /// Dictionary that maps a course name string to a parent node in the tree
        /// </summary>
        private Dictionary<string, TreeViewItem> m_courseNodes = new Dictionary<string, TreeViewItem>();
        
        private int m_refreshRemaining = 0;
        
        private bool m_saveMode;

        private OSBLEState m_state = null;

#if DEBUG
        private static string s_URLPrefix = "http://localhost:17532";
#else
        private static string s_URLPrefix = "https://www.osble.org";
#endif

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

        /// <summary>
        /// Must be invoked on the UI thread.
        /// </summary>
        private void AssignmentGetComplete(object sender, EventArgs e)
        {            
            RelevantAssignment.RelevantAssignmentEventArgs args =
                e as RelevantAssignment.RelevantAssignmentEventArgs;
            RelevantAssignment ra = sender as RelevantAssignment;

            // In earlier versions I tried out not having a parent node for each course but
            //  1. The user should see what course the assignments belong to
            //  2. Having to put the course name in each individual assignment node made it looked cluttered
            // So each course gets its own parent node. Under this node will be assignment nodes and under 
            // the assignment nodes will be assignment files (and maybe some other items relevant to that 
            // assignment)
            TreeViewItem courseNode;
            if (!m_courseNodes.ContainsKey(ra.CourseName))
            {
                courseNode = new TreeViewItem()
                {
                    Header = ra.CourseName
                };
                MainTreeView.Items.Add(courseNode);
                m_courseNodes.Add(ra.CourseName, courseNode);
                
                // We want all nodes expanded by default
                courseNode.IsExpanded = true;
            }
            else
            {
                courseNode = m_courseNodes[ra.CourseName];
            }

            // Critical review discussions are a special case because they don't actually contain any 
            // files that we can open. Instead, we want to give a link to the relevant OSBLE page. We 
            // only show them in open mode (they are hidden in save mode).
            if (!m_saveMode && AssignmentTypes.CriticalReviewDiscussion == ra.ActualAssignment.Type)
            {
                StackPanel sp = new StackPanel();
                TextBlock tb = new TextBlock();
                tb.Text = string.Format("{0}\nType: Critical Review Discussion\nDue Date: {1}",
                    ra.Name, ra.ActualAssignment.DueDate.ToString("f"));
                sp.Children.Add(tb);
                HyperlinkButton hb = new HyperlinkButton();
                string url = string.Format(
                    "{0}/Account/TokenLogin?authToken={1}&destinationUrl=/AssignmentDetails/{2}",
                    s_URLPrefix, ra.LastAuthToken, ra.ActualAssignment.ID.ToString());
                hb.NavigateUri = new Uri(url);
                hb.Content = "Go to this assignment in OSBLE";
                sp.Children.Add(hb);

                // Every assignment node must have the assignment object as its tag
                sp.Tag = ra;

                // Insert the assignment item in sorted position by due date
                InsertAssignmentNodeSorted(courseNode, sp, ra.ActualAssignment.DueDate);
            }
            else
            {
                // Build the node for the assignment
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = string.Format("{0}\nType: {1}\nDue Date: {2}",
                    ra.Name, (AssignmentTypes.CriticalReview == ra.ActualAssignment.Type) ?
                        "Critical Review" : ra.ActualAssignment.Type.ToString(),
                    ra.ActualAssignment.DueDate.ToString("f"));
                tvi.Tag = ra;
                tvi.IsExpanded = true;

                // Put it under the course node, in sorted order by due date
                InsertAssignmentNodeSorted(courseNode, tvi, ra.ActualAssignment.DueDate);

                // Now make child nodes for each file in the assignment
                foreach (RelevantAssignment.AssignmentStream stream in args.Files)
                {
                    // If the assignment is a critical review then it can potentially have both original 
                    // documents and reviewed documents as streams within it. In the case where we're 
                    // loading, we want to show all of these.
                    // However, in the case where we're saving we cannot overwrite the originals. Rather, 
                    // we can only save reviews. So for that save case we should show only originals and 
                    // label them in a way that implies that a save submits a review for that document.
                    if (!m_saveMode)
                    {
                        TreeViewItem tviChild = new TreeViewItem();
                        tviChild.Header = stream.Name;
                        if (ra.IsCriticalReview)
                        {
                            // Mark files as originals or reviews
                            if (stream.IsOriginalForReview)
                            {
                                tviChild.Header += " (author's original file)";
                            }
                            else
                            {
                                tviChild.Header += " (your review file)";
                            }
                        }
                        tviChild.Tag = stream;
                        tvi.Items.Add(tviChild);
                    }
                    else if (stream.IsOriginalForReview)
                    {
                        TreeViewItem tviChild = new TreeViewItem();
                        tviChild.Header = "Save as review for " + stream.AuthorName;
                        tviChild.Tag = stream;
                        tvi.Items.Add(tviChild);
                    }
                }
            }

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

        /// <summary>
        /// Inserts a child item under the tree view node in sorted order by assignment due date. All 
        /// children currently under the parent node must have a RelevantAssignment object in their tag 
        /// that can be used to get the assignment due date. Also, all existing children under the 
        /// parent must in sorted order.
        /// </summary>
        private void InsertAssignmentNodeSorted(TreeViewItem courseParent, object newItem, DateTime dueDate)
        {            
            if (0 == courseParent.Items.Count)
            {
                courseParent.Items.Add(newItem);
                return;
            }

            int index = 0;
            while (index < courseParent.Items.Count)
            {
                FrameworkElement fe = courseParent.Items[index] as FrameworkElement;
                DateTime due = (fe.Tag as RelevantAssignment).ActualAssignment.DueDate;

                // If the due date of the assignment at 'index' is greater than the due date of the 
                // one that we want to insert, then insert at this index.
                if (due > dueDate)
                {
                    break;
                }

                index++;
            }

            courseParent.Items.Insert(index, newItem);
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem tvi = MainTreeView.SelectedItem as TreeViewItem;
            OKButton.IsEnabled = (null != tvi && null != tvi.Tag);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Route the call based on whether we are loading or saving
            
            if (m_saveMode)
            {
                OKButton_Click_Save(sender, e);
            }
            else
            {
                OKButton_Click_Open(sender, e);
            }
        }

        private void OKButton_Click_Open(object sender, RoutedEventArgs e)
        {
            // Don't do anything if there's nothing selected in the tree
            if (null == MainTreeView.SelectedItem ||
                null == (MainTreeView.SelectedItem as Control).Tag)
            {
                return;
            }

            // Get a reference to the workspace
            Logic.Workspace ws = Core.App.Workspace.DrawingCanvasReference.GetWorkspace();

            // Get the selected tree view item
            Control selectedItem = MainTreeView.SelectedItem as Control;

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
                    // Hide the buttons and show the progress bar
                    OKButton.Visibility = System.Windows.Visibility.Collapsed;
                    CancelButton.Visibility = System.Windows.Visibility.Collapsed;
                    MainProgressBar.Visibility = System.Windows.Visibility.Visible;
                    
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

                this.DialogResult = true;
            }
            else
            {
                RelevantAssignment ra = (MainTreeView.SelectedItem as TreeViewItem).Tag as RelevantAssignment;

                // If there's not an assignment associated with this node then we just ignore
                if (null == ra)
                {
                    return;
                }

                if (0 == (MainTreeView.SelectedItem as TreeViewItem).Items.Count)
                {
                    if (AssignmentTypes.Basic == ra.ActualAssignment.Type)
                    {
                        m_state.CurrentAssignment = ra;
                        
                        // 0 files for a basic assignment means the user has yet to submit anything
                        MessageBox.Show("Assignment has yet to be submitted. You may save your work to OSBLE to " +
                            "submit the first version for this assignment.");
                    }
                    else if (AssignmentTypes.CriticalReview == ra.ActualAssignment.Type)
                    {
                        // 0 files for a critical review means that no one has submitted files to review
                        MessageBox.Show("You have selected a critical review assignment for which there are " + 
                            "currently no files available. Files for this assignment will become available " +
                            "after the individuals who you are assigned to review submit their files.");
                        return;
                    }
                    else
                    {
                        // Else we ignore it because they have to choose a child item
                        return;
                    }
                }
                else if (1 == (MainTreeView.SelectedItem as TreeViewItem).Items.Count)
                {
                    // If there's only one option under this node then just load that
                    MainTreeView.SelectItem((MainTreeView.SelectedItem as TreeViewItem).Items[0]);
                    OKButton_Click_Open(sender, e);
                    return;
                }
                else
                {
                    // Else we ignore it because they have to choose a child item
                    MessageBox.Show("You have selected a assignment that has multiple files within it. Please " +
                        "select a specific file to open.");

                    return;
                }
            }
            this.DialogResult = true;
        }

        private void OKButton_Click_Save(object sender, RoutedEventArgs e)
        {
            // Don't do anything if there's nothing selected in the tree
            if (null == MainTreeView.SelectedItem ||
                null == (MainTreeView.SelectedItem as Control).Tag)
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
            Control item = MainTreeView.SelectedItem as Control;

            // Setup callback for save completion
            m_state.OnSaveComplete -= this.State_OnSaveComplete;
            m_state.OnSaveComplete += this.State_OnSaveComplete;

            RelevantAssignment.AssignmentStream assignmentObj =
                item.Tag as RelevantAssignment.AssignmentStream;
            if (null == assignmentObj)
            {
                m_state.CurrentAssignment = (item.Tag as RelevantAssignment);

                TreeViewItem tvi = item as TreeViewItem;

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
            m_courseNodes.Clear();

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
        }
    }
}

