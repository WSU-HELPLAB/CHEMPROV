/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

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
using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the state we're in when we're placing a new sticky note. This is a 
    /// fairly simple state and is focused just on placing the sticky note when the 
    /// user clicks the left mouse button down.
    /// </summary>
    public class PlacingStickyNote : IState
    {
        private DrawingCanvas m_canvas;

        /// <summary>
        /// For states that get created by the control palette, as is the case with the 
        /// PlacingStickyNote state, we need to keep a reference to the control palette 
        /// so that when we're done with our job we can inform it.
        /// </summary>
        private ControlPalette m_creator;

        /// <summary>
        /// This is the note that we will be placing. It will be initialized in the 
        /// constructor. Should this state be ended before we actually place it, then 
        /// we will have to remove it from the canvas.
        /// </summary>
        private StickyNote m_note;

        public PlacingStickyNote(DrawingCanvas canvas, ControlPalette creator)
        {
            // Store the reference to the drawing canvas. This is where we will place the new 
            // sticky note.
            m_canvas = canvas;

            // Store a reference to the control palette
            m_creator = creator;

            // Create the sticky note and add it
            m_note = new StickyNote(canvas);
            m_canvas.AddNewChild(m_note);
            m_note.Width = m_note.Height = 100.0;
        }
        
        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (null == m_note)
            {
                return;
            }

            // Just set the location to follow the mouse
            m_note.Location = e.GetPosition(m_canvas);
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Pressing the mouse button down places the note on the canvas and ends this action. Note 
            // that it's already one of the canvas's children. We need to do the following:
            //  1. Create and undo
            //  2. Null the note's reference (so as to avoid removing it in "StateEnding") and 
            //  3. change the state of the canvas.

            m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo sticky note creation",
                new PFD.Undos.RemoveFromCanvas(m_note, m_canvas)));

            // Select it on the canvas
            m_canvas.SelectedElement = m_note;

            m_note = null;
            
            // Tell the control palette to switch back to select mode. It will also set the drawing 
            // canvas's state to null
            m_creator.SwitchToSelect();
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        public void StateEnding()
        {
            // We set the note reference to null after placing it, so if it's non-null and 
            // we're switching out the the state that means that we haven't placed it and 
            // should remove it.
            if (null != m_note)
            {
                m_canvas.Children.Remove(m_note);
                m_note = null;
            }
        }
    }
}
