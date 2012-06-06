/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Collections.Generic;
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
using ChemProV.Core;
using ChemProV.PFD;
using ChemProV.PFD.Streams;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the state used to place a new stream on the drawing canvas.
    /// </summary>
    public class PlacingNewStream : IState
    {
        private DrawingCanvas m_canvas;

        /// <summary>
        /// This is the initial placement icon. It is just an image (with the icon of stream 
        /// source connect) that floats around with the mouse until the user clicks. After 
        /// we've placed the source of the stream on the canvas, this is removed from the 
        /// canvas and set to null. Then we move on to placing the stream destination.
        /// </summary>
        private Image m_initialIcon;

        private AbstractStream m_newStream = null;

        private UI.ControlPalette m_palette;

        /// <summary>
        /// Indicates the phase we're in, which determines how we handle mouse events.
        ///  1 = Moving the stream source icon around with the mouse, waiting for a mouse-
        ///     down event to create the stream
        ///  2 = Stream source has been placed, either in a blank space within the PFD or 
        ///      connected to a process unit. We're waiting for a mouse-up event if we're 
        ///      in this state. If we are in this state and release the mouse button in a 
        ///      blank space or over a process unit that is not the same as the source, then 
        ///      this finalizes the creation action.
        ///  3 = The mouse was over the source process unit during the mouse-up event that was 
        ///      received in phase 2. In this case we are in click-click stream creation mode, 
        ///      meaning that the next mouse-down event should finalize the stream creation.
        /// -1 = The state is finished
        /// </summary>
        private int m_phase = 1;

        /// <summary>
        /// Location of the mouse during the mouse-down event that was received in phase 1
        /// </summary>
        private MathCore.Vector m_phase1DownPt;

        /// <summary>
        /// As we move stream source and destination connectors over various process units, we 
        /// may change their border colors. If this value is non-null, then it references a 
        /// process unit whose border color has been changed and must be changed back when we 
        /// complete the placing action or move the mouse out of its area.
        /// </summary>
        private PFD.ProcessUnits.GenericProcessUnit m_puWithAlteredBorderColor = null;

        private Type m_newObjType;

        public PlacingNewStream(UI.ControlPalette sender, DrawingCanvas canvas, Type objectType)
        {
            // Quick error check on parameters
            if (null == sender || null == canvas || null == objectType)
            {
                throw new ArgumentNullException();
            }

            // Make sure the object type is OK
            if (!objectType.IsSubclassOf(typeof(AbstractStream)))
            {
                throw new InvalidOperationException(
                    "Cannot use PlacingNewStream state to create an object of type: " +
                    objectType.Name);
            }
            
            m_palette = sender;
            m_canvas = canvas;
            m_newObjType = objectType;

            // Create the stream, put it on the canvas, and hide it
            m_newStream = (AbstractStream)Activator.CreateInstance(objectType, m_canvas);
            m_canvas.AddNewChild(m_newStream);
            m_newStream.SetValue(Canvas.ZIndexProperty, -1);
            // The stream has 4 components that we have to deal with
            m_newStream.Visibility = Visibility.Collapsed;
            m_newStream.SourceDragIcon.Visibility = Visibility.Collapsed;
            m_newStream.DestinationDragIcon.Visibility = Visibility.Collapsed;
            (m_newStream.Table as UIElement).Visibility = Visibility.Collapsed;

            // Create the placement icon
            m_initialIcon = new Image();
            Uri uri = new Uri("/UI/Icons/pu_source.png", UriKind.Relative);
            ImageSource img = new System.Windows.Media.Imaging.BitmapImage(uri);
            m_initialIcon.SetValue(Image.SourceProperty, img);
            m_canvas.AddNewChild(m_initialIcon);
            m_initialIcon.SetValue(Canvas.ZIndexProperty, 4);
        }

        #region IState members

        public void MouseEnter(object sender, MouseEventArgs e)
        {
            // Nothing needed here
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
            // Nothing needed here
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePt = e.GetPosition(m_canvas);

            // Clear the border if we'd previously set one
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
                m_puWithAlteredBorderColor = null;
            }

            // See if we're hovering over a process unit (will be null if we're not)
            m_puWithAlteredBorderColor = 
                m_canvas.GetChildAt(e.GetPosition(m_canvas), m_initialIcon) as GenericProcessUnit;
            
            // If the initial placement icon is non-null, then this means we're in the initial state 
            // where we're about to place the stream source. All we have to do in this state is 
            // highlight process units appropriately when we hover over them and position the icon.
            if (null != m_initialIcon)
            {                
                m_initialIcon.SetValue(Canvas.LeftProperty, mousePt.X - m_initialIcon.ActualWidth / 2.0);
                m_initialIcon.SetValue(Canvas.TopProperty, mousePt.Y - m_initialIcon.ActualHeight / 2.0);

                if (null != m_puWithAlteredBorderColor)
                {
                    m_puWithAlteredBorderColor.SetBorderColor(
                        m_puWithAlteredBorderColor.IsAcceptingOutgoingStreams(m_newStream) ?
                        ProcessUnitBorderColor.AcceptingStreams : ProcessUnitBorderColor.NotAcceptingStreams);
                }
            }
            else
            {
                // Move the destination icon around
                m_newStream.DestinationDragIcon.Location = mousePt;

                if (null != m_puWithAlteredBorderColor)
                {
                    m_puWithAlteredBorderColor.SetBorderColor(
                        m_puWithAlteredBorderColor.IsAcceptingIncomingStreams(m_newStream) ?
                        ProcessUnitBorderColor.AcceptingStreams : ProcessUnitBorderColor.NotAcceptingStreams);
                }

                m_newStream.UpdateStreamLocation();
            }
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point location = e.GetPosition(m_canvas);

            // Clear the border if we'd previously set one
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
                m_puWithAlteredBorderColor = null;
            }

            // See if we're in the phase where we're placing the source
            if (1 == m_phase)
            {
                // Start by removing the floating icon
                m_canvas.RemoveChild(m_initialIcon);
                m_initialIcon = null;

                // Store the mouse position
                m_phase1DownPt = new MathCore.Vector(location.X, location.Y);

                // See if we clicked on a valid destination object
                GenericProcessUnit gpu = m_canvas.GetChildAt(location, m_newStream) as GenericProcessUnit;

                if (null == gpu)
                {
                    m_newStream.SourceDragIcon.Location = location;
                    m_newStream.DestinationDragIcon.Location = location;
                }
                else if (gpu.IsAcceptingOutgoingStreams(m_newStream))
                {
                    m_newStream.SourceDragIcon.Location = location;
                    m_newStream.DestinationDragIcon.Location = location;
                    gpu.AttachOutgoingStream(m_newStream);
                    m_newStream.Source = gpu;
                    m_newStream.UpdateStreamLocation();
                }
                else
                {
                    // Bad placement. Cancel everything
                    m_phase = -1;
                    m_canvas.RemoveChild(m_newStream);
                    m_canvas.RemoveChild(m_newStream.SourceDragIcon);
                    m_canvas.RemoveChild(m_newStream.DestinationDragIcon);
                    m_canvas.RemoveChild(m_newStream.Table as UIElement);

                    m_palette.SwitchToSelect();
                    return;
                }

                // Now is where we finally show the stream (everything but the table)
                m_newStream.Visibility = Visibility.Visible;
                m_newStream.SourceDragIcon.Visibility = Visibility.Visible;
                m_newStream.DestinationDragIcon.Visibility = Visibility.Visible;

                // Position the table and make sure it is hidden
                m_newStream.Table.Location = m_newStream.CalculateTablePositon(location, location);
                m_newStream.HideTable();

                // We're in phase 2 now
                m_phase = 2;
            }
            else if (3 == m_phase)
            {
                // This means that we need to place the stream endpoint. All the logic for this 
                // is already in the mouse-up handler for phase 2. So we can simply switch to phase 
                // 2 and call the mouse-up function.
                m_phase = 2;
                MouseLeftButtonUp(sender, e);
            }
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If we're not in phase two then we shouldn't be getting this event
            if (2 != m_phase)
            {
                return;
            }

            // Ending phase 2 by whatever we do here
            m_phase = -1;

            // Clear the border if we'd previously set one
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
                m_puWithAlteredBorderColor = null;
            }

            // Get the mouse position
            Point pos = e.GetPosition(m_canvas);

            // Check to see if we're over a process unit. If there is one then we check to see if 
            // we can attach to it
            GenericProcessUnit gpu = m_canvas.GetChildAt(pos, m_newStream) as GenericProcessUnit;

            // There are 2 possible cases we can have that would send us to phase 3 from here:
            // 1. If the mouse is let up while over the same process unit that was clicked on for the source.
            // 2. If there is no source process unit and the mouse-up location is very close to the 
            //    mouse-down location that was received in phase 1.
            if ((null != gpu && object.ReferenceEquals(gpu, m_newStream.Source)) ||
                (null == m_newStream.Source && ((new MathCore.Vector(pos)) - m_phase1DownPt).Length < 5.0))
            {
                m_phase = 3;
                return;
            }

            // Initialize the list of undo actions.
            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();

            // If we attached to a source earlier then we need an undo to detach
            if (null != m_newStream.Source)
            {
                undos.Add(new DetachOutgoingStream(m_newStream.Source, m_newStream));
            }

            if (null != gpu)
            {
                if (!gpu.IsAcceptingIncomingStreams(m_newStream))
                {
                    // There's a process unit, but we can't connect to it
                    // Thus we end without a destination attachment
                    EndWithoutDestConnection(true);

                    // Go back to the selecting state
                    m_palette.SwitchToSelect();
                    return;
                }
                else
                {
                    // There's a process unit that we can connect to
                    undos.Add(new DetachIncomingStream(gpu, m_newStream));
                    gpu.AttachIncomingStream(m_newStream);
                    m_newStream.Destination = gpu;
                }
            }

            // The regardless of our source/destination situation, our undo will have to remove 
            // the stream components from the canvas
            undos.Add(new RemoveFromCanvas(m_newStream, m_canvas));
            undos.Add(new RemoveFromCanvas(m_newStream.SourceDragIcon, m_canvas));
            undos.Add(new RemoveFromCanvas(m_newStream.DestinationDragIcon, m_canvas));
            undos.Add(new RemoveFromCanvas(m_newStream.Table as UIElement, m_canvas));

            // Add the undo
            m_canvas.AddUndo(new UndoRedoCollection(
                "Undo creation of new stream", undos.ToArray()));

            // We've placed the stream so we can now show the table
            m_newStream.ShowTable(true);

            m_newStream.UpdateStreamLocation();

            // Go back to the selecting state
            m_palette.SwitchToSelect();
            return;
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
            // Nothing needed here
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        public void StateEnding()
        {
            if (m_phase > 1)
            {
                EndWithoutDestConnection(false);
                
                m_phase = -1;
            }
            else if (null != m_initialIcon)
            {
                // This means we haven't placed anything yet so we should remove the hidden stream
                m_canvas.RemoveChild(m_newStream);
                m_canvas.RemoveChild(m_newStream.SourceDragIcon);
                m_canvas.RemoveChild(m_newStream.DestinationDragIcon);
                m_canvas.RemoveChild(m_newStream.Table as UIElement);

                // Also get rid of the placement icon
                m_canvas.RemoveChild(m_initialIcon);
                m_initialIcon = null;

                // Use HighlightSelect instead of SwitchToSelect to avoid state changes
                m_palette.HighlightSelect();
            }
        }

        #endregion

        private void EndWithoutDestConnection(bool switchPaletteBackToSelect)
        {
            // In this case we've already connected the source, so we don't want to delete the 
            // stream entirely, we just want to have the destination unconnected.

            m_phase = -1;

            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();

            // If we've attached to a source then we need an undo to detach
            if (null != m_newStream.Source)
            {
                undos.Add(new DetachOutgoingStream(m_newStream.Source, m_newStream));
            }

            // Position the destination drag icon
            if (null != m_newStream.Source)
            {
                // Put it below the process unit
                m_newStream.DestinationDragIcon.Location = new Point(
                    m_newStream.Source.Location.X,
                    m_newStream.Source.Location.Y + 40.0);
            }
            else
            {
                // Put it below the source drag icon
                m_newStream.DestinationDragIcon.Location = new Point(
                    m_newStream.SourceDragIcon.Location.X,
                    m_newStream.SourceDragIcon.Location.Y + 20.0);
            }

            // The regardless of our source/destination situation, our undo will have to remove 
            // all of the stream components from the canvas
            undos.Add(new RemoveFromCanvas(m_newStream, m_canvas));
            undos.Add(new RemoveFromCanvas(m_newStream.SourceDragIcon, m_canvas));
            undos.Add(new RemoveFromCanvas(m_newStream.DestinationDragIcon, m_canvas));
            undos.Add(new RemoveFromCanvas(m_newStream.Table as UIElement, m_canvas));

            // Create an undo to delete the stream
            m_canvas.AddUndo(new UndoRedoCollection("Undo creation of new stream", undos.ToArray()));

            // Update the stream's visual stuff
            m_newStream.ShowTable(false);
            m_newStream.UpdateStreamLocation();

            if (switchPaletteBackToSelect)
            {
                m_palette.SwitchToSelect();
            }
            else
            {
                // Use HighlightSelect instead of SwitchToSelect to avoid state changes
                m_palette.HighlightSelect();
            }
        }
    }
}
