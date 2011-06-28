/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Windows.Data;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public class TempUnitsFormatter : IValueConverter
    {
        /// <summary>
        /// This converts an index value from the unit dropdown box into a string that it
        /// puts in the table for when the dropdown box is not shown
        /// </summary>
        /// <param name="value">index of currently selected dropdown menu item</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns>a string for the text box to use when dropdown is not visble</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertFromIntToString((int)value);
        }

        /// <summary>
        /// This converts an index value from the unit dropdown box into a string that it puts in the table for when the dropdown box is not shown
        /// </summary>
        /// <param name="value">index of currently selected dropdown menu item</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns>a string for the text box to use when dropdown is not visble</returns>
        public string ConvertFromIntToString(int value)
        {
            switch (value)
            {
                //case -1 is for when row is first created nothing is set yet
                case -1:
                    return "";
                case 0:
                    return "celsius";
                case 1:
                    return "fahrenheit";
            }
            return "";
        }

        /// <summary>
        /// This function is never called but required to be implimented because it is
        /// virtual in IValueConverter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "":
                    return -1;
                case "celsius":
                    return 0;
                case "fahrenheit":
                    return 1;
            }
            return -1;
        }
    }
}