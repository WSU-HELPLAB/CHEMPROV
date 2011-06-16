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

namespace ChemProV.PFD.ProcessUnits
{
    /// <summary>
    /// Houses a bunch of string constants that describe the available process units.
    /// Hopefully, the code is pretty self-explanitory, so I'm not going to supply
    /// further comments.
    /// </summary>
    public static class ProcessUnitDescriptions
    {
        public static string Blank
        {
            get
            {
                return "Blank Process Unit";
            }
        }

        public static string Generic
        {
            get
            {
                return "Generic Process Unit";
            }
        }

        public static string HeatExchanger
        {
            get
            {
                return "Heat Exchanger With Utility";
            }
        }

        public static string HeatExchangerNoUtility
        {
            get
            {
                return "Heat Exchanger Without Utility";
            }
        }

        public static string Mixer
        {
            get
            {
                return "Mixer";
            }
        }

        public static string Separator
        {
            get
            {
                return "Separator";
            }
        }

        public static string Sink
        {
            get
            {
                return "Sink";
            }
        }

        public static string Source
        {
            get
            {
                return "Source";
            }
        }
    }
}
