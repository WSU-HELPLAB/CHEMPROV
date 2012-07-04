/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the state used to place a new stream on the drawing canvas. It only handles placement 
    /// up to the point where the source has been set. It then flips over to the MovingStreamEndpoint 
    /// state to handle placement of the destination.
    /// </summary>
    public class PlacingNewStream : IState
    {
        private DrawingCanvas m_canvas;

        private UI.ControlPalette m_palette;

        /// <summary>
        /// As we move stream source and destination connectors over various process units, we 
        /// may change their border colors. If this value is non-null, then it references a 
        /// process unit whose border color has been changed and must be changed back when we 
        /// complete the placing action or move the mouse out of its area.
        /// </summary>
        private PFD.ProcessUnits.ProcessUnitControl m_puWithAlteredBorderColor = null;

        /// <summary>
        /// The source placement icon floats around under the mouse to begin with until the user 
        /// clicks the left mouse button down. At this point there are three possibilities:
        ///  1. Mouse is over a process unit that cannot be used as a stream source. In this 
        ///     case we just remove the placement icon from the canvas and we end the state.
        ///  2. Mouse is over a process unit that can be used as a stream source. In this 
        ///     case we remove this icon from the canvas.
        ///  3. Mouse is not over a process unit in which case we leave this icon where it 
        ///     is and it stays visible as we move to the next phase.
        /// </summary>
        private Image m_sourcePlacementIcon;

        /// <summary>
        /// This is the stream that is created in the constructor but not added to the workspace until 
        /// the source is placed.
        /// </summary>
        private Core.AbstractStream m_stream;

        public PlacingNewStream(UI.ControlPalette sender, DrawingCanvas canvas, StreamType streamType)
        {
            // Quick error check on parameters
            if (null == sender || null == canvas)
            {
                throw new ArgumentNullException();
            }
            if (StreamType.Chemical != streamType && StreamType.Heat != streamType)
            {
                // To save us from problems with future changes
                throw new NotImplementedException();
            }

            m_palette = sender;
            m_canvas = canvas;

            // Create the stream (data structure, not control). Take care to pick an ID that's not 
            // already in use.
            int newID;
            do
            {
                newID = Core.AbstractStream.GetNextUID();
            }
            while (canvas.GetWorkspace().StreamExists(newID));
            if (StreamType.Chemical == streamType)
            {
                m_stream = new Core.ChemicalStream(newID);
            }
            else
            {
                m_stream = new Core.HeatStream(newID);
            }

            // Create the table
            m_stream.PropertiesTable = new Core.StreamPropertiesTable(m_stream);

            // Add a default row if it's not already there
            if (0 == m_stream.PropertiesTable.RowCount)
            {
                m_stream.PropertiesTable.AddNewRow();
            }
            // Choose a default label
            m_stream.PropertiesTable.Rows[0].Label = "M" + m_stream.Id.ToString();
            // Flag it as not renamed by the user yet
            m_stream.PropertiesTable.Rows[0].UserHasRenamed = false;

            // Create the source placement icon and add it to the canvas
            m_sourcePlacementIcon = new Image();
            Uri uri = new Uri("/UI/Icons/pu_source.png", UriKind.Relative);
            ImageSource img = new System.Windows.Media.Imaging.BitmapImage(uri);
            m_sourcePlacementIcon.SetValue(Image.SourceProperty, img);
            m_canvas.AddNewChild(m_sourcePlacementIcon);
            m_sourcePlacementIcon.SetValue(Canvas.ZIndexProperty, 4);
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
            // If the canvas reference is null then we ignore mouse events
            if (null == m_canvas)
            {
                return;
            }
            
            Point mousePt = e.GetPosition(m_canvas);

            // Clear the border if we'd previously set one
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
                m_puWithAlteredBorderColor = null;
            }

            // See if we're hovering over a process unit (will be null if we're not)
            m_puWithAlteredBorderColor = 
                m_canvas.GetChildAt(e.GetPosition(m_canvas), m_sourcePlacementIcon) as ProcessUnitControl;
            
            // Set the location of icon so that it's right under the mouse pointer
            m_sourcePlacementIcon.SetValue(Canvas.LeftProperty, mousePt.X - m_sourcePlacementIcon.ActualWidth / 2.0);
            m_sourcePlacementIcon.SetValue(Canvas.TopProperty, mousePt.Y - m_sourcePlacementIcon.ActualHeight / 2.0);

            // If we're over a process unit then we need to set its border
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(
                    m_puWithAlteredBorderColor.ProcessUnit.CanAcceptOutgoingStream(m_stream) ?
                    ProcessUnitBorderColor.AcceptingStreams : ProcessUnitBorderColor.NotAcceptingStreams);
            }
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the canvas reference is null then we ignore mouse events
            if (null == m_canvas)
            {
                return;
            }

            // Here's where we need to flip over to the MovingStreamEndpoint state. We start by 
            // finding out if we clicked on a process unit or not
            
            Point location = e.GetPosition(m_canvas);

            // Clear the border if we'd previously set one
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
                m_puWithAlteredBorderColor = null;
            }

            // Remove the floating icon
            m_canvas.RemoveChild(m_sourcePlacementIcon);
            m_sourcePlacementIcon = null;

            // Get a reference to the workspace
            Core.Workspace ws = m_canvas.GetWorkspace();

            // See if we clicked on a valid destination object
            ProcessUnitControl gpu = m_canvas.GetChildAt(location, m_sourcePlacementIcon) as ProcessUnitControl;

            if (null == gpu)
            {
                // This means that we clicked in an area where there's no process unit that we have 
                // to attach to.
                m_stream.SourceLocation = new MathCore.Vector(location.X, location.Y);
                m_stream.DestinationLocation = m_stream.SourceLocation + (new MathCore.Vector(0, 20.0));
                ws.AddStream(m_stream);

                // We need an undo to remove the stream that we just added. Note that we're going to switch 
                // over to a different state below and that creates and undo as well. This is fine since 
                // it allows you to undo the destination placement and stream creation separately.
                ws.AddUndo(new Core.UndoRedoCollection("Undo stream creation", 
                    new Logic.Undos.RemoveStream(m_stream)));
            }
            else if (gpu.ProcessUnit.CanAcceptOutgoingStream(m_stream))
            {
                // Setup the stream and add it to the workspace                
                m_stream.SourceLocation = new MathCore.Vector(location.X, location.Y);
                m_stream.DestinationLocation = m_stream.SourceLocation + (new MathCore.Vector(0, 35.0));
                m_stream.Source = gpu.ProcessUnit;
                ws.AddStream(m_stream);

                // Attach the outgoing stream to the process unit after we've added the stream to
                // the workspace
                gpu.ProcessUnit.AttachOutgoingStream(m_stream);
                
                // We've linked up the stream and added it to the workspace. Now create an undo that 
                // does the opposite
                ws.AddUndo(new Core.UndoRedoCollection("Undo stream creation",
                    new Logic.Undos.DetachOutgoingStream(gpu.ProcessUnit, m_stream),
                    new Logic.Undos.RemoveStream(m_stream)));
            }
            else
            {
                // This means we clicked on a process unit that we cannot connect to. In this case 
                // we want to cancel everything. This is easy since we haven't modified the workspace, 
                // so all we have to do it switch back to select mode.
                m_palette.SwitchToSelect();
                return;
            }

            // Get the stream control that the DrawingCanvas should have added
            PFD.Streams.AbstractStream streamControl = m_canvas.GetStreamControl(m_stream);

            // Tell it to hide the table
            streamControl.HideTable();

            // Set references to null to indicate that this state should no longer process mouse input 
            // messages after this. We are finishing up right here.
            DrawingCanvas c = m_canvas;
            m_canvas = null;
            m_stream = null;

            // Switch back to select mode
            m_palette.SwitchToSelect();

            // Flip over to the state where we position the stream destination
            c.CurrentState = new MovingStreamEndpoint(streamControl.DestinationDragIcon, c, true);
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }

        public void MouseWheel(object sender, MouseEventArgs e) { }

        public void LostMouseCapture(object sender, MouseEventArgs e) { }

        public void StateEnding()
        {
            // If the canvas reference is non-null then this means we never placed the 
            // source and need to just cancel the stream creation. This is easy because 
            // we never added anything to the workspace, so all we need to do is remove 
            // the placement icon.
            if (null != m_canvas)
            {
                m_canvas.RemoveChild(m_sourcePlacementIcon);

                // Use HighlightSelect instead of SwitchToSelect to avoid state changes
                m_palette.HighlightSelect();
            }
        }

        #endregion

        //private void EndWithoutDestConnection(bool switchPaletteBackToSelect)
        //{
        //    // In this case we've already connected the source, so we don't want to delete the 
        //    // stream entirely, we just want to have the destination unconnected.

        //    m_phase = -1;

        //    List<ChemProV.Core.IUndoRedoAction> undos = new List<ChemProV.Core.IUndoRedoAction>();

        //    // If we've attached to a source then we need an undo to detach
        //    if (null != m_newStream.Source)
        //    {
        //        undos.Add(new DetachOutgoingStream(m_newStream.Source, m_newStream));
        //    }

        //    // Position the destination drag icon
        //    if (null != m_newStream.Source)
        //    {
        //        // Put it below the process unit
        //        m_newStream.DestinationDragIcon.Location = new Point(
        //            m_newStream.Source.Location.X,
        //            m_newStream.Source.Location.Y + 40.0);
        //    }
        //    else
        //    {
        //        // Put it below the source drag icon
        //        m_newStream.DestinationDragIcon.Location = new Point(
        //            m_newStream.SourceDragIcon.Location.X,
        //            m_newStream.SourceDragIcon.Location.Y + 20.0);
        //    }

        //    // The regardless of our source/destination situation, our undo will have to remove 
        //    // all of the stream components from the canvas
        //    undos.Add(new RemoveFromCanvas(m_newStream, m_canvas));
        //    undos.Add(new RemoveFromCanvas(m_newStream.SourceDragIcon, m_canvas));
        //    undos.Add(new RemoveFromCanvas(m_newStream.DestinationDragIcon, m_canvas));
        //    undos.Add(new RemoveFromCanvas(m_newStream.Table as UIElement, m_canvas));

        //    // Create an undo to delete the stream
        //    m_canvas.GetWorkspace().AddUndo(new ChemProV.Core.UndoRedoCollection(
        //        "Undo creation of new stream", undos.ToArray()));

        //    // Update the stream's visual stuff
        //    m_newStream.ShowTable(false);
        //    m_newStream.UpdateStreamLocation();

        //    if (switchPaletteBackToSelect)
        //    {
        //        m_palette.SwitchToSelect();
        //    }
        //    else
        //    {
        //        // Use HighlightSelect instead of SwitchToSelect to avoid state changes
        //        m_palette.HighlightSelect();
        //    }
        //}
    }
}
