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

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// E.O.
    /// This class represents an undo action that will set the position of a UserControl 
    /// on a canvas.
    /// </summary>
    public class RestorePosition : IUndoRedoAction
    {
        private UserControl m_control;

        private Point m_pt;

        public RestorePosition(UserControl control)
        {
            // Store the reference to the control
            m_control = control;

            // Get the current position of the control. This is the position that 
            // we will restore to when executed.
            m_pt = new Point(
                (double)control.GetValue(Canvas.LeftProperty),
                (double)control.GetValue(Canvas.TopProperty));
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Start by creating a redo that moves the control back to where it
            // is now
            IUndoRedoAction redo = new RestorePosition(m_control);

            // Restore the Left and Top properties
            m_control.SetValue(Canvas.LeftProperty, m_pt.X);
            m_control.SetValue(Canvas.TopProperty, m_pt.Y);

            return redo;
        }
    }
}
