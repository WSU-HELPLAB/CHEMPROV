/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ChemProV.Logic;
using ChemProV.UI;
using ChemProV.PFD.Undos;

namespace ChemProV.UI
{
    public partial class SubprocessChooserWindow : ChildWindow
    {
        private ProcessUnitControl m_lpu;
        
        private Core.NamedColor m_nc = new Core.NamedColor(null, Colors.Transparent);

        private Workspace m_workspace;
        
        public SubprocessChooserWindow()
            : this(null, null) { }
        
        public SubprocessChooserWindow(ProcessUnitControl lpu, Workspace workspace)
        {
            InitializeComponent();

            // Store a reference to the process unit and workspace
            m_lpu = lpu;
            m_workspace = workspace;

            if (null != lpu)
            {                
                // Initialize the color options
                bool didFirst = false;
                OptionsStackPanel.Children.Clear();
                foreach (Core.NamedColor nc in Core.NamedColors.All)
                {
                    Border b = new Border();
                    b.BorderBrush = new SolidColorBrush(Colors.LightGray);
                    b.CornerRadius = new CornerRadius(3.0);
                    b.BorderThickness = new Thickness(2.0);
                    b.Background = new SolidColorBrush(nc.Color);
                    if (didFirst)
                    {
                        b.Margin = new Thickness(3.0, 0.0, 3.0, 3.0);
                    }
                    else
                    {
                        b.Margin = new Thickness(3.0);
                        didFirst = true;
                    }

                    // Create the radio button to put in the border
                    RadioButton rb = new RadioButton();
                    rb.GroupName = "A";
                    rb.Content = nc.Name;
                    rb.Tag = nc;
                    b.Child = rb;
                    // Setup check-change event
                    rb.Checked += new RoutedEventHandler(rb_Checked);

                    // If the color matches the existing subprocess then check it
                    if (nc.Color.Equals(lpu.Subprocess))
                    {
                        rb.IsChecked = true;
                    }

                    // Add it to the stack panel
                    OptionsStackPanel.Children.Add(b);
                }
            }
        }

        private void rb_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            m_nc = (Core.NamedColor)rb.Tag;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Add the undo item before setting the new subgroup
            m_workspace.AddUndo(
                new UndoRedoCollection("Undo subprocess change", new Logic.Undos.SetSubprocess(m_lpu.ProcessUnit)));

            // Set the new subprocess
            m_lpu.ProcessUnit.Subprocess = m_nc.Color.ToString();
            
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

