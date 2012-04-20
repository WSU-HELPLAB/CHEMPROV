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
    /// This class represents an undo action that will remove a UIElement from a 
    /// DrawingCanvas.
    /// </summary>
    public class RemoveFromCanvas : IUndoRedoAction
    {
        private ChemProV.UI.DrawingCanvas.DrawingCanvas m_canvas;

        private UIElement m_element;

        public RemoveFromCanvas(UIElement element, ChemProV.UI.DrawingCanvas.DrawingCanvas canvas)
        {
            m_canvas = canvas;
            m_element = element;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Start by making the opposite action that will add it back
            IUndoRedoAction opposite = new AddToCanvas(m_element, m_canvas);

            // Remove the element from the canvas
            m_canvas.Children.Remove(m_element);

            return opposite;
        }
    }
}
