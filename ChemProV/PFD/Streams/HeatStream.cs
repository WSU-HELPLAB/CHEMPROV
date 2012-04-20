/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows.Media;

namespace ChemProV.PFD.Streams
{
    public class HeatStream : AbstractStream
    {
        public HeatStream()
            : base()
        {
            SolidColorBrush red = new SolidColorBrush(Colors.Red);
            this.Stem.Stroke = red;
            this.Stem.Fill = red;
            this.Arrow.Fill = red;
            this.rectangle.Fill = red;
            this.rectangle.Stroke = red;
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Heat streams are only available with MaterialAndEnergyBalance
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override bool IsValidSource(ProcessUnits.IProcessUnit unit)
        {
            // E.O.
            // Um, it looks like with the current version NOTHING is valid as 
            // source for a heat stream
            // TODO: Fix this, it can't be right
            return false;
        }

        public override bool IsValidDestination(ProcessUnits.IProcessUnit unit)
        {
            // E.O.
            // Heat streams can only have reactors as destinations (and of course the 
            // reactor unit has to be accepting incoming streams).
            // TODO: Check with the chemistry guys to verify this
            return ((unit is PFD.ProcessUnits.Reactor) &&
                unit.IsAcceptingIncomingStreams(this));
        }

        public override string Title
        {
            get
            {
                return "Heat Stream";
            }
        }
    }
}