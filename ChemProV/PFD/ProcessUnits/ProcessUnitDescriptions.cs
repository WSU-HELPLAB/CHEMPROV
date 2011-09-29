/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

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

        public static string Reactor
        {
            get
            {
                return "Reactor";
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