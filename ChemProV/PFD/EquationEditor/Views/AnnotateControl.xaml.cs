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

namespace ChemProV.PFD.EquationEditor.Views
{
    public partial class AnnotateControl : UserControl
    {
        public AnnotateControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AnnotateWindow window = new AnnotateWindow();
            window.DataContext = this.DataContext;
            window.Show();
        }
    }
}
