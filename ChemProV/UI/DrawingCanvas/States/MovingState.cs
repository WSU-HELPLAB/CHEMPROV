/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD;
using ChemProV.Core;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the state for the drawing canvas when we are moving an object. This is NOT used to create 
    /// new objects, only move existing ones.
    /// </summary>
    public class MovingState : IState
    {
        private DrawingCanvas m_canvas;

        private bool m_mouseDown = false;

        private MathCore.Vector m_mouseDownPt = new MathCore.Vector();

        private MathCore.Vector m_originalLocation;

        private GenericProcessUnit m_puThatGotBorderChange = null;
        
        /// <summary>
        /// Private constructor. Use the static "Create" method to create an instance of a moving state. There 
        /// are only certain types of items that we can move so we use the static constructor for error 
        /// checking.
        /// </summary>
        /// <param name="c"></param>
        private MovingState(DrawingCanvas c)
        {
            m_canvas = c;
        }

        public static MovingState Create(DrawingCanvas canvas)
        {
            // It's implied that the element we want to move is the selected item on the canvas
            object elementToMove = canvas.SelectedElement;
            
            if (null == elementToMove || !(elementToMove is ICanvasElement))
            {
                // We can only move canvas elements
                return null;
            }

            MovingState ms = new MovingState(canvas);
            ICanvasElement ce = elementToMove as ICanvasElement;
            ms.m_originalLocation = new MathCore.Vector(ce.Location);
            return ms;
        }

        #region IState Members

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_mouseDown)
            {
                return;
            }
            
            // We can't do anything if the canvas is read-only
            if (m_canvas.IsReadOnly)
            {
                return;
            }
            
            Point pt = e.GetPosition(m_canvas);

            MathCore.Vector diff = (new MathCore.Vector(pt)) - m_mouseDownPt;
            pt = (m_originalLocation + diff).ToPoint();

            // If we changed a border color, change it back and null the reference
            if (null != m_puThatGotBorderChange)
            {
                m_puThatGotBorderChange.SetBorderColor(ProcessUnitBorderColor.Selected);
                m_puThatGotBorderChange = null;
            }

            // Various objects have slightly different moving rules so we need to check the type
            if (m_canvas.SelectedElement is PFD.ProcessUnits.GenericProcessUnit)
            {
                // First off, move the process unit
                (m_canvas.SelectedElement as GenericProcessUnit).Location = pt;
                
                // See if we're dragging it over anything else
                object hoveringOver = m_canvas.GetChildAt(pt, m_canvas.SelectedElement);

                // See if it's a stream connection endpoint
                DraggableStreamEndpoint dse = hoveringOver as DraggableStreamEndpoint;
                if (null != dse)
                {
                    m_puThatGotBorderChange = m_canvas.SelectedElement as GenericProcessUnit;
                    // Set the process unit's border color
                    m_puThatGotBorderChange.SetBorderColor(dse.CanConnectTo(m_puThatGotBorderChange) ?
                        ProcessUnitBorderColor.AcceptingStreams : ProcessUnitBorderColor.NotAcceptingStreams);
                }
            }
            else if (m_canvas.SelectedElement is Core.ICanvasElement)
            {
                // Other items just get their locations set
                (m_canvas.SelectedElement as Core.ICanvasElement).Location = pt;
            }

            // NOTE: Stream-oriented moving stuff is handled elsewhere. See the DraggableStreamEndpoint class
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!m_mouseDown)
            {
                // If we didn't capture a mouse down first then we don't do the mouse-up
                return;
            }
            
            // Mouse button is no longer down
            m_mouseDown = false;

            Point location = e.GetPosition(m_canvas);
            if (m_canvas.SelectedElement is IProcessUnit)
            {
                DropProcessUnit(m_canvas.SelectedElement as IProcessUnit, location);
            }
            // Note that stream endpoint stuff is handled in a different state object
            else if (m_canvas.SelectedElement is ICanvasElement)
            {
                // All we have to do here is create an undo and then fall through to 
                // below and go back to the null state
                m_canvas.AddUndo(new UndoRedoCollection("Undo move",
                    new PFD.Undos.RestoreLocation((ICanvasElement)m_canvas.SelectedElement, 
                        m_originalLocation.ToPoint())));
            }

            // Letting up the left mouse button signifies the end of the moving state. Therefore 
            // we want to set the drawing canvas state to null
            m_canvas.CurrentState = null;
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_mouseDown = true;
            m_mouseDownPt = new MathCore.Vector(e.GetPosition(m_canvas));
        }

        #region Unused Mouse Events

        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public void StateEnding()
        {
        }

        #endregion Unused Mouse Events

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
            MouseLeftButtonUp(sender, e as MouseButtonEventArgs);
        }

        #endregion IState Members

        private void DropProcessUnit(IProcessUnit pu, Point location)
        {
            // We want to see if the process unit is being dragged and dropped onto a stream 
            // source or destination

            // Get the element (besides the one being dragged) that's at the mouse location
            object dropTarget = m_canvas.GetChildAt(location, m_canvas.SelectedElement);

            // If there is no child at that point or there is a child but it's not a stream endpoint 
            // then this ends up just being a move of the process unit
            if (null == dropTarget || !(dropTarget is DraggableStreamEndpoint))
            {
                // Add an undo that will move the process unit back to where it was
                m_canvas.AddUndo(new UndoRedoCollection("Undo moving process unit",
                    new PFD.Undos.RestoreLocation(pu, m_originalLocation.ToPoint())));

                // The control is already in the right position from the mouse-move event, so we're done
                return;
            }

            // Coming here means that we've dropped the process unit on a stream endpoint. We need to
            // check if this is a valid move or not and handle it appropriately.
            DraggableStreamEndpoint streamEndpoint = dropTarget as DraggableStreamEndpoint;
            if (streamEndpoint.CanConnectTo(pu))
            {
                switch (streamEndpoint.Type)
                {
                    case DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected:
                        m_canvas.AddUndo(new UndoRedoCollection("Undo moving and connecting process unit",
                            new PFD.Undos.DetachIncomingStream(pu, streamEndpoint.ParentStream),
                            new PFD.Undos.SetStreamDestination(streamEndpoint.ParentStream, null),
                            new PFD.Undos.RestoreLocation(pu, m_originalLocation.ToPoint())));
                        pu.AttachIncomingStream(streamEndpoint.ParentStream);
                        streamEndpoint.ParentStream.Destination = pu;
                        break;

                    case DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected:
                        m_canvas.AddUndo(new UndoRedoCollection("Undo moving and connecting process unit",
                            new PFD.Undos.DetachOutgoingStream(pu, streamEndpoint.ParentStream),
                            new PFD.Undos.SetStreamSource(streamEndpoint.ParentStream, null),
                            new PFD.Undos.RestoreLocation(pu, m_originalLocation.ToPoint())));
                        pu.AttachOutgoingStream(streamEndpoint.ParentStream);
                        streamEndpoint.ParentStream.Source = pu;
                        break;

                    default:
                        // Um....
                        break;
                }
            }
            else // Not a valid move
            {
                // In this case we simply snap it back to where it was when the move started. 
                // Ideally we should have some sort of animation that makes it slide back to its original 
                // location, but that can come much later if we want it.
                pu.Location = m_originalLocation.ToPoint();
                
                // Note that in either case we've essentially canceled the action and no net-change has 
                // been made. Thus we don't need to create an undo.
                return;
            }
        }
    }
}