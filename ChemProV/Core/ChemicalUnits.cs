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
using System.ComponentModel;

namespace ChemProV.Core
{
    [TypeConverter(typeof(ChemicalUnitsFormatter))]
    public enum ChemicalUnits
    {
        MassPercent = 0,
        MolePercent,
        Grams,
        GramsPerSecond,
        Kilogram,
        KilogramsPerSecond,
        Moles,
        MolesPerSecond,
        MoleFraction,
        MassFraction,
    }

    /// <summary>
    /// Used to automatically format ChemicalUnits into "pretty" strings 
    /// </summary>
    public class ChemicalUnitsFormatter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType.Equals(typeof(ChemicalUnits));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.Equals(typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType.Equals(typeof(string)) == false)
            {
                throw new ArgumentException("Can only convert to string.", "destinationType");
            }
            if (value.GetType().Equals(typeof(ChemicalUnits)) == false)
            {
                throw new ArgumentException("Can only convert from ChemicalUnits.", "value");
            }
            string name = "";
            try
            {
                ChemicalUnits unit = (ChemicalUnits)value;
                name = unit.ToPrettyString();
            }
            catch(Exception ex)
            {
                name = value.ToString();
            }
            return name;
        }
    }

    /// <summary>
    /// Adds a "ToPrettyString" to all ChemicalUnits enumeration variables
    /// </summary>
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

                case ChemicalUnits.MassPercent:
                    prettyString = "mass %";
                    break;

                case ChemicalUnits.MolePercent:
                    prettyString = "mol %";
                    break;

                case ChemicalUnits.MoleFraction:
                    prettyString = "molFrac";
                    break;

                case ChemicalUnits.MassFraction:
                    prettyString = "massFrac";
                    break;


            }
            return prettyString;
        }
    }
}
