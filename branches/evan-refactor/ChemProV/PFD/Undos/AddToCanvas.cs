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
    /// This class represents an undo action that will add a UIElement to a 
    /// DrawingCanvas.
    /// </summary>
    public class AddToCanvas : IUndoRedoAction
    {
        private ChemProV.UI.DrawingCanvas.DrawingCanvas m_canvas;

        private UIElement m_element;

        public AddToCanvas(UIElement element, ChemProV.UI.DrawingCanvas.DrawingCanvas canvas)
        {
            m_canvas = canvas;
            m_element = element;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Start by making the opposite action that will remove it
            IUndoRedoAction opposite = new RemoveFromCanvas(m_element, m_canvas);

            // Add the element to the canvas
            m_canvas.Children.Add(m_element);

            return opposite;
        }
    }
}
