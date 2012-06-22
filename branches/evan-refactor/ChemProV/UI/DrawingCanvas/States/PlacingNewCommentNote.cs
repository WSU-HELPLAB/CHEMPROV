/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChemProV.Core;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the state we're in when we're placing a new comment. The comment can be free-floating 
    /// or anchored depending on where the user clicks to place it.
    /// </summary>
    public class PlacingNewCommentNote : IState
    {
        private DrawingCanvas m_canvas;

        /// <summary>
        /// For states that get created by the control palette, as is the case with the 
        /// PlacingStickyNote state, we need to keep a reference to the control palette 
        /// so that when we're done with our job we can inform it.
        /// </summary>
        private ControlPalette m_creator;

        /// <summary>
        /// This will be a reference to a stream or a process unit if either has been "highlighted" 
        /// by a mouse-over.
        /// </summary>
        private object m_highlightedHover = null;

        private Image m_placementIcon;

        public PlacingNewCommentNote(DrawingCanvas canvas, ControlPalette creator)
        {
            // Store the reference to the drawing canvas. This is where we will place the new 
            // sticky note.
            m_canvas = canvas;

            // Store a reference to the control palette
            m_creator = creator;

            // Create the placement icon and add it to the canvas. This will move around under the mouse 
            // until the user clicks in order to create it.
            m_placementIcon = Core.App.CreateImageFromSource("palette_stickyNote_16x16.png");
            m_placementIcon.Width = m_placementIcon.Height = 16;
            m_canvas.AddNewChild(m_placementIcon);
        }
        
        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (null == m_placementIcon)
            {
                return;
            }

            // Set the location of the icon to follow the mouse
            Point p = e.GetPosition(m_canvas);
            m_placementIcon.SetValue(Canvas.LeftProperty, p.X - m_placementIcon.Width / 2.0);
            m_placementIcon.SetValue(Canvas.TopProperty, p.Y - m_placementIcon.Height / 2.0);

            // If we've highlighted a process unit or stream then clear the highlight
            UnhighlightHover();

            // See if we're hovering over a process unit (will be null if we're not)
            m_highlightedHover = m_canvas.GetChildAtIncludeStreams(p, m_placementIcon);
            GenericProcessUnit pu = m_highlightedHover as GenericProcessUnit;
            AbstractStream stream = m_highlightedHover as AbstractStream;

            // Set the border if we are hovering over a process unit to indicate that we can attach 
            // an anchored comment to it
            if (null != pu)
            {
                pu.SetBorderColor(ProcessUnitBorderColor.AcceptingStreams);
            }
            else if (null != stream)
            {
                stream.Stem.Stroke = new SolidColorBrush(Colors.Green);
            }
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the placement icon is null then we don't do anything
            if (null == m_placementIcon)
            {
                return;
            }
            
            // Remove the placement icon from the canvas
            m_canvas.RemoveChild(m_placementIcon);
            m_placementIcon = null;

            // Make sure we don't leave anything highlighted
            UnhighlightHover();

            Point mousePt = e.GetPosition(m_canvas);
            
            // There are two possibilities for a mouse-down. The first is that the mouse isn't over 
            // any object that can have comments created for it, in which case we create a free-
            // floating comment. The second is that there is and we need to create an anchored comment.
            UIElement uie = m_canvas.GetChildAtIncludeStreams(mousePt);
            Core.ICommentCollection collection = uie as Core.ICommentCollection;
            if (null == collection)
            {
                // Create the sticky note and add it to the canvas
                StickyNoteControl note = new StickyNoteControl(m_canvas);
                m_canvas.AddNewChild(note);
                note.Width = note.Height = 100.0;
                note.Location = mousePt;
                note.SetValue(Canvas.ZIndexProperty, (int)4);

                // Create an undo
                m_canvas.GetWorkspace().AddUndo(new UndoRedoCollection(
                    "Undo creation of free-floating comment", new RemoveFromCanvas(note, m_canvas)));

                // Select the note on the canvas
                m_canvas.SelectedElement = note;

                // Tell the control palette to switch back to select mode. It will also set the drawing 
                // canvas's state to null
                m_creator.SwitchToSelect();
            }
            else
            {
                StickyNoteControl newNote;
                List<IUndoRedoAction> undos = StickyNoteControl.CreateCommentNote(
                    m_canvas, collection, null, out newNote);

                // Create an undo
                m_canvas.GetWorkspace().AddUndo(
                    new UndoRedoCollection("Undo creation of anchored comment", undos.ToArray()));

                // Select the note on the canvas
                m_canvas.SelectedElement = newNote;

                // Tell the control palette to switch back to select mode. It will also set the drawing 
                // canvas's state to null
                m_creator.SwitchToSelect();
            }
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
            // We set the icon reference to null after placing it, so if it's non-null and 
            // we're switching out the the state that means that we haven't placed it and 
            // should remove it.
            if (null != m_placementIcon)
            {
                m_canvas.Children.Remove(m_placementIcon);
                m_placementIcon = null;
            }
        }

        /// <summary>
        /// The member variable m_highlightedHover can be a reference to a process unit with a highlighted 
        /// border or a stream with a specially colored stem line. This method removes either of these 
        /// highlights and sets the member variable back to null.
        /// </summary>
        private void UnhighlightHover()
        {
            if (null == m_highlightedHover)
            {
                return;
            }

            GenericProcessUnit gpu = m_highlightedHover as GenericProcessUnit;
            if (null != gpu)
            {
                gpu.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
            }
            else
            {
                AbstractStream s = m_highlightedHover as AbstractStream;
                if (null != s)
                {
                    s.Selected = true;
                    s.Selected = false;
                }
            }

            m_highlightedHover = null;
        }
    }
}
