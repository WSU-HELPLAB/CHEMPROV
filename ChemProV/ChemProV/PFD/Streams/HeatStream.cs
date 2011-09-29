/*
Copyright 2010, 2011 HELP Lab @ Washington State University

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
    }
}