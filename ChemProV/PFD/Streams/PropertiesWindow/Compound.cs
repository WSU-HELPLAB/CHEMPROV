/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Collections.Generic;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public class Compound
    {
        public Dictionary<Element, int> elements
        {
            get;
            set;
        }

        public double HeatCapacity
        {
            get;
            set;
        }

        public double HeatFormation
        {
            get;
            set;
        }

        public double HeatVaporization
        {
            get;
            set;
        }

        public double BoilingPoint
        {
            get;
            set;
        }

        public double MeltingPoint
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// ALL the abbreviations used. They should be unique
        /// abbreviated abbreviation Abbr
        ///
        /// "acetic acid":
        ///newCompound.Abbr = "aa";
        ///
        ///"ammonia":
        ///newCompound.Abbr = "am";
        ///
        ///"benzene":
        ///newCompound.Abbr = "be";
        ///
        ///"carbon dioxide":
        ///newCompound.Abbr = "cd";
        ///
        ///"carbon monoxide":
        ///newCompound.Abbr = "cm";
        ///
        ///"cyclohexane":
        ///newCompound.Abbr = "cy";
        ///
        ///"ethane":
        ///newCompound.Abbr = "et";
        ///
        ///"ethanol":
        ///newCompound.Abbr = "el";
        ///
        ///"ethylene":
        ///newCompound.Abbr = "ee";
        ///
        ///"hydrochloric acid":
        ///newCompound.Abbr = "ha";
        ///
        ///"hydrogen":
        ///newCompound.Abbr = "hy";
        ///
        ///"methane":
        ///newCompound.Abbr = "me";
        ///
        ///"methanol":
        ///newCompound.Abbr = "ml";
        ///
        ///"n-butane":
        ///newCompound.Abbr = "bu";
        ///
        ///"n-hexane":
        ///newCompound.Abbr = "he";
        ///
        ///"n-octane":
        ///newCompound.Abbr = "oc";
        ///
        ///"nitrogen":
        ///newCompound.Abbr = "ni";
        ///"oxygen":
        ///newCompound.Abbr = "ox";
        ///
        ///"propane":
        ///newCompound.Abbr = "pr";
        ///
        ///"sodium hydroxide":
        ///newCompound.Abbr = "sh";
        ///
        ///"sulfuric acid":
        ///newCompound.Abbr = "sa";
        ///
        ///"toluene":
        ///newCompound.Abbr = "to";
        ///
        ///"water":
        ///newCompound.Abbr = "wa";
        ///
        /// "xylene"
        /// newCompound.Abbr = "xy";
        /// </summary>
        public string Abbr
        {
            get;
            set;
        }
    }
}