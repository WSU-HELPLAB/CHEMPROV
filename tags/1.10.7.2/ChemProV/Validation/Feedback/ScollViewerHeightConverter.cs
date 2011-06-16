using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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
