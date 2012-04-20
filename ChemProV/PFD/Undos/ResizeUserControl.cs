/*
Copyright 2012 HELP Lab @ Washington State University

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
    /// This class represents an undo action that will resize a UserControl.
    /// </summary>
    public class ResizeUserControl : IUndoRedoAction
    {
        private UserControl m_control;

        private Size m_size;

        public ResizeUserControl(UserControl control)
        {
            // Store a reference to the element and copy its size value
            m_control = control;
            m_size = new Size(control.Width, control.Height);
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Create the opposite action first
            IUndoRedoAction opposite = new ResizeUserControl(m_control);

            // Set the new size
            m_control.Width = m_size.Width;
            m_control.Height = m_size.Height;

            return opposite;
        }
    }
}
