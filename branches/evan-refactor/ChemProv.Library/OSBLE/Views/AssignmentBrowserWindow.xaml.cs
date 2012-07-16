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

namespace ChemProV.Library.OSBLE.Views
{
    public partial class AssignmentBrowserWindow : ChildWindow
    {
        private bool m_saveMode;

        /// <summary>
        /// Specifies whether we show all assignments and all courses or just ones with ChemProV 
        /// deliverables
        /// </summary>
        private bool m_showAll = false;
        
        private OSBLEState m_state = null;

        private SolidColorBrush s_lightGreen = new SolidColorBrush(Color.FromArgb(255, 210, 255, 210));
        
        public AssignmentBrowserWindow(OSBLEState state, bool showLoginMessage, bool saveMode)
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
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open the file associated with the selected assignment
            
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            OKButton.IsEnabled = (null != MainTreeView.SelectedItem &&
                null != (MainTreeView.SelectedItem as TreeViewItem).Tag);
        }

        private void UpdateTree()
        {
            MainTreeView.Items.Clear();

            if (m_showAll)
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
                foreach (OsbleService.Course course in m_state.Courses)
                {
                    TreeViewItem parentItem = new TreeViewItem()
                    {
                        Header = course.Name
                    };

                    if (0 == course.Assignments.Count)
                    {
                        TreeViewItem tvi = new TreeViewItem()
                        {
                            Header = "(no assignments found for this course)"
                        };
                        tvi.Background = new SolidColorBrush(Color.FromArgb(255, 255, 210, 210));

                        // Add it to the parent item
                        parentItem.Items.Add(tvi);
                    }
                    else
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
                                    // Add a visual indication that there's something worthwhile here
                                    tvi.Background = s_lightGreen;

                                    // Put the file name in the text after the assignment name
                                    tvi.Header = string.Format("{0} ({1}.cpml)",
                                        assignment.AssignmentName, d.Name);

                                    break;
                                }
                            }

                            // Add it to the parent item
                            parentItem.Items.Add(tvi);
                        }
                    }

                    // Add the fully built parent item to the tree
                    MainTreeView.Items.Add(parentItem);
                }
            }
            else
            {
                foreach (OSBLEState.RelevantAssignment ra in m_state.RelevantAssignments)
                {
                    TreeViewItem tvi = new TreeViewItem()
                    {
                        Header = ra.ToString(),
                        Tag = ra.Assignment
                    };
                    MainTreeView.Items.Add(tvi);
                }
            }
        }
    }
}

