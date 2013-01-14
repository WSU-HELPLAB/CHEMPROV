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
using ChemProV.Logic;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvasStates
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
        /// This will be a reference to a stream control or a process unit control if either has 
        /// been "highlighted" by a mouse-over.
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
            ProcessUnitControl pu = m_highlightedHover as ProcessUnitControl;
            ChemProV.PFD.Streams.StreamControl stream =
                m_highlightedHover as ChemProV.PFD.Streams.StreamControl;

            // Set the border if we are hovering over a process unit to indicate that we can attach 
            // an anchored comment to it
            if (null != pu)
            {
                pu.SetBorderColor(ProcessUnitBorderColor.AcceptingStreams);
            }
            else if (null != stream)
            {
                stream.SetLineBrush(new SolidColorBrush(Colors.Green));
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

            // Get a reference to the workspace
            Workspace ws = m_canvas.GetWorkspace();
            
            // There are two possibilities for a mouse-down. The first is that the mouse isn't over 
            // any object that can have comments created for it, in which case we create a free-
            // floating comment. The second is that there is and we need to create an anchored comment.
            UIElement uie = m_canvas.GetChildAtIncludeStreams(mousePt);
            if (uie is ProcessUnitControl || uie is PFD.Streams.StreamControl)
            {                
                // All we need to do is add a comment to the appropriate collection in the workspace, 
                // but we need to compute a smart location for it. There's a static method in the 
                // sticky note control that does this for is.
                MathCore.Vector pos = StickyNoteControl.ComputeNewCommentNoteLocation(
                    m_canvas, uie, 100.0, 100.0);

                // Create the new sticky note and add it to the workspace. Event handlers will update 
                // the UI appropriately.
                StickyNote sn = new StickyNote()
                {
                    Width = 100.0,
                    Height = 100.0,
                    LocationX = pos.X,
                    LocationY = pos.Y
                };
                if (uie is ProcessUnitControl)
                {
                    (uie as ProcessUnitControl).ProcessUnit.Comments.Add(sn);
                }
                else
                {
                    (uie as PFD.Streams.StreamControl).Stream.Comments.Add(sn);
                }

                // Create an undo
                m_canvas.GetWorkspace().AddUndo(new UndoRedoCollection(
                    "Undo creation of anchored comment", new Logic.Undos.RemoveStickyNote(sn)));

                // Tell the control palette to switch back to select mode. It will also set the drawing 
                // canvas's state to null
                m_creator.SwitchToSelect();
            }
            else
            {
                // This means we need to create a free-floating sticky note.
                StickyNote note = new StickyNote();
                note.Height = note.Width = 100.0;
                note.LocationX = mousePt.X;
                note.LocationY = mousePt.Y;

                // Add it to the workspace. Event subscribers will update the UI automatically.
                m_canvas.GetWorkspace().StickyNotes.Add(note);

                // Add an undo that will remove the sticky note
                m_canvas.GetWorkspace().AddUndo(new UndoRedoCollection(
                    "Undo creation of free-floating comment",
                    new Logic.Undos.RemoveStickyNote(note)));

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

            ProcessUnitControl gpu = m_highlightedHover as ProcessUnitControl;
            if (null != gpu)
            {
                gpu.SetBorderColor(ProcessUnitBorderColor.NoBorder);
            }
            else
            {
                PFD.Streams.StreamControl s = m_highlightedHover as PFD.Streams.StreamControl;
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
