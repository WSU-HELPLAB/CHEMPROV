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
    public class Reactor : LabeledProcessUnit
    {
        public Reactor()
            : base("/UI/Icons/pu_reactor.png")
        { }

        public override string Description
        {
            get
            {
                return "Reactor";
            }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Reactors are available on everything but the easiest difficulty setting
            return (OptionDifficultySetting.MaterialBalance != difficulty);
        }

        public override int MaxIncomingStreams
        {
            get
            {
                return -1;
            }
            //set
            //{
            //    throw new InvalidOperationException();
            //}
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
                return 1;
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
