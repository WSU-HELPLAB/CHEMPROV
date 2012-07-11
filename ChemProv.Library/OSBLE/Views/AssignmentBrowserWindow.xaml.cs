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
        private OSBLEState m_state = null;

        private SolidColorBrush s_lightGreen = new SolidColorBrush(Color.FromArgb(255, 210, 255, 210));
        
        public AssignmentBrowserWindow(OSBLEState state, bool showLoginMessage)
        {
            InitializeComponent();

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
    }
}

