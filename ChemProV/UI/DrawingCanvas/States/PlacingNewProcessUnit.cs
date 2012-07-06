/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChemProV.Core;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvasStates
{
    public class PlacingNewProcessUnit : IState
    {
        private DrawingCanvas m_canvas;

        private ControlPalette m_palette;

        /// <summary>
        /// The icon that hovers under the mouse until we click down to create the 
        /// actual process unit. Will have the icon image for the process unit that 
        /// we are going to create.
        /// If this reference is null, then the state ignores input events.
        /// </summary>
        private Border m_placementIcon;

        /// <summary>
        /// The type of process unit that we are to create
        /// </summary>
        private Type m_type;

        private Workspace m_workspace;

        private SolidColorBrush s_greenBrush = new SolidColorBrush(Colors.Green);

        private SolidColorBrush s_redBrush = new SolidColorBrush(Colors.Red);
        
        public PlacingNewProcessUnit(ControlPalette sender, DrawingCanvas canvas,
            Type processUnitType)
        {
            m_canvas = canvas;
            m_palette = sender;
            m_type = processUnitType;
            m_workspace = canvas.GetWorkspace();

            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
            
            // Create the placement icon. This will hover under the mouse pointer 
            // as it moves and a mouse-down event will complete the placement.
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(ProcessUnitControl.GetIconSource(processUnitType), UriKind.Relative);
            Image img = new Image()
            {
                Source = bmp,
                Width = 44.0,
                Height = 44.0
            };

            m_placementIcon = new Border();
            m_placementIcon.Background = whiteBrush;
            m_placementIcon.Child = img;
            m_placementIcon.BorderThickness = new Thickness(0.0);

            // Add it to the canvas but don't show it until we get our first mouse-move event
            canvas.AddNewChild(m_placementIcon);
            m_placementIcon.Visibility = Visibility.Collapsed;

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
            // Position the placement icon
            Point pos = e.GetPosition(m_canvas);
            m_placementIcon.SetValue(Canvas.LeftProperty, pos.X - 20.0);
            m_placementIcon.SetValue(Canvas.TopProperty, pos.Y - 20.0);
            
            // Show the placement icon if it wasn't already visible
            if (Visibility.Visible != m_placementIcon.Visibility)
            {
                m_placementIcon.Visibility = Visibility.Visible;
            }

            // Set border highlight if we're hoving over a stream endpoint
            DraggableStreamEndpoint endpoint = m_canvas.GetChildAt(pos, m_placementIcon) as
                DraggableStreamEndpoint;
            if (null == endpoint)
            {
                m_placementIcon.BorderThickness = new Thickness(0.0);
            }
            else
            {
                Core.AbstractProcessUnit temp = (Core.AbstractProcessUnit)
                    Activator.CreateInstance(m_type, -1);
                m_placementIcon.BorderThickness = new Thickness(2.0);
                m_placementIcon.BorderBrush = (endpoint.CanConnectTo(temp) ? s_greenBrush : s_redBrush);
            }
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // We don't process messages if the placement icon is null
            if (null == m_placementIcon)
            {
                return;
            }
            
            // Handle heat exchanger with utility as a special case
            if (m_type.Equals(typeof(HeatExchangerWithUtility)))
            {
                MLBD_HEWU(sender, e);
                return;
            }           
            
            // Start by getting the mouse position
            Point pos = e.GetPosition(m_canvas);
            MathCore.Vector vPos = new MathCore.Vector(pos.X, pos.Y);

            // See if we have a DraggableStreamEndpoint where we clicked
            DraggableStreamEndpoint endpoint = m_canvas.GetChildAt(pos, m_placementIcon) as
                DraggableStreamEndpoint;

            // Remove the placement icon and then set it to null to indicate state completion
            m_canvas.RemoveChild(m_placementIcon);
            m_placementIcon = null;

            // If there's not an endpoint, we create the process unit with no stream connections
            if (null == endpoint)
            {
                Core.AbstractProcessUnit unit = (Core.AbstractProcessUnit)
                    Activator.CreateInstance(m_type);

                // Set the location
                unit.Location = new MathCore.Vector(pos.X, pos.Y);

                // Add it to the workspace
                m_workspace.AddProcessUnit(unit);

                // Add an undo that will remove it                
                m_workspace.AddUndo(new UndoRedoCollection("Undo process unit creation",
                    new ChemProV.Logic.Undos.RemoveProcessUnit(unit)));

                m_canvas.SelectedElement = null;

                // Tell the control palette to switch back to select mode and then return
                m_palette.SwitchToSelect();
                return;
            }

            // Otherwise, if we HAVE clicked on an endpoint...

            // Check to see if we can't connect this way
            Core.AbstractProcessUnit temp = (Core.AbstractProcessUnit)
                Activator.CreateInstance(m_type, -1);
            if (!endpoint.CanConnectTo(temp))
            {
                // The usability here may be debatable. But for now we'll cancel placement and 
                // give the user an error message
                m_palette.SwitchToSelect();
                Core.App.MessageBox("The stream endpoint that you clicked cannot connect with " +
                    "the process unit that you were placing.");
                return;
            }
            
            // Otherwise, if we CAN connect this way...

            // Create the process unit
            Core.AbstractProcessUnit apu = (Core.AbstractProcessUnit)
                Activator.CreateInstance(m_type);

            // Make the undo and then the actual attachment
            if (DraggableStreamEndpoint.EndpointType.StreamDestination == endpoint.Type)
            {
                // Set the location
                apu.Location = endpoint.ParentStream.Stream.DestinationLocation;
                
                // Create an undo that sets the stream destination back to what it was and removes the 
                // process unit
                m_workspace.AddUndo(new UndoRedoCollection("Undo process unit creation + attachment",
                    new Logic.Undos.SetStreamDestination(endpoint.ParentStream.Stream, null, apu,vPos),
                    new Logic.Undos.RemoveProcessUnit(apu)));

                apu.AttachIncomingStream(endpoint.ParentStream.Stream);
                endpoint.ParentStream.Stream.Destination = apu;
            }
            else
            {                
                // Set the lofcation
                apu.Location = endpoint.ParentStream.Stream.SourceLocation;
                
                // Create an undo that sets the stream source back to what it was and removes the 
                // process unit from the canvas
                Core.AbstractStream stream = endpoint.ParentStream.Stream;
                m_workspace.AddUndo(new UndoRedoCollection("Undo process unit creation + attachment",
                    new Logic.Undos.SetStreamSource(stream, null, apu, stream.SourceLocation),
                    new Logic.Undos.RemoveProcessUnit(apu)));
                
                apu.AttachOutgoingStream(endpoint.ParentStream.Stream);
                endpoint.ParentStream.Stream.Source = apu;
            }

            // Don't forget to add the process unit to the workspace. Event handlers will update the 
            // UI appropriately.
            m_workspace.AddProcessUnit(apu);

            m_canvas.SelectedElement = null;

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
            // If we're ending this state but have a non-null placement icon, then it means we haven't 
            // placed anything. In fact, it most likely means that we don't want to place anything. It's 
            // likely that this signifies an event such as the user clicking on the "Select" button in the 
            // control palette, which should cancel the creation of the process unit.
            if (null != m_placementIcon)
            {
                // Just remove from canvas and set reference to null
                m_canvas.RemoveChild(m_placementIcon);
                m_placementIcon = null;
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
            DraggableStreamEndpoint endpoint = m_canvas.GetChildAt(pos, m_placementIcon) as
                DraggableStreamEndpoint;

            // Remove the placement icon and then set it to null to indicate state completion
            m_canvas.RemoveChild(m_placementIcon);
            m_placementIcon = null;

            // If there's not an endpoint:
            // 1. Create and attach an incoming heat stream. Position the source endpoint just
            //    below the process unit
            // 2. Create an undo that will remove both
            // 3. Switch to the appropriate state for moving the source endpoint
            if (null == endpoint)
            {
                // Create the actual process unit
                Core.AbstractProcessUnit apu = (Core.AbstractProcessUnit)Activator.CreateInstance(
                    m_type, Core.AbstractProcessUnit.GetNextUID());
                apu.Location = new MathCore.Vector(pos.X, pos.Y);
                
                Core.HeatStream s = new Core.HeatStream();
                apu.AttachIncomingStream(s);
                s.Destination = apu;
                s.PropertiesTable = new StreamPropertiesTable(s);
                
                // Position the source of the heat stream. It's above the process unit by default, unless 
                // that would make it go off the screen
                if (apu.Location.Y < 150.0)
                {
                    s.SourceLocation = new MathCore.Vector(
                        apu.Location.X - 10.0, apu.Location.Y + 80.0);
                    s.PropertiesTable.Location = new MathCore.Vector(
                        apu.Location.X + 10.0, apu.Location.Y + 140.0);
                }
                else
                {
                    s.SourceLocation = new MathCore.Vector(
                        apu.Location.X - 10.0, apu.Location.Y - 80.0);
                    s.PropertiesTable.Location = new MathCore.Vector(
                        apu.Location.X + 10.0, apu.Location.Y - 100.0);
                }

                // Add the stream and the process unit to the workspace. Event handlers will update 
                // the UI appropriately.
                m_workspace.AddProcessUnit(apu);
                m_workspace.AddStream(s);

                m_workspace.AddUndo(new UndoRedoCollection("Undo creation of heat exchanger with utility",
                    new Logic.Undos.DetachIncomingStream(apu, s),
                    new Logic.Undos.SetStreamDestination(s, null, apu, s.DestinationLocation),
                    new Logic.Undos.RemoveStream(s),
                    new Logic.Undos.RemoveProcessUnit(apu)));

                // Select nothing on the canvas
                m_canvas.SelectedElement = null;

                m_palette.SwitchToSelect();

                return;
            }

            // Otherwise, if we HAVE clicked on an endpoint, just deny it by doing nothing
        }
    }
}
