/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

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
using ChemProV.Core;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// E.O.
    /// This class represents an undo action that will set the position of a UserControl 
    /// on a canvas.
    /// </summary>
    public class RestoreLocation : IUndoRedoAction
    {
        private ICanvasElement m_control;

        private Point m_pt;

        /// <summary>
        /// Creates an instance that will restore the control to the position that it's 
        /// currently at.
        /// </summary>
        public RestoreLocation(ICanvasElement control)
        {
            // Store the reference to the control
            m_control = control;

            // Get the current location of the control. This is the location that 
            // we will restore to when executed.
            m_pt = control.Location;
        }

        /// <summary>
        /// Creates an instance that will restore the control's position to the specified location.
        /// </summary>
        public RestoreLocation(ICanvasElement control, Point location)
        {
            // Store the reference to the control
            m_control = control;

            // Use the point provided in the argument
            m_pt = location;
        }

        /// <summary>
        /// Creates an instance that will restore the control's position to the specified location.
        /// </summary>
        public RestoreLocation(ICanvasElement control, MathCore.Vector location)
            : this(control, new Point(location.X, location.Y)) { }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Start by creating a redo that moves the control back to where it
            // is now
            IUndoRedoAction redo = new RestoreLocation(m_control);

            // Restore the location
            m_control.Location = m_pt;

            return redo;
        }
    }
}
