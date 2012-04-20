/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

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
    public class HeatExchanger : LabeledProcessUnit
    {
        public HeatExchanger()
            : base("/UI/Icons/pu_heat_exchanger.png")
        { }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // These are only available on the highest difficulty setting
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override string Description
        {
            get
            {
                return "Heat Exchanger With Utility";
            }
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public override int MaxIncomingStreams
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public override int MaxOutgoingStreams
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Total number of incoming heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public override int MaxIncomingHeatStreams
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Total number of outgoing heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public override int MaxOutgoingHeatStreams
        {
            get
            {
                return 0;
            }
        }
        
    }
}
