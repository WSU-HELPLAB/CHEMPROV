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
using ChemProV.Logic;

namespace ChemProV.UI
{
    public partial class ChangeStreamNumberWindow : ChildWindow
    {
        private AbstractStream m_stream;

        private Workspace m_workspace;

        public ChangeStreamNumberWindow(AbstractStream stream, Workspace workspace)
        {
            InitializeComponent();

            m_stream = stream;
            m_workspace = workspace;

            if (null != workspace)
            {
                // Make a list of integer options for stream numbers
                List<int> numOpts = new List<int>();
                int max = 50;
                while (0 == numOpts.Count)
                {
                    max *= 2;
                    for (int i = 1; i <= 100; i++)
                    {
                        if (null == workspace.GetStream(i) ||
                            i == stream.Id)
                        {
                            numOpts.Add(i);
                        }
                    }
                }

                // Set them as the options in the combo box
                NumberComboBox.ItemsSource = numOpts;
                NumberComboBox.SelectedItem = stream.Id;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == NumberComboBox.SelectedItem)
            {
                this.DialogResult = false;
                return;
            }

            // Get the selected ID
            int id = Convert.ToInt32(NumberComboBox.SelectedItem);

            // If it's the same then there's nothing to do
            if (m_stream.Id == id)
            {
                this.DialogResult = false;
                return;
            }

            m_workspace.AddUndo(new UndoRedoCollection("Undo changing stream number from " +
                m_stream.Id.ToString() + " to " + id.ToString(),
                new Logic.Undos.SetStreamId(m_stream, m_stream.Id)));
            m_stream.Id = id;
            
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

