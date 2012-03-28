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

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public enum ChemicalCompounds : byte
    { 
        AceticAcid = 1,
        Ammonia,
        Benzene,
        CarbonDioxide,
        CarbonMonoxide,
        Cyclohexane,
        Ethane,
        Ethanol,
        Ethylene,
        HydrochloricAcid,
        Hyrdogen,
        Methane,
        Methanol,
        n_nutane,
        n_nexane,
        n_nctane,
        Nitrogen,
        Oxygen,
        PhosphoricAcid,
        Propane,
        SodiumHydroxide,
        SulfuricAcid,
        Toluene,
        Water
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
