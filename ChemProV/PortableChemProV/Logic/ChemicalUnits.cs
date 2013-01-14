using System;
using System.Net;
using System.ComponentModel;

namespace ChemProV.Logic
{
    //public enum ChemicalUnits
    //{
    //    MassPercent = 0,
    //    MolePercent,
    //    Grams,
    //    GramsPerSecond,
    //    Kilogram,
    //    KilogramsPerSecond,
    //    Moles,
    //    MolesPerSecond,
    //    MoleFraction,
    //    MassFraction,
    //}

    public static class ChemicalUnitOptions
    {
        public static readonly string[] FullNames = new string[]{
            "Mass Percent", "Mole Percent", "Grams", "Grams Per Second", "Kilogram",
            "Kilograms Per Second", "Moles", "Moles Per Second", "Mole Fraction",
            "Mass Fraction"};

        public static readonly string[] ShortNames = new string[]{
            "mass %",
            "mol %",
            "g",
            "g/sec",
            "kg",
            "kg/sec",
            "mol",
            "mol/sec",
            "molFrac",
            "massFrac"};
    }

    ///// <summary>
    ///// Used to format ChemicalUnits into "pretty" strings 
    ///// </summary>
    //public static class ChemicalUnitsFormatter
    //{
    //    public static string ToPrettyString(ChemicalUnits unit)
    //    {
    //        string prettyString = "";
    //        switch (unit)
    //        {
    //            case ChemicalUnits.Grams:
    //                prettyString = "g";
    //                break;

    //            case ChemicalUnits.GramsPerSecond:
    //                prettyString = "g/sec";
    //                break;

    //            case ChemicalUnits.Kilogram:
    //                prettyString = "kg";
    //                break;

    //            case ChemicalUnits.KilogramsPerSecond:
    //                prettyString = "kg/sec";
    //                break;

    //            case ChemicalUnits.Moles:
    //                prettyString = "mol";
    //                break;

    //            case ChemicalUnits.MolesPerSecond:
    //                prettyString = "mol/sec";
    //                break;

    //            case ChemicalUnits.MassPercent:
    //                prettyString = "mass %";
    //                break;

    //            case ChemicalUnits.MolePercent:
    //                prettyString = "mol %";
    //                break;

    //            case ChemicalUnits.MoleFraction:
    //                prettyString = "molFrac";
    //                break;

    //            case ChemicalUnits.MassFraction:
    //                prettyString = "massFrac";
    //                break;


    //        }
    //        return prettyString;
    //    }
    //}
}
