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
    public class HeatExchangerNoUtility : LabeledProcessUnit
    {
        public HeatExchangerNoUtility()
            : base("/UI/Icons/pu_heat_exchanger_no_utility.png")
        { }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // These are only available on the highest difficulty setting
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override string DefaultLabelPrefix
        {
            get
            {
                return "Exc";
            }
        }

        public override string Description
        {
            get
            {
                return "Heat Exchanger Without Utility";
            }
        }

        public override int MaxIncomingStreams
        {
            get
            {
                return 2;
            }
        }

        public override int MaxOutgoingStreams
        {
            get
            {
                return 2;
            }
        }

        public override int MaxIncomingHeatStreams
        {
            get
            {
                return 0;
            }
        }

        public override int MaxOutgoingHeatStreams
        {
            get
            {
                return 0;
            }
        }
    }
}
