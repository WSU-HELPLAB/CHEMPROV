/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Author: Evan Olds

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

namespace ChemProV.Core
{
    /// <summary>
    /// Every item that can be placed and dragged around (moved) within the ChemProV drawing canvas area 
    /// must implement this interface
    /// </summary>
    public interface ICanvasElement
    {
        /// <summary>
        /// Gets or sets the location of the element within the canvas. Each object can implement this 
        /// the way they choose. For example, process units can consider the location to be the exact 
        /// center of the control. Sticky notes will consider it to be in the center horizontally but 
        /// towards the top vertically, in the "title bar" of the note.
        /// </summary>
        Point Location
        {
            get;
            set;
        }
    }
}
