/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows.Data;

namespace ChemProV.PFD.EquationEditor
{
    public class ScollViewerHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double originalValue = (double)value;
            if (originalValue < 50)
            {
                return 0;
            }
            return originalValue - 50;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double originalValue = (double)value;
            return originalValue + 50;
        }
    }
}