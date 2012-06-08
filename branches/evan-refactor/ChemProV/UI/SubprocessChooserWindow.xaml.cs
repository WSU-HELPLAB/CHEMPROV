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
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Undos;

namespace ChemProV.UI
{
    public partial class SubprocessChooserWindow : ChildWindow
    {
        private LabeledProcessUnit m_lpu;
        
        private Core.NamedColor m_nc = new Core.NamedColor(null, Colors.Transparent);
        
        public SubprocessChooserWindow()
            : this(null) { }
        
        public SubprocessChooserWindow(LabeledProcessUnit lpu)
        {
            InitializeComponent();

            // Store a reference to the process unit
            m_lpu = lpu;

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
            Core.App.Workspace.DrawingCanvas.AddUndo(
                new UndoRedoCollection("Undo subprocess change", new SetSubprocess(m_lpu)));

            // Set the new subprocess
            m_lpu.Subprocess = m_nc.Color;
            
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

