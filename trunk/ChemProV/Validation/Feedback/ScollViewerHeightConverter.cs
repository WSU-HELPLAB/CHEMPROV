/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows.Data;

namespace ChemProV.Validation.Feedback
{
    /// <summary>
    /// This is used so the ScollViewerHeigh on the feedback and equation windows cannot go below 33 pixels as that is the
    /// minium hight at which it will still display the arrows
    /// </summary>
    public class ScollViewerHeightConverter : IValueConverter
    {
        /// <summary>
        /// This substracts 33 from the passed value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">not used assumed to be double</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns>value as a double minus 33</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double originalValue = (double)value;
            return originalValue - 33;
        }

        /// <summary>
        /// This adds 33 from the passed value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">not used assumed to be double</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns>value as a double plus 33</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double originalValue = (double)value;
            return originalValue + 33;
        }
    }
}