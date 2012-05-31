using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    [TypeConverter(typeof(ChemicalCompoundsFormatter))]
    public enum ChemicalCompounds : byte
    { 
        AceticAcid = 0,
        Ammonia,
        Benzene,
        CarbonDioxide,
        CarbonMonoxide,
        Cyclohexane,
        Ethane,
        Ethanol,
        Ethylene,
        HydrochloricAcid,
        Hydrogen,
        Methane,
        Methanol,
        n_butane,
        n_hexane,
        n_octane,
        Nitrogen,
        Oxygen,
        PhosphoricAcid,
        Propane,
        SodiumHydroxide,
        SulfuricAcid,
        Toluene,
        Water
    }

    /// <summary>
    /// Used to automatically format ChemicalCompounds into "pretty" strings 
    /// </summary>
    public class ChemicalCompoundsFormatter : TypeConverter
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
            if (value.GetType().Equals(typeof(ChemicalCompounds)) == false)
            {
                throw new ArgumentException("Can only convert from ChemicalCompounds.", "value");
            }
            string name = "";
            try
            {
                ChemicalCompounds unit = (ChemicalCompounds)value;
                name = unit.ToPrettyString();
            }
            catch (Exception ex)
            {
                name = value.ToString();
            }
            return name;
        }
    }

    public static class ChemicalCompoundsExtensions
    {
        /// <summary>
        /// Breaks apart the string name of the AssignmentType based on upper camel casing.
        /// Will also convert underscores "_" into dashes "-".
        /// EX: SomeEnum_1 will get returned as "Some Enum-1".
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ToPrettyString(this ChemicalCompounds compound)
        {
            string rawEnumValue = compound.ToString();
            rawEnumValue = rawEnumValue.Replace('_', '-');

            char[] characters = rawEnumValue.ToArray();

            string formattedValue = characters[0].ToString();
            for (int i = 1; i < characters.Length; i++)
            {
                if (char.IsUpper(characters[i]))
                {
                    formattedValue += " ";
                }
                formattedValue += characters[i].ToString();
            }
            return formattedValue.ToLower();
        }
    }
}
