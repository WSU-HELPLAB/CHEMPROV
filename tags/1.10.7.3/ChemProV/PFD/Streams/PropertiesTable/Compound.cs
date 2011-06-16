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
using System.Collections.Generic;

namespace ChemProV.PFD.Streams.PropertiesTable
{
    public class Compound
    {
        public List<KeyValuePair<Element, int>> elements
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
        /// ALL the abbreviations used. They should unique
        /// yes I abbreviated abbreviation
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
        /// </summary>
        public string Abbr
        {
            get;
            set;
        }
    }
}
