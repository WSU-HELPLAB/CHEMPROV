/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

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
