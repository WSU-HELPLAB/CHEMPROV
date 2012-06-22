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
using ChemProV.Core;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// E.O.
    /// This class represents an undo action that will resize a UserControl.
    /// </summary>
    public class ResizeControl : IUndoRedoAction
    {
        private Control m_control;

        private Size m_size;

        public ResizeControl(Control control)
        {
            // Store a reference to the element and copy its size value
            m_control = control;
            m_size = new Size(control.Width, control.Height);
        }

        public ResizeControl(Control control, Size sizeToRestore)
        {
            // Store a reference to the element and copy the size value
            m_control = control;
            m_size = sizeToRestore;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Create the opposite action first
            IUndoRedoAction opposite = new ResizeControl(m_control);

            // Set the new size
            m_control.Width = m_size.Width;
            m_control.Height = m_size.Height;

            return opposite;
        }
    }
}
