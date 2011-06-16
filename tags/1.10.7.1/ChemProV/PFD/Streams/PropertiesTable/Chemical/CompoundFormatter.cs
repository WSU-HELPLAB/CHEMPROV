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

namespace ChemProV.PFD.Streams.PropertiesTable.Chemical
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
                    return "Select";
                case 1:
                    return "acetic acid";
                case 2:
                    return "ammonia";
                case 3:
                    return "benzene";
                case 4:
                    return "carbon dioxide";
                case 5:
                    return "carbon monoxide";
                case 6:
                    return "cyclohexane";
                case 7:
                    return "ethane";
                case 8:
                    return "ethanol";
                case 9:
                    return "ethylene";
                case 10:
                    return "hydrochloric acid";
                case 11:
                    return "hydrogen";
                case 12:
                    return "methane";
                case 13:
                    return "methanol";
                case 14:
                    return "n-butane";
                case 15:
                    return "n-hexane";
                case 16:
                    return "n-octane";
                case 17:
                    return "nitrogen";
                case 18:
                    return "oxygen";
                case 19:
                    return "phosphoric acid";
                case 20:
                    return "propane";
                case 21:
                    return "sodium hydroxide";
                case 22:
                    return "sulfuric acid";
                case 23:
                    return "toluene";
                case 24:
                    return "water";
                case 25:
                    return "Overall";


            }
            return "";
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Select":
                    return 0;
                case "acetic acid":
                    return 1;
                case "ammonia":
                    return 2;
                case "benzene":
                    return 3;
                case "carbon dioxide":
                    return 4;
                case "carbon monoxide":
                    return 5;
                case "cyclohexane":
                    return 6;
                case "ethane":
                    return 7;
                case "ethanol":
                    return 8;
                case "ethylene":
                    return 9;
                case "hydrochloric acid":
                    return 10;
                case "hydrogen":
                    return 11;
                case "methane":
                    return 12;
                case "methanol":
                    return 13;
                case "n-butane":
                    return 14;
                case "n-hexane":
                    return 15;
                case "n-octane":
                    return 16;
                case "nitrogren":
                    return 17;
                case "oxygen":
                    return 18;
                case "phosphoric acid":
                    return 19;
                case "propane":
                    return 20;
                case "sodium hydroxide":
                    return 21;
                case "sulfuric acid":
                    return 22;
                case "toluene":
                    return 23;
                case "water":
                    return 24;
                case "Overall":
                    return 25;
            }
            return "";
        }
    }
}
