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
using System.Reflection;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class PlacingNewProcessUnit : IState
    {
        private DrawingCanvas m_canvas;

        private ControlPalette m_palette;
        
        private GenericProcessUnit m_pu;
        
        public PlacingNewProcessUnit(ControlPalette sender, DrawingCanvas canvas, Type processUnitType)
        {
            m_canvas = canvas;
            m_palette = sender;
            
            // Create a new instance of the process unit
            m_pu = (GenericProcessUnit)Activator.CreateInstance(processUnitType);

            // Add it to the canvas but don't show it until we get our first mouse-move event
            canvas.AddNewChild(m_pu);
            m_pu.SetBorderColor(ProcessUnitBorderColor.NoBorder);
            m_pu.Visibility = Visibility.Collapsed;

            // Deselect
            canvas.SelectedElement = null;
        }
        
        #region IState Members

        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            // Position the process unit
            Point pos = e.GetPosition(m_canvas);
            m_pu.Location = pos;
            
            // Show the process unit if it wasn't already visible
            if (Visibility.Visible != m_pu.Visibility)
            {
                m_pu.Visibility = Visibility.Visible;
            }

            // Set border highlight if we're hoving over a stream endpoint
            DraggableStreamEndpoint endpoint = m_canvas.GetChildAt(pos, m_pu) as
                DraggableStreamEndpoint;
            if (null == endpoint)
            {
                m_pu.SetBorderColor(ProcessUnitBorderColor.Selected);
            }
            else
            {
                m_pu.SetBorderColor(endpoint.CanConnectTo(m_pu) ?
                    ProcessUnitBorderColor.AcceptingStreams : ProcessUnitBorderColor.NotAcceptingStreams);
            }
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Handle heat exchanger with utility as a special case
            if (m_pu is HeatExchanger)
            {
                MLBD_HEWU(sender, e);
                return;
            }           
            
            // Start by getting the mouse position
            Point pos = e.GetPosition(m_canvas);

            // See if we have a DraggableStreamEndpoint where we clicked
            DraggableStreamEndpoint endpoint = m_canvas.GetChildAt(pos, m_pu) as
                DraggableStreamEndpoint;

            // If there's not an endpoint, we just create an undo and we're finished
            if (null == endpoint)
            {
                m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo process unit creation",
                    new PFD.Undos.RemoveFromCanvas(m_pu, m_canvas)));

                // Select the process unit we just placed on the canvas
                m_canvas.SelectedElement = m_pu;

                // Set the process unit reference to null (see StateEnding function)
                m_pu = null;

                // Tell the control palette to switch back to select mode and then return
                m_palette.SwitchToSelect();
                return;
            }

            // Otherwise, if we HAVE clicked on an endpoint...

            // Check to see if we can't connect this way
            if (!endpoint.CanConnectTo(m_pu))
            {
                // The usability here may be debatable. But for now we'll cancel placement and 
                // give the user an error message
                m_canvas.RemoveChild(m_pu);
                m_pu = null;
                m_palette.SwitchToSelect();
                Core.App.MessageBox("The stream endpoint that you clicked cannot connect with " +
                    "the process unit that you were placing");
                return;
            }
            
            // Otherwise, if we CAN connect this way...

            // Make the undo and then the actual attachment
            if (DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected == endpoint.Type)
            {
                // Create an undo that sets the stream destination back to what it was and removes the 
                // process unit from the canvas
                m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo process unit creation + attachment",
                    new PFD.Undos.SetStreamDestination(endpoint.ParentStream, endpoint.ParentStream.Destination),
                    new PFD.Undos.RemoveFromCanvas(m_pu, m_canvas)));
                
                m_pu.AttachIncomingStream(endpoint.ParentStream);
                endpoint.ParentStream.Destination = m_pu;
            }
            else
            {
                // Create an undo that sets the stream source back to what it was and removes the 
                // process unit from the canvas
                m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo process unit creation + attachment",
                    new PFD.Undos.SetStreamSource(endpoint.ParentStream, endpoint.ParentStream.Source),
                    new PFD.Undos.RemoveFromCanvas(m_pu, m_canvas)));
                
                m_pu.AttachOutgoingStream(endpoint.ParentStream);
                endpoint.ParentStream.Source = m_pu;
            }

            // Select the process unit we just placed on the canvas
            m_canvas.SelectedElement = m_pu;

            // Make sure we set the process unit reference to null. This indicates completion of our 
            // state's job
            m_pu = null;

            // Go back to select mode
            m_palette.SwitchToSelect();
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
            // If we're ending this state but have a non-null process unit reference, then it means we haven't 
            // finished placing it. In fact, it most likely means that we don't want to finish placing it. It's 
            // likely that this signifies an event such as the user clicking on the "Select" button in the 
            // control palette, which should cancel the creation of the process unit.
            if (null != m_pu)
            {
                // Just remove from canvas and set reference to null
                m_canvas.RemoveChild(m_pu);
                m_pu = null;
            }
        }

        #endregion

        /// <summary>
        /// Handles mouse-left-button-down event for placing a heat exchanger with utility
        /// </summary>
        public void MLBD_HEWU(object sender, MouseButtonEventArgs e)
        {
            // Start by getting the mouse position
            Point pos = e.GetPosition(m_canvas);

            // See if we have a DraggableStreamEndpoint where we clicked
            DraggableStreamEndpoint endpoint = m_canvas.GetChildAt(pos, m_pu) as
                DraggableStreamEndpoint;

            // If there's not an endpoint:
            // 1. Create and attach an incoming heat stream. Position the source endpoint just
            //    below the process unit
            // 2. Create an undo that will remove both
            // 3. Switch to the appropriate state for moving the source endpoint
            if (null == endpoint)
            {
                HeatStream s = new HeatStream(m_canvas);
                m_canvas.AddNewChild(s);
                s.SetValue(Canvas.ZIndexProperty, -3);
                s.Destination = m_pu;
                m_pu.AttachIncomingStream(s);
                if (m_pu.Location.Y < 100.0)
                {
                    s.SourceDragIcon.Location = new Point(
                        m_pu.Location.X, m_pu.Location.Y + 80.0);
                    s.Table.Location = new Point(
                        m_pu.Location.X, m_pu.Location.Y + 140.0);
                }
                else
                {
                    s.SourceDragIcon.Location = new Point(
                        m_pu.Location.X, m_pu.Location.Y - 80.0);
                    s.Table.Location = new Point(
                        m_pu.Location.X, m_pu.Location.Y + 80.0);
                }

                // Show the stream and its components
                s.Visibility = Visibility.Visible;
                s.SourceDragIcon.Visibility = Visibility.Visible;
                s.DestinationDragIcon.Visibility = Visibility.Visible;
                (s.Table as UIElement).Visibility = Visibility.Visible;
                
                m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo creation of heat exchanger with utility",
                    new PFD.Undos.RemoveFromCanvas(m_pu, m_canvas),
                    new PFD.Undos.RemoveFromCanvas(s, m_canvas),
                    new PFD.Undos.RemoveFromCanvas(s.SourceDragIcon, m_canvas),
                    new PFD.Undos.RemoveFromCanvas(s.DestinationDragIcon, m_canvas),
                    new PFD.Undos.RemoveFromCanvas(s.Table as UIElement, m_canvas)));

                // Select the process unit we just placed on the canvas
                m_canvas.SelectedElement = m_pu;

                // Set the process unit reference to null (see StateEnding function)
                m_pu = null;

                m_palette.SwitchToSelect();

                // Flip to moving state and simulate mouse-down to start the positioning
                //m_canvas.CurrentState = s.SourceDragIcon;
                //m_canvas.CurrentState.MouseLeftButtonDown(this, e);

                return;
            }

            // Otherwise, if we HAVE clicked on an endpoint, just deny it by doing nothing
        }
    }
}
