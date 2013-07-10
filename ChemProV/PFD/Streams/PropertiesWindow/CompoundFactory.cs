/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Collections.Generic;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public class CompoundFactory
    {
        private static Element hydrogren = new Element("hydrogen");
        private static Element oxygen = new Element("oxygen");
        private static Element carbon = new Element("carbon");
        private static Element nitrogen = new Element("nitrogen");
        private static Element chlorine = new Element("chlorine");
        private static Element phosphorus = new Element("phosphorus");
        private static Element sulfer = new Element("sulfur");
        private static Element sodium = new Element("sodium");

        private CompoundFactory()
        {
        }

        public static string GetCompoundNameFromAbbr(string abbr)
        {
            switch (abbr)
            {
                case "aa":
                    return "acetic acid";
                case "am":
                    return "ammonia";
                case "be":
                    return "benzene";
                case "cd":
                    return "carbon dioxide";

                case "cm":
                    return "carbon monoxide";

                case "cy":
                    return "cyclohexane";

                case "et":
                    return "ethane";

                case "el":
                    return "ethanol";

                case "ee":
                    return "ethylene";

                case "eo":
                    return "ethylene oxide";
                
                case "fo":
                    return "formaldehyde";

                case "ha":
                    return "hydrochloric acid";

                case "hy":
                    return "hydrogen";

                case "me":
                    return "methane";

                case "ml":
                    return "methanol";

                case "bu":
                    return "n-butane";

                case "he":
                    return "n-hexane";

                case "oc":
                    return "n-octane";

                case "ni":
                    return "nitrogen";
                case "ox":
                    return "oxygen";

                case "pr":
                    return "propane";

                case "sh":
                    return "sodium hydroxide";

                case "sa":
                    return "sulfuric acid";

                case "to":
                    return "toluene";

                case "wa":
                    return "water";

                case "xy":
                    return "xylene";
            }
            return "";
        }

        public static Compound GetElementsOfCompound(string compoundName)
        {
            Compound newCompound = new Compound();
            Dictionary<Element, int> elements = new Dictionary<Element, int>();
            switch (compoundName)
            {
                case "overall":
                    {
                        elements.Add(hydrogren, 0);
                        elements.Add(oxygen, 0);
                        elements.Add(carbon, 0);
                        elements.Add(nitrogen, 0);
                        elements.Add(chlorine, 0);
                        elements.Add(phosphorus, 0);
                        elements.Add(sulfer, 0);
                        elements.Add(sodium, 0);
                    }
                    break;
                case "acetic acid":
                    {
                        elements.Add(carbon, 2);
                        elements.Add(hydrogren, 4);
                        elements.Add(oxygen, 2);
                        newCompound.HeatCapacity = double.NaN;
                        newCompound.HeatFormation = -486.18;
                        newCompound.HeatVaporization = 24.39;
                        newCompound.BoilingPoint = 118.2;
                        newCompound.MeltingPoint = 16.6;
                        newCompound.Abbr = "aa";
                    }
                    break;
                case "ammonia":
                    {
                        elements.Add(nitrogen, 1);
                        elements.Add(hydrogren, 3);
                        newCompound.HeatCapacity = 35.15;
                        newCompound.HeatFormation = -46.19;
                        newCompound.HeatVaporization = 23.351;
                        newCompound.BoilingPoint = -33.43;
                        newCompound.MeltingPoint = -77.8;
                        newCompound.Abbr = "am";
                    }
                    break;
                case "benzene":
                    {
                        elements.Add(carbon, 6);
                        elements.Add(hydrogren, 6);
                        newCompound.HeatCapacity = 62.55;
                        newCompound.HeatFormation = 48.66;
                        newCompound.HeatVaporization = 30.765;
                        newCompound.BoilingPoint = 80.1;
                        newCompound.MeltingPoint = 5.53;
                        newCompound.Abbr = "be";
                    }
                    break;
                case "carbon dioxide":
                    {
                        elements.Add(carbon, 1);
                        elements.Add(oxygen, 2);
                        newCompound.HeatCapacity = 36.11;
                        newCompound.HeatFormation = -393.5;
                        newCompound.HeatVaporization = double.NaN;
                        newCompound.BoilingPoint = double.NaN;
                        newCompound.MeltingPoint = -56.6;
                        newCompound.Abbr = "cd";
                    }
                    break;
                case "carbon monoxide":
                    {
                        elements.Add(carbon, 1);
                        elements.Add(oxygen, 1);
                        newCompound.HeatCapacity = 28.95;
                        newCompound.HeatFormation = -110.52;
                        newCompound.HeatVaporization = 6.042;
                        newCompound.BoilingPoint = -191.5;
                        newCompound.MeltingPoint = -205.1;
                        newCompound.Abbr = "cm";
                    }
                    break;
                case "cyclohexane":
                    {
                        elements.Add(carbon, 6);
                        elements.Add(hydrogren, 12);
                        newCompound.HeatCapacity = 94.14;
                        newCompound.HeatFormation = -156.2;
                        newCompound.HeatVaporization = 30.1;
                        newCompound.BoilingPoint = 80.7;
                        newCompound.MeltingPoint = 6.7;
                        newCompound.Abbr = "cy";
                    }
                    break;
                case "ethane":
                    {
                        elements.Add(carbon, 2);
                        elements.Add(hydrogren, 6);
                        newCompound.HeatCapacity = 49.37;

                        newCompound.HeatFormation = -84.67;
                        newCompound.HeatVaporization = 14.72;
                        newCompound.BoilingPoint = -88.6;
                        newCompound.MeltingPoint = -183.3;
                        newCompound.Abbr = "et";
                    }
                    break;
                case "ethanol":
                    {
                        elements.Add(carbon, 2);
                        elements.Add(hydrogren, 6);
                        elements.Add(oxygen, 1);
                        newCompound.HeatCapacity = 103.1;
                        newCompound.HeatFormation = -277.63;
                        newCompound.HeatVaporization = 38.58;
                        newCompound.BoilingPoint = 78.5;
                        newCompound.MeltingPoint = -114.6;
                        newCompound.Abbr = "el";
                    }
                    break;
                case "ethylene":
                    {
                        elements.Add(carbon, 2);
                        elements.Add(hydrogren, 4);
                        newCompound.HeatCapacity = 40.75;
                        newCompound.HeatFormation = 52.28;
                        newCompound.HeatVaporization = 13.54;
                        newCompound.BoilingPoint = -103.7;
                        newCompound.MeltingPoint = -169.2;
                        newCompound.Abbr = "ee";
                    }
                    break;

                case "ethylene oxide":
                    {
                        elements.Add(carbon, 2);
                        elements.Add(hydrogren, 4);
                        elements.Add(oxygen, 1);
                        newCompound.HeatCapacity = 48.19;
                        newCompound.HeatFormation = -52.28;
                        newCompound.HeatVaporization = 581.7;
                        newCompound.BoilingPoint = 10.7;
                        newCompound.MeltingPoint = -111.3;
                        newCompound.Abbr = "ee";
                    }
                    break;

                case "formaldehyde":
                    {
                        elements.Add(carbon, 1);
                        elements.Add(hydrogren, 2);
                        elements.Add(oxygen, 1);
                        newCompound.HeatCapacity = 34.28;
                        newCompound.HeatFormation = -115.9;
                        newCompound.HeatVaporization = 24.48;
                        newCompound.BoilingPoint = -19.3;
                        newCompound.MeltingPoint = -92.0;
                        newCompound.Abbr = "fo";
                    }
                    break;

                case "hydrochloric acid":
                    {
                        elements.Add(hydrogren, 1);
                        elements.Add(chlorine, 1);
                        newCompound.HeatCapacity = 29.13;
                        newCompound.HeatFormation = -92.31;
                        newCompound.HeatVaporization = 16.1;
                        newCompound.BoilingPoint = -85;
                        newCompound.MeltingPoint = -114.2;
                        newCompound.Abbr = "ha";
                    }
                    break;
                case "hydrogen":
                    {
                        elements.Add(hydrogren, 2);
                        newCompound.HeatCapacity = 28.84;
                        newCompound.HeatFormation = 0;
                        newCompound.HeatVaporization = 0.904;
                        newCompound.BoilingPoint = -252.76;
                        newCompound.MeltingPoint = -259.19;
                        newCompound.Abbr = "hy";
                    }
                    break;
                case "methane":
                    {
                        elements.Add(carbon, 1);
                        elements.Add(hydrogren, 4);
                        newCompound.HeatCapacity = 34.31;
                        newCompound.HeatFormation = -74.85;
                        newCompound.HeatVaporization = 8.179;
                        newCompound.BoilingPoint = -161.5;
                        newCompound.MeltingPoint = -182.5;
                        newCompound.Abbr = "me";
                    }
                    break;
                case "methanol":
                    {
                        elements.Add(carbon, 1);
                        elements.Add(hydrogren, 4);
                        elements.Add(oxygen, 1);
                        newCompound.HeatCapacity = 75.86;
                        newCompound.HeatFormation = -238.6;
                        newCompound.HeatVaporization = 35.27;
                        newCompound.BoilingPoint = 64.7;
                        newCompound.MeltingPoint = -97.9;
                        newCompound.Abbr = "ml";
                    }
                    break;
                case "n-butane":
                    {
                        elements.Add(carbon, 4);
                        elements.Add(hydrogren, 10);
                        newCompound.HeatCapacity = 92.3;
                        newCompound.HeatFormation = -124.7;
                        newCompound.HeatVaporization = 22.305;
                        newCompound.BoilingPoint = -0.6;
                        newCompound.MeltingPoint = -138.3;
                        newCompound.Abbr = "bu";
                    }
                    break;
                case "n-hexane":
                    {
                        elements.Add(carbon, 6);
                        elements.Add(hydrogren, 1);
                        newCompound.HeatCapacity = 216.3;
                        newCompound.HeatFormation = -198.8;
                        newCompound.HeatVaporization = 28.85;
                        newCompound.BoilingPoint = 68.74;
                        newCompound.MeltingPoint = -95.32;
                        newCompound.Abbr = "he";
                    }
                    break;
                case "n-octane":
                    {
                        elements.Add(carbon, 8);
                        elements.Add(hydrogren, 18);
                        newCompound.HeatCapacity = double.NaN;
                        newCompound.HeatFormation = -249.9;
                        newCompound.HeatVaporization = double.NaN;
                        newCompound.BoilingPoint = 125.5;
                        newCompound.MeltingPoint = -57;
                        newCompound.Abbr = "oc";
                    }
                    break;
                case "nitrogen":
                    {
                        elements.Add(nitrogen, 2);
                        newCompound.HeatCapacity = 29;
                        newCompound.HeatFormation = 0;
                        newCompound.HeatVaporization = 5.577;
                        newCompound.BoilingPoint = -195.8;
                        newCompound.MeltingPoint = -210;
                        newCompound.Abbr = "ni";
                    }
                    break;
                case "oxygen":
                    {
                        elements.Add(oxygen, 2);
                        newCompound.HeatCapacity = 29.1;
                        newCompound.HeatFormation = 0;
                        newCompound.HeatVaporization = 6.82;
                        newCompound.BoilingPoint = -182.997;
                        newCompound.MeltingPoint = -218.75;
                        newCompound.Abbr = "ox";
                    }
                    break;
                case "phosphoric acid":
                    {
                        elements.Add(hydrogren, 3);
                        elements.Add(phosphorus, 1);
                        elements.Add(oxygen, 4);
                        newCompound.HeatCapacity = double.NaN;
                        newCompound.HeatFormation = -1281.1;
                        newCompound.HeatVaporization = double.NaN;
                        newCompound.BoilingPoint = double.NaN;
                        newCompound.MeltingPoint = 42.3;
                        newCompound.Abbr = "pa";
                    }
                    break;
                case "propane":
                    {
                        elements.Add(carbon, 3);
                        elements.Add(hydrogren, 8);
                        newCompound.HeatCapacity = 68.032;
                        newCompound.HeatFormation = -103.8;
                        newCompound.HeatVaporization = 18.77;
                        newCompound.BoilingPoint = -42.07;
                        newCompound.MeltingPoint = -187.69;
                        newCompound.Abbr = "pr";
                    }
                    break;
                case "sodium hydroxide":
                    {
                        elements.Add(sodium, 1);
                        elements.Add(oxygen, 1);
                        elements.Add(hydrogren, 1);
                        newCompound.HeatCapacity = double.NaN;
                        newCompound.HeatFormation = -426.6;
                        newCompound.HeatVaporization = double.NaN;
                        newCompound.BoilingPoint = 1390;
                        newCompound.MeltingPoint = 319;
                        newCompound.Abbr = "sh";
                    }
                    break;
                case "sulfuric acid":
                    {
                        elements.Add(hydrogren, 2);
                        elements.Add(oxygen, 4);
                        elements.Add(sulfer, 1);
                        newCompound.HeatCapacity = 139.1;
                        newCompound.HeatFormation = -811.32;
                        newCompound.HeatVaporization = double.NaN;
                        newCompound.BoilingPoint = double.NaN;
                        newCompound.MeltingPoint = 10.35;
                        newCompound.Abbr = "sa";
                    }
                    break;
                case "toluene":
                    {
                        elements.Add(carbon, 7);
                        elements.Add(hydrogren, 8);
                        newCompound.HeatCapacity = 148.8;
                        newCompound.HeatFormation = 12;
                        newCompound.HeatVaporization = 33.47;
                        newCompound.BoilingPoint = 110.62;
                        newCompound.MeltingPoint = -94.99;
                        newCompound.Abbr = "to";
                    }
                    break;
                case "water":
                    {
                        elements.Add(hydrogren, 2);
                        elements.Add(oxygen, 1);
                        newCompound.HeatCapacity = 75.4;
                        newCompound.HeatFormation = -285.84;
                        newCompound.HeatVaporization = 40.656;
                        newCompound.BoilingPoint = 100;
                        newCompound.MeltingPoint = 0;
                        newCompound.Abbr = "wa";
                    }
                    break;

                case "xylene":
                    {
                        elements.Add(hydrogren, 10);
                        elements.Add(carbon, 8);
                        newCompound.HeatCapacity = 182;
                        newCompound.HeatFormation = -24.4;
                        newCompound.HeatVaporization = 35.7;
                        newCompound.BoilingPoint = 138.5;
                        newCompound.MeltingPoint = -47.4;
                        newCompound.Abbr = "xy";
                    }
                    break;
            }
            newCompound.Name = compoundName;
            newCompound.elements = elements;
            return newCompound;
        }
    }
}