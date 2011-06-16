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

namespace ChemProV.PFD.EquationEditor
{
    public partial class OperationLabels : UserControl
    {
        public static int NumberCreated;
        public OperationLabels()
        {
            InitializeComponent();
            switch (NumberCreated)
            {
                case 0: Operation.Content = " = "; break;
                case 1: Operation.Content = " + "; break;
                case 2: Operation.Content = " - "; break;
                case 3: Operation.Content = " * "; break;
                case 4: Operation.Content = " ^ "; break;
                case 5: Operation.Content = " ( "; break;
                case 6: Operation.Content = " ) "; break;
            }
            NumberCreated++;
        }
    }
}
