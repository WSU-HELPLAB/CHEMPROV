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

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public enum ChemicalUnits
    {
        Percent = 1,
        Grams,
        GramsPerSecond,
        Kilogram,
        KilogramsPerSecond,
        Moles,
        MolesPerSecond
    }

    public static class ChemicalUnitsExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ToPrettyString(this ChemicalUnits unit)
        {
            string prettyString = "";
            switch (unit)
            { 
                case ChemicalUnits.Grams:
                    prettyString = "g";
                    break;

                case ChemicalUnits.GramsPerSecond:
                    prettyString = "g/sec";
                    break;

                case ChemicalUnits.Kilogram:
                    prettyString = "kg";
                    break;

                case ChemicalUnits.KilogramsPerSecond:
                    prettyString = "kg/sec";
                    break;

                case ChemicalUnits.Moles:
                    prettyString = "mol";
                    break;

                case ChemicalUnits.MolesPerSecond:
                    prettyString = "mol/sec";
                    break;

                case ChemicalUnits.Percent:
                    prettyString = "%";
                    break;
            }
            return prettyString;
        }
    }
}
