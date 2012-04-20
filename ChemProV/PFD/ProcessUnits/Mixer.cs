﻿/*
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
    public class Mixer : LabeledProcessUnit
    {
        public Mixer()
            : base("/UI/Icons/pu_mixer.png")
        {        }

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// </summary>
        public override string Description
        {
            get
            {
                return "Mixer";
            }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Mixers are available at all difficulty settings
            return true;
        }

        public override int MaxIncomingStreams
        {
            get
            {
                // -1 implies an infinite number of possible incoming streams
                return -1;
            }
        }

        public override int MaxOutgoingStreams
        {
            get
            {
                return 1;
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