using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;

namespace ChemProV.PFD.EquationEditor.Models
{
    public class EquationModifierComparer : IComparer
    {

        public int Compare(object x, object y)
        {
            IEquationModifier left = (IEquationModifier)x;
            IEquationModifier right = (IEquationModifier)y;
            if (left.ClassificationId != right.ClassificationId)
            {
                return left.ClassificationId.CompareTo(right.ClassificationId);
            }
            return left.Name.CompareTo(right.Name);
        }
    }
}
