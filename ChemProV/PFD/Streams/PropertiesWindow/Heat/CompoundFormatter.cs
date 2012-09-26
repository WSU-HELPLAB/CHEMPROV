/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows.Data;

namespace ChemProV.PFD.Streams.PropertiesWindow.Heat
{
    public class CompoundFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertFromIntToString((int)value);
        }

        public string ConvertFromIntToString(int value)
        {
            switch ((int)value)
            {
                case 0:
                    return "acetic acid";
                case 1:
                    return "ammonia";
                case 2:
                    return "benzene";
                case 3:
                    return "carbon dioxide";
                case 4:
                    return "carbon monoxide";
                case 5:
                    return "cyclohexane";
                case 6:
                    return "ethane";
                case 7:
                    return "ethanol";
                case 8:
                    return "ethylene";
                case 30:
                    return "formaldehyde";
                case 9:
                    return "hydrochloric acid";
                case 10:
                    return "hydrogen";
                case 11:
                    return "methane";
                case 12:
                    return "methanol";
                case 13:
                    return "n-butane";
                case 14:
                    return "n-hexane";
                case 15:
                    return "n-octane";
                case 16:
                    return "nitrogen";
                case 17:
                    return "oxygen";
                case 18:
                    return "phosphoric acid";
                case 29:
                    return "propane";
                case 20:
                    return "sodium hydroxide";
                case 21:
                    return "sulfuric acid";
                case 22:
                    return "toluene";
                case 23:
                    return "water";
                case 24:
                    return "Select";
                case 25:
                    return "Overall";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "acetic acid":
                    return 0;
                case "ammonia":
                    return 1;
                case "benzene":
                    return 2;
                case "carbon dioxide":
                    return 3;
                case "carbon monoxide":
                    return 4;
                case "cyclohexane":
                    return 5;
                case "ethane":
                    return 6;
                case "ethanol":
                    return 7;
                case "ethylene":
                    return 8;
                case "formaldehyde":
                    return 30;
                case "hydrochloric acid":
                    return 9;
                case "hydrogen":
                    return 10;
                case "methane":
                    return 11;
                case "methanol":
                    return 12;
                case "n-butane":
                    return 13;
                case "n-hexane":
                    return 14;
                case "n-octane":
                    return 15;
                case "nitrogren":
                    return 16;
                case "oxygen":
                    return 17;
                case "phosphoric acid":
                    return 18;
                case "propane":
                    return 29;
                case "sodium hydroxide":
                    return 20;
                case "sulfuric acid":
                    return 21;
                case "toluene":
                    return 22;
                case "water":
                    return 23;
                case "Select":
                    return 0;
                case "Overall":
                    return 0;

                default:
                    return 0;
            }
        }
    }
}