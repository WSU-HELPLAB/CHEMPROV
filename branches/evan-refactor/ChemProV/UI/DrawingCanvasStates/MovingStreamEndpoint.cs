using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemProV.MathCore;
using ChemProV.PFD.Streams;
using ChemProV.Logic;

namespace ChemProV.UI.DrawingCanvasStates
{
    public class MovingStreamEndpoint : IState
    {
        private DrawingCanvas m_canvas;

        private ProcessUnitControl m_changedBorderColor = null;

        private AbstractProcessUnit m_connectedToOnStart;

        private bool m_creatingStream;

        /// <summary>
        /// Reference to the control being used as the drag icon. We store a reference to this 
        /// because when we're looking for controls that the mouse is hovering over on the drawing 
        /// canvas we need to make sure that we ignore this one.
        /// We are NOT responsible for setting the location of the control on the canvas. That is done 
        /// for us in event handlers after we change the location in m_stream.
        /// </summary>
        private DraggableStreamEndpoint m_dragIcon;
        
        /// <summary>
        /// Stores the location of the stream endpoint when the move first started
        /// </summary>
        private Vector m_startLocation;

        private AbstractStream m_stream;

        public MovingStreamEndpoint(DraggableStreamEndpoint endpoint, 
            DrawingCanvas canvas, bool creatingStream)
        {
            m_dragIcon = endpoint;
            m_canvas = canvas;
            m_creatingStream = creatingStream;
            m_stream = endpoint.ParentStream.Stream;

            if (m_dragIcon.IsSource)
            {
                m_startLocation = m_stream.SourceLocation;
                m_connectedToOnStart = m_stream.Source;
            }
            else
            {
                m_startLocation = m_stream.DestinationLocation;
                m_connectedToOnStart = m_stream.Destination;
            }
        }

        public void MouseEnter(object sender, MouseEventArgs e) { }

        public void MouseLeave(object sender, MouseEventArgs e) { }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            // If the stream is null then we don't process events
            if (null == m_stream)
            {
                return;
            }
            
            // Get the mouse position relative to the canvas
            Point p = e.GetPosition(m_canvas);

            // Clear the border color if we had set one
            if (null != m_changedBorderColor)
            {
                m_changedBorderColor.SetBorderColor(ProcessUnitBorderColor.NoBorder);
                m_changedBorderColor = null;
            }

            // Find out if we're hovering over a process unit
            ProcessUnitControl lpu = m_canvas.GetChildAt(p, m_dragIcon) as ProcessUnitControl;
            if (null == lpu)
            {
                // All we need to do on a mouse move is make the changes in the m_stream data structure. The 
                // application is designed such that UI controls monitor changes in these data structures and 
                // update themselves appropriately.
                if (m_dragIcon.IsSource)
                {
                    // If the source is not null, we need to disconnect
                    if (null != m_stream.Source)
                    {
                        m_stream.Source.DetachOutgoingStream(m_stream);
                        m_stream.Source = null;
                    }
                    m_stream.SourceLocation = new Vector(p.X, p.Y);
                }
                else
                {
                    // If the destination is not null, we need to disconnect
                    if (null != m_stream.Destination)
                    {
                        m_stream.Destination.DetachIncomingStream(m_stream);
                        m_stream.Destination = null;
                    }
                    m_stream.DestinationLocation = new Vector(p.X, p.Y);
                }
            }
            else
            {
                // We are hovering over a process unit
                m_changedBorderColor = lpu;
                if (m_dragIcon.IsSource)
                {
                    if (object.ReferenceEquals(m_stream.Source, lpu.ProcessUnit))
                    {
                        // This means that the process unit we're dragging the source icon over is 
                        // already set as the source, so we don't have to do anything.
                    }
                    else if (lpu.ProcessUnit.CanAcceptOutgoingStream(m_stream))
                    {
                        // If we had a non-null source then disconnect
                        if (null != m_stream.Source)
                        {
                            m_stream.Source.DetachOutgoingStream(m_stream);
                        }

                        m_stream.Source = lpu.ProcessUnit;
                        lpu.ProcessUnit.AttachOutgoingStream(m_stream);

                        // Set the border color to indicate that we can make this connection
                        lpu.SetBorderColor(ProcessUnitBorderColor.AcceptingStreams);
                    }
                    else
                    {
                        // If we had a non-null source then disconnect
                        if (null != m_stream.Source)
                        {
                            m_stream.Source.DetachOutgoingStream(m_stream);
                            m_stream.Source = null;
                        }

                        // This means that the connection cannot be made
                        lpu.SetBorderColor(ProcessUnitBorderColor.NotAcceptingStreams);
                        m_stream.SourceLocation = new Vector(p.X, p.Y);
                    }
                }
                else
                {
                    if (object.ReferenceEquals(m_stream.Destination, lpu.ProcessUnit))
                    {
                        // This means that the process unit we're dragging the destination icon over 
                        // is already set as the destination, so we don't have to do anything.
                    }
                    else if (lpu.ProcessUnit.CanAcceptIncomingStream(m_stream))
                    {
                        // If we had a non-null destination then disconnect
                        if (null != m_stream.Destination)
                        {
                            m_stream.Destination.DetachIncomingStream(m_stream);
                        }

                        m_stream.Destination = lpu.ProcessUnit;
                        lpu.ProcessUnit.AttachIncomingStream(m_stream);

                        // Set the border color to indicate that we can make this connection
                        lpu.SetBorderColor(ProcessUnitBorderColor.AcceptingStreams);
                    }
                    else
                    {
                        // If we had a non-null destination then disconnect
                        if (null != m_stream.Destination)
                        {
                            m_stream.Destination.DetachIncomingStream(m_stream);
                            m_stream.Destination = null;
                        }

                        // This means that the connection cannot be made
                        lpu.SetBorderColor(ProcessUnitBorderColor.NotAcceptingStreams);
                        m_stream.DestinationLocation = new Vector(p.X, p.Y);
                    }
                }
            }
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_creatingStream)
            {
                // Button down and button up do the same thing
                MouseLeftButtonUp(sender, e);
            }
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If the stream is null then we don't process events
            if (null == m_stream)
            {
                return;
            }
            
            Point p = e.GetPosition(m_canvas);
            
            // We handle things in a special way if we're creating a stream. If the mouse is 
            // let up near the source location then we go to "click-click" creation mode 
            // instead of "click-drag-release" creation mode.
            MathCore.Vector v = new Vector(p.X, p.Y);
            if (m_creatingStream && (v - m_stream.SourceLocation).Length < 10.0)
            {
                // Ignore this event and wait for the next mouse down
                return;
            }

            // Clear the border color if we had set one
            if (null != m_changedBorderColor)
            {
                m_changedBorderColor.SetBorderColor(ProcessUnitBorderColor.NoBorder);
                m_changedBorderColor = null;
            }
            
            // If the mouse is let up over a process unit that we cannot connect to, then send the 
            // endpoint back to where it was originally.
            ProcessUnitControl puc = m_canvas.GetChildAt(p, m_dragIcon) as ProcessUnitControl;
            if (null != puc)
            {
                if ((m_dragIcon.IsSource && object.ReferenceEquals(puc.ProcessUnit, m_stream.Source)) ||
                    (!m_dragIcon.IsSource && object.ReferenceEquals(puc.ProcessUnit, m_stream.Destination)))
                {
                    // This means we've already connected and we're good to go
                }
                else if (!m_dragIcon.CanConnectTo(puc))
                {
                    // Set the state back to what it was when we started
                    if (m_dragIcon.IsSource)
                    {
                        m_stream.SourceLocation = m_startLocation;
                        m_stream.Source = m_connectedToOnStart;
                    }
                    else
                    {
                        m_stream.DestinationLocation = m_startLocation;
                        m_stream.Destination = m_connectedToOnStart;
                    }

                    m_stream = null;
                    Core.App.ControlPalette.SwitchToSelect();
                    return;
                }
            }
            
            StateEnding();
            Core.App.ControlPalette.SwitchToSelect();
        }

        public void MouseWheel(object sender, MouseEventArgs e) { }

        public void LostMouseCapture(object sender, MouseEventArgs e) { }

        public void StateEnding()
        {
            // If the stream is null then we don't process events
            if (null == m_stream)
            {
                return;
            }

            // Clear the border color if we had set one
            if (null != m_changedBorderColor)
            {
                m_changedBorderColor.SetBorderColor(ProcessUnitBorderColor.NoBorder);
                m_changedBorderColor = null;
            }

            // Create an undo
            Workspace ws = m_canvas.GetWorkspace();
            if (m_dragIcon.IsSource)
            {
                // There are 4 cases for what could have happened:
                //  1. Source was originally null and still is
                //  2. Source was originally null and now isn't
                //  3. Source was originally non-null and now is null
                //  4. Source was originally non-null and still is non-null
                //    4a. It is the same
                //    4b. It is different

                if (null == m_connectedToOnStart && null == m_stream.Source)
                {
                    // This is just a move of the endpoint from one location to another
                    ws.AddUndo(new UndoRedoCollection("Undo moving stream source",
                        new Logic.Undos.SetStreamSourceLocation(m_stream, m_startLocation)));
                }
                else if (null == m_connectedToOnStart && null != m_stream.Source)
                {
                    ws.AddUndo(new UndoRedoCollection("Undo changing stream source",
                        new Logic.Undos.SetStreamSource(m_stream, null, m_stream.Source, m_startLocation),
                        new Logic.Undos.DetachOutgoingStream(m_stream.Source, m_stream)));
                }
                else if (null != m_connectedToOnStart && null == m_stream.Source)
                {
                    ws.AddUndo(new UndoRedoCollection("Undo detaching stream source",
                        new Logic.Undos.SetStreamSource(m_stream, m_connectedToOnStart, null, m_stream.SourceLocation),
                        new Logic.Undos.AttachOutgoingStream(m_connectedToOnStart, m_stream)));
                }
                else
                {
                    // Only take action if we've changed the source
                    if (!object.ReferenceEquals(m_connectedToOnStart, m_stream.Source))
                    {
                        ws.AddUndo(new UndoRedoCollection("Undo changing stream source",
                            new Logic.Undos.SetStreamSource(m_stream, m_connectedToOnStart, m_stream.Source, m_stream.SourceLocation),
                            new Logic.Undos.DetachOutgoingStream(m_stream.Source, m_stream),
                            new Logic.Undos.AttachOutgoingStream(m_connectedToOnStart, m_stream)));
                    }
                }
            }
            else
            {
                // There are 4 cases for what could have happened:
                //  1. Destination was originally null and still is
                //  2. Destination was originally null and now isn't
                //  3. Destination was originally non-null and now is null
                //  4. Destination was originally non-null and still is non-null
                //    4a. It is the same
                //    4b. It is different

                if (null == m_connectedToOnStart && null == m_stream.Destination)
                {
                    // This is just a move of the endpoint from one location to another
                    ws.AddUndo(new UndoRedoCollection("Undo moving stream destination",
                        new Logic.Undos.SetStreamDestinationLocation(m_stream, m_startLocation)));
                }
                else if (null == m_connectedToOnStart && null != m_stream.Destination)
                {
                    ws.AddUndo(new UndoRedoCollection("Undo changing stream destination",
                        new Logic.Undos.SetStreamDestination(m_stream, null, m_stream.Destination, m_startLocation),
                        new Logic.Undos.DetachIncomingStream(m_stream.Destination, m_stream)));
                }
                else if (null != m_connectedToOnStart && null == m_stream.Destination)
                {
                    ws.AddUndo(new UndoRedoCollection("Undo detaching stream destination",
                        new Logic.Undos.SetStreamDestination(m_stream, m_connectedToOnStart, null, m_stream.DestinationLocation),
                        new Logic.Undos.AttachIncomingStream(m_connectedToOnStart, m_stream)));
                }
                else
                {
                    // Only take action if we've changed the destination
                    if (!object.ReferenceEquals(m_connectedToOnStart, m_stream.Destination))
                    {
                        ws.AddUndo(new UndoRedoCollection("Undo changing stream destination",
                            new Logic.Undos.SetStreamDestination(m_stream, m_connectedToOnStart, m_stream.Destination, m_stream.DestinationLocation),
                            new Logic.Undos.DetachIncomingStream(m_stream.Destination, m_stream),
                            new Logic.Undos.AttachIncomingStream(m_connectedToOnStart, m_stream)));
                    }
                }
            }

            if (m_creatingStream)
            {
                // Tell the stream control to show the table
                m_dragIcon.ParentStream.ShowTable(true);
            }

            // Set the stream reference to null to end this state
            m_stream = null;
        }
    }
}
