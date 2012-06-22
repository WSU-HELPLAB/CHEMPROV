/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Windows;
using System.Windows.Input;
using ChemProV.Core;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// State logic (mouse-input processing) for resizing a sticky note and creating appropriate 
    /// undo actions.
    /// </summary>
    public class ResizingStickyNote : IState
    {
        private const double c_minDim = 50.0;
        
        /// <summary>
        /// Refernce to the application's drawing canvas
        /// </summary>
        private DrawingCanvas m_canvas;

        private bool m_isMouseDown = false;

        private Point m_mouseDownPt = new Point();
        
        /// <summary>
        /// Reference to the note that we're resizing
        /// </summary>
        private ChemProV.PFD.StickyNote.StickyNoteControl m_note;

        private Size m_sizeOnMouseDown = new Size();

        public ResizingStickyNote(DrawingCanvas canvas, ChemProV.PFD.StickyNote.StickyNoteControl note)
        {
            m_canvas = canvas;
            m_note = note;
        }

        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_isMouseDown)
            {
                // Nothing we can do if we didn't get a mouse-down event first
                return;
            }

            Point now = e.GetPosition(m_canvas);
            double newW = m_sizeOnMouseDown.Width + (now.X - m_mouseDownPt.X);
            double newH = m_sizeOnMouseDown.Height + (now.Y - m_mouseDownPt.Y);

            // Don't go below the minimum size on either dimension
            m_note.Width = Math.Max(newW, c_minDim);
            m_note.Height = Math.Max(newH, c_minDim);
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_isMouseDown = true;
            m_mouseDownPt = e.GetPosition(m_canvas);
            m_sizeOnMouseDown = m_note.RenderSize;
            e.Handled = true;

            // Ensure that we have the right cursor
            m_canvas.Cursor = Cursors.SizeNWSE;
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!m_isMouseDown)
            {
                // Nothing we can do if we didn't get a mouse-down event first
                return;
            }

            // Releasing the mouse button signifies completion of the resize and 
            // requires us to create an undo
            m_canvas.GetWorkspace().AddUndo(
                new UndoRedoCollection("Undo sticky note resizing",
                    new ResizeControl(m_note, m_sizeOnMouseDown)));

            m_isMouseDown = false;

            // Make sure we go back to the default cursor
            m_canvas.Cursor = Cursors.Arrow;

            // Switch back to the default canvas state
            m_canvas.CurrentState = null;
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        public void StateEnding()
        {
            if (m_isMouseDown)
            {
                // If we had a mouse down event and thus started a resize but now we're being 
                // told that we need to end, then the best we can do is restore the sticky 
                // note to its original size and treat this as a resize cancelation
                m_note.Width = m_sizeOnMouseDown.Width;
                m_note.Height = m_sizeOnMouseDown.Height;
                m_isMouseDown = false;

                // Make sure we go back to the default cursor
                m_canvas.Cursor = Cursors.Arrow;
            }
        }
    }
}
