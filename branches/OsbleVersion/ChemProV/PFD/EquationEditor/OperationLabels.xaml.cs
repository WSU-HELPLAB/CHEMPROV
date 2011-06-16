/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows.Controls;

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