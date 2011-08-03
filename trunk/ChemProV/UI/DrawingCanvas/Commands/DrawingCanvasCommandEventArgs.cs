/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL) v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;

namespace ChemProV.UI.DrawingCanvas.Commands
{
    public class DrawingCanvasCommandEventArgs
    {
        public readonly Point location;

        public DrawingCanvasCommandEventArgs(Point location = new Point())
        {
            this.location = location;
        }
    }
}