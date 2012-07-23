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
using ChemProV.Library.OsbleService;
using ChemProV.Library.OSBLE;

namespace ChemProV.UI.OSBLE
{
    public partial class AssignmentChooserWindow : ChildWindow
    {
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
            OKButton.IsEnabled = (null != MainTreeView.SelectedItem &&
                null != (MainTreeView.SelectedItem as TreeViewItem).Tag);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't do anything if there's nothing selected in the tree
            if (null == MainTreeView.SelectedItem)
            {
                return;
            }

            // Hide the buttons and show the progress bar
            OKButton.Visibility = System.Windows.Visibility.Collapsed;
            CancelButton.Visibility = System.Windows.Visibility.Collapsed;
            MainProgressBar.Visibility = System.Windows.Visibility.Visible;

            Assignment a = (MainTreeView.SelectedItem as TreeViewItem).Tag as Assignment;
            if (m_saveMode)
            {
                // Setup callback for save completion
                m_state.OnSaveComplete -= this.State_OnSaveComplete;
                m_state.OnSaveComplete += this.State_OnSaveComplete;
                
                Logic.Workspace ws = Core.App.Workspace.DrawingCanvasReference.GetWorkspace();
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ws.Save(ms);
                byte[] data = ms.ToArray();
                ms.Dispose();
                m_state.SaveAssignmentAsync(a, data);
            }
            else
            {
                // Open the file associated with the selected assignment
                m_state.GetAssignmentStreamAsync(a);
            }
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

        private void UpdateTree()
        {
            MainTreeView.Items.Clear();

            if (true)
            {
                if (0 == m_state.Courses.Count)
                {
                    TreeViewItem tvi = new TreeViewItem()
                    {
                        Header = "(no courses found)"
                    };
                    MainTreeView.Items.Add(tvi);
                    return;
                }

                // Add a node for each course
                foreach (Course course in m_state.Courses)
                {
                    TreeViewItem parentItem = new TreeViewItem()
                    {
                        Header = course.Name
                    };

                    if (0 != course.Assignments.Count)
                    {
                        // Add child nodes for each assignment
                        foreach (Assignment assignment in course.Assignments)
                        {
                            TreeViewItem tvi = new TreeViewItem()
                            {
                                Header = assignment.AssignmentName
                            };
                            tvi.Tag = assignment;

                            // See if we have ChemProV deliverables
                            foreach (Deliverable d in assignment.Deliverables)
                            {
                                if (DeliverableType.ChemProV == d.DeliverableType)
                                {
                                    // Put the file name in the text after the assignment name
                                    tvi.Header = string.Format("{0} ({1}.cpml)",
                                        assignment.AssignmentName, d.Name);

                                    // Add it to the parent item
                                    parentItem.Items.Add(tvi);

                                    break;
                                }
                            }
                        }

                        // Add the fully built parent item to the tree
                        MainTreeView.Items.Add(parentItem);
                    }
                }
            }
        }
    }
}

