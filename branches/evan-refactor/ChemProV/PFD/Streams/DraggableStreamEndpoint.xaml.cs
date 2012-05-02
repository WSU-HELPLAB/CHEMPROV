/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ChemProV.UI.DrawingCanvas.States;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.ProcessUnits;
using ChemProV.MathCore;

namespace ChemProV.PFD.Streams
{
    /// <summary>
    /// Represents a stream endpoint that shows up as a draggable icon in the workspace. Implements 
    /// it's own mouse input logic (IState) and knows whether to accept or reject drag-drop actions 
    /// from the user.
    /// </summary>
    public partial class DraggableStreamEndpoint : UserControl, Core.ICanvasElement, IState
    {
        public enum EndpointType
        {
            StreamSourceNotConnected,
            StreamDestinationNotConnected,
            StreamSourceConnected,
            StreamDestinationConnected
        }

        #region Private member variables
        
        private DrawingCanvas m_canvas;

        /// <summary>
        /// Keeps track of whether or not this endpoint was connected to a steam when the mouse was 
        /// pressed down. A draggable stream endpoint can be one of four types
        /// 1. Unconnected source
        /// 2. Unconnected destination
        /// 3. Connected source
        /// 4. Connected destination
        /// If it's one of the first two when we the mouse is pressed down, this value will be 
        /// non-null and will contain a reference to the process unit that we were connected to. 
        /// Otherwise it will be null. If we drag around this endpoint and break a connection to a 
        /// process unit then this will be relevant for creating undos.
        /// </summary>
        private GenericProcessUnit m_connectedToOnMouseDown = null;

        private int m_id;

        /// <summary>
        /// Tracks whether the left mouse button is pressed down. This is set to true in LMB down 
        /// event and false in LMB up event.
        /// </summary>
        private bool m_isMouseDown = false;

        /// <summary>
        /// The position of this object on the canvas when the left mouse button was pressed down. These 
        /// coordinates are relative to the drawing canvas. We need this to create undo actions and 
        /// restore position if the user tries an invalid drag-drop.
        /// </summary>
        private Point m_locationOnLMBDown;
        
        private AbstractStream m_owner;

        private bool m_settingLocation = false;
        
        private EndpointType m_type;

        /// <summary>
        /// This reference is used to keep track of process units whose border color has changed to 
        /// indicate acceptance or rejection of hover-over action. When the mouse is moved off of 
        /// this item, its border color needs to be restored to normal.
        /// </summary>
        private GenericProcessUnit m_weChangedThisUnitsBorder = null;
        
        #endregion

        private static int s_idCounter = 1;

        /// <summary>
        /// Private constructor with no parameters. This is used ONLY to get the designer to work since 
        /// it needs a default constructor to show up correctly in the editor.
        /// </summary>
        private DraggableStreamEndpoint()
            : this(EndpointType.StreamSourceNotConnected, null, null)
        {
        }
        
        public DraggableStreamEndpoint(EndpointType endpointType, AbstractStream owner, DrawingCanvas canvas)
        {
            InitializeComponent();

            m_id = s_idCounter;
            s_idCounter++;

            m_canvas = canvas;
            m_owner = owner;
            m_type = endpointType;

            // Initialize the icon
            RebuildIcon();
        }

        /// <summary>
        /// Determines whether or not this endpoint can connect to the specified process unit.
        /// </summary>
        public bool CanConnectTo(IProcessUnit processUnit)
        {
            // The logic to determine this is already implemented in the process unit. We just 
            // have to identify whether we're incoming or outgoing.
            if (EndpointType.StreamDestinationNotConnected == m_type)
            {
                return processUnit.IsAcceptingIncomingStreams(m_owner) &&
                    !object.ReferenceEquals(processUnit, m_owner.Source);
            }
            else if (EndpointType.StreamSourceNotConnected == m_type)
            {
                return processUnit.IsAcceptingOutgoingStreams(m_owner) &&
                    !object.ReferenceEquals(processUnit, m_owner.Destination);
            }

            // If we're already connected then we cannot connect to anything else. Endpoints attach 
            // to only one item.
            return false;
        }

        public string Id
        {
            get
            {
                return "EGPU_" + m_id.ToString();
            }
        }

        public bool IsSource
        {
            get
            {
                switch (m_type)
                {
                    case EndpointType.StreamSourceConnected:
                    case EndpointType.StreamSourceNotConnected:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public IStream ParentStream
        {
            get
            {
                return m_owner;
            }
        }

        private void RebuildIcon()
        {
            // If we have are a connected destination endpoint then we have to procedurally build 
            // an arrow-head polygon
            if (EndpointType.StreamDestinationConnected == m_type)
            {
                AbstractStream a = m_owner as AbstractStream;

                // Get the points array for the arrow's vertices
                Point[] pts = a.GetArrowVertices();

                double minX, minY, maxX, maxY;
                minX = minY = double.MaxValue;
                maxX = maxY = double.MinValue;
                foreach (Point pt in pts)
                {
                    minX = Math.Min(minX, pt.X);
                    minY = Math.Min(minY, pt.Y);
                    maxX = Math.Max(maxX, pt.X);
                    maxY = Math.Max(maxY, pt.Y);
                }

                // Set the width and height
                Width = maxX - minX;
                Height = maxY - minY;

                // Adjust points to make them relative to this control's coordinate system
                for (int i = 0; i < 3; i++)
                {
                    ArrowIcon.Points[i] = new Point(pts[i].X - minX, pts[i].Y - minY);
                }

                // Want position such that: 
                //   Left.X + (pts[0].X - minX) = pts[0].X
                //   Left.X = minX
                // Similar thing for Y

                SetValue(Canvas.LeftProperty, minX);
                SetValue(Canvas.TopProperty, minY);

                // Use the same fill as the stem
                ArrowIcon.Fill = m_owner.Stem.Fill;

                // Make sure it's visible and the icon is hidden
                ArrowIcon.Visibility = System.Windows.Visibility.Visible;
                IconImage.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                BitmapImage bmp = new BitmapImage();
                
                // We have an icon for each of these 3 types
                switch (m_type)
                {
                    case EndpointType.StreamDestinationNotConnected:
                        bmp.UriSource = new Uri("/UI/Icons/pu_sink.png", UriKind.Relative);
                        Width = Height = 20;
                        break;

                    case EndpointType.StreamSourceConnected:
                        bmp.UriSource = new Uri("/UI/Icons/StreamSourceConnection.png", UriKind.Relative);
                        Width = Height = 12;
                        break;

                    case EndpointType.StreamSourceNotConnected:
                        bmp.UriSource = new Uri("/UI/Icons/pu_source.png", UriKind.Relative);
                        Width = Height = 20;
                        break;

                    default:
                        // Should be impossible to get here (unless breaking changes are made to this code)
                        throw new InvalidOperationException();
                }

                // Set the icon image
                IconImage.SetValue(Image.SourceProperty, bmp);
                IconImage.Visibility = System.Windows.Visibility.Visible;
                
                // Make sure the arrow is hidden
                ArrowIcon.Visibility = System.Windows.Visibility.Collapsed;
            }            
            
        }

        public EndpointType Type
        {
            get
            {
                return m_type;
            }
        }

        #region ICanvasElement Members

        /// <summary>
        /// The draggable endpoint implements the location as the midpoint of the control.
        /// 
        /// As a reminder, this assumes that the parent control to this object inherits 
        /// from System.Windows.Controls.Canvas. There is code all around the place that 
        /// assumes this, but I just thought I'd mention it again.
        /// </summary>
        public Point Location
        {
            get
            {
                return new Point(
                    (double)GetValue(Canvas.LeftProperty) + this.Width / 2.0,
                    (double)GetValue(Canvas.TopProperty) + this.Height / 2.0);
            }
            set
            {
                // IMPORTANT
                // Never try to create undos in here. The location property can be changed by a variety of 
                // code paths for a variety of different reasons. Thus, undo actions to restore location 
                // (if needed) are created at a higher level and not within this setter.

                // Avoid recursive calls
                if (m_settingLocation)
                {
                    return;
                }
                m_settingLocation = true;
                
                Point current = new Point(
                    (double)GetValue(Canvas.LeftProperty) + this.Width / 2.0,
                    (double)GetValue(Canvas.TopProperty) + this.Height / 2.0);

                // See if this is actually a change
                if (current.Equals(value))
                {
                    m_settingLocation = false;
                    return;
                }
                
                // This means the position is changing
                SetValue(Canvas.LeftProperty, value.X - this.Width / 2.0);
                SetValue(Canvas.TopProperty, value.Y - this.Height / 2.0);

                // Enpoints for a stream let each other when their locations change
                OtherEndpoint.PU_LocationChanged(null, null);

                // Update the parent stream's location, provided we are not connected
                if (EndpointType.StreamDestinationNotConnected == m_type ||
                    EndpointType.StreamSourceNotConnected == m_type)
                {
                    m_owner.UpdateStreamLocation();
                }

                m_settingLocation = false;
            }
        }

        #endregion

        #region IState Members

        public new void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public new void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public new void MouseMove(object sender, MouseEventArgs e)
        {            
            // Ignore mouse moves when the mouse button is not down. In theory this should never 
            // happen, so perhaps I should throw an exception (?)
            if (!m_isMouseDown)
            {
                return;
            }

            // Restore border color if we need to
            if (null != m_weChangedThisUnitsBorder)
            {
                m_weChangedThisUnitsBorder.SetBorderColor(ProcessUnitBorderColor.NoBorder);
                m_weChangedThisUnitsBorder = null;
            }
            
            Point pt = e.GetPosition(m_canvas);

            // If we are moving the endpoint to break a connection, then we need to do that
            if (EndpointType.StreamSourceConnected == m_type && 
                null != m_connectedToOnMouseDown)
            {
                // Break the connection on the process unit
                m_connectedToOnMouseDown.DettachOutgoingStream(m_owner);

                // Set the source to null on the stream. Note that this will invoke our type change event
                m_owner.Source = null;
            }
            else if (EndpointType.StreamDestinationConnected == m_type &&
                null != m_connectedToOnMouseDown)
            {
                // Break the connection on the process unit
                m_connectedToOnMouseDown.DettachIncomingStream(m_owner);

                // Set the destination to null on the stream. Note that this will invoke our type change event
                m_owner.Destination = null;
            }
            
            // First position this control on the drawing canvas. This will update the stream's location
            this.Location = pt;

            // If we are hovering over an element then we want to see if it's a process unit. We 
            // connect stream endpoints to process units by dragging and dropping over them, so 
            // this is a crucial piece of code.
            IProcessUnit pu = m_canvas.GetChildAt(pt, this) as IProcessUnit;

            // If it's not a process unit then we can return
            if (null == pu)
            {
                return;
            }

            // Now we need to ask the question of: if we were to drop the endpoint here, could we make 
            // a valid connection? We need to answer this question and give some sort of signal to 
            // the user to let them know if they can connect this way or not.
            
            // We are about to change the border color for this process unit
            m_weChangedThisUnitsBorder = (GenericProcessUnit)pu;

            // Set a border color based on whether or not the action is doable
            m_weChangedThisUnitsBorder.SetBorderColor(CanConnectTo(pu) ? 
                ProcessUnitBorderColor.AcceptingStreams : ProcessUnitBorderColor.NotAcceptingStreams);
        }

        public new void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_locationOnLMBDown = this.Location;
            m_isMouseDown = true;
            e.Handled = true;

            // Starting a drag selects the parent stream
            m_canvas.SelectedElement = m_owner;

            switch (m_type)
            {
                case EndpointType.StreamDestinationConnected:
                    m_connectedToOnMouseDown = m_owner.Destination as GenericProcessUnit;
                    break;

                case EndpointType.StreamSourceConnected:
                    m_connectedToOnMouseDown = m_owner.Source as GenericProcessUnit;
                    break;

                default:
                    m_connectedToOnMouseDown = null;
                    break;
            }
        }

        public new void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            
            // Only process if we're coming up from a mouse down
            if (!m_isMouseDown)
            {
                return;
            }

            // The mouse button is no longer down
            m_isMouseDown = false;

            // We can start by restoring process unit border color if it was changed
            if (null != m_weChangedThisUnitsBorder)
            {
                m_weChangedThisUnitsBorder.SetBorderColor(ProcessUnitBorderColor.NoBorder);
                m_weChangedThisUnitsBorder = null;
            }

            // Now we need to finalize the move. The logic here is a lot like the mouse move logic, in 
            // that we need to see if we're over a process unit that we can connect to.
            Point pt = e.GetPosition(m_canvas);

            // First position this control on the drawing canvas
            this.Location = pt;

            // Next, find out if we are hovering over another element
            UIElement uie = m_canvas.GetChildAt(pt, this);

            // "Cast" as process unit. Will be null if it's not
            IProcessUnit pu = uie as IProcessUnit;

            // If we aren't hovering over anything or we're hovering over something other than a process 
            // unit, then we have two possibilities:
            // 1. m_connectedToOnMouseDown is null, implying that this is just a simple drag to reposition. 
            //    In this case we just need to create an undo action to restore position
            // 2. It WAS connected originally and we have broken the connection and dropped the endpoint at 
            //    a location where there is no process unit. We need our undo to create re-attaching logic 
            //    in this case.
            // In both cases we finish up by setting the canvas state back to null.
            if (null == uie || null == pu)
            {
                if (null == m_connectedToOnMouseDown)
                {
                    m_canvas.AddUndo(new UndoRedoCollection("Undo moving stream endpoint",
                        new Undos.RestoreLocation(this, m_locationOnLMBDown)));
                    m_canvas.CurrentState = null;
                    return;
                }

                if (m_type == EndpointType.StreamDestinationNotConnected)
                {
                    m_canvas.AddUndo(new UndoRedoCollection("Undo moving stream endpoint",
                        new Undos.RestoreLocation(this, this.Location),
                        new Undos.AttachIncomingStream(m_connectedToOnMouseDown, m_owner),
                        new Undos.SetStreamDestination(m_owner, m_connectedToOnMouseDown)));
                }
                else
                {
                    m_canvas.AddUndo(new UndoRedoCollection("Undo moving stream endpoint",
                        new Undos.RestoreLocation(this, this.Location),
                        new Undos.AttachOutgoingStream(m_connectedToOnMouseDown, m_owner),
                        new Undos.SetStreamSource(m_owner, m_connectedToOnMouseDown)));
                }

                Core.App.ControlPalette.SwitchToSelect();
                return;
            }

            // Now we know we're dropping on a process unit and we need to see if it's a valid connection 
            // or not.
            if (!CanConnectTo(pu))
            {
                // This implies that it is an invalid drag-drop
                FinishBadDrag(pu as GenericProcessUnit);
                return;
            }

            // For connecting, we need to handle the different endpoint types separately.
            if (EndpointType.StreamSourceNotConnected == m_type)
            {
                if (null != m_connectedToOnMouseDown)
                {
                    // We were connected to something and broke that connection. Unless it's the exact 
                    // same process unit that we're dropping on, create an undo to detach from the one 
                    // we're about to attach to and then reattah to the old one.
                    if (object.ReferenceEquals(m_connectedToOnMouseDown, pu))
                    {
                        // No net-change was made. Reattach and return
                        if (IsSource)
                        {
                            m_connectedToOnMouseDown.AttachOutgoingStream(m_owner);
                            m_owner.Source = m_connectedToOnMouseDown;
                        }
                        else
                        {
                            m_connectedToOnMouseDown.AttachIncomingStream(m_owner);
                            m_owner.Destination = m_connectedToOnMouseDown;
                        }
                        Core.App.ControlPalette.SwitchToSelect();
                        return;
                    }

                    // Create an undo that will detach and reattach
                    m_canvas.AddUndo(new UndoRedoCollection("Undo linking stream source to different process unit",
                        new Undos.DetachOutgoingStream(pu, m_owner),
                        new Undos.AttachOutgoingStream(m_connectedToOnMouseDown, m_owner),
                        new Undos.SetStreamSource(m_owner, m_connectedToOnMouseDown)));
                }
                else
                {
                    // Create an undo that will:
                    // 1. Detach the stream from the process unit that we're about to connect it to
                    // 2. Set the stream source back to null
                    // 3. Move the draggable icon back to it was when the drag first started
                    m_canvas.AddUndo(new UndoRedoCollection(
                        "Undo linking stream source to process unit",
                        new Undos.RestoreLocation(this, m_locationOnLMBDown),
                        new Undos.DetachOutgoingStream(pu, m_owner),
                        new Undos.SetStreamSource(m_owner, null)));
                }

                // Now do the actual attaching
                pu.AttachOutgoingStream(m_owner);
                m_owner.Source = pu;
            }
            else if (EndpointType.StreamDestinationNotConnected == m_type)
            {
                if (null != m_connectedToOnMouseDown)
                {
                    // We were connected to something and broke that connection. Unless it's the exact 
                    // same process unit that we're dropping on, create an undo to detach from the one 
                    // we're about to attach to and then reattah to the old one.
                    if (object.ReferenceEquals(m_connectedToOnMouseDown, pu))
                    {
                        // No change was made - switch back to the selecting state
                        Core.App.ControlPalette.SwitchToSelect();
                        return;
                    }

                    // Create an undo that will detach and reattach
                    m_canvas.AddUndo(new UndoRedoCollection("Undo linking stream source to different process unit",
                        new Undos.DetachIncomingStream(pu, m_owner),
                        new Undos.AttachIncomingStream(m_connectedToOnMouseDown, m_owner),
                        new Undos.SetStreamDestination(m_owner, m_connectedToOnMouseDown)));
                }
                else
                {
                    // Create an undo that will:
                    // 1. Detach the stream from the process unit that we're about to connect it to
                    // 2. Set the stream destination back to null
                    // 3. Move the draggable icon back to it was when the drag first started
                    m_canvas.AddUndo(new UndoRedoCollection(
                        "Undo linking stream source to process unit",
                        new Undos.RestoreLocation(this, m_locationOnLMBDown),
                        new Undos.DetachIncomingStream(pu, m_owner),
                        new Undos.SetStreamDestination(m_owner, null)));
                }

                // Now do the actual attaching
                pu.AttachIncomingStream(m_owner);
                m_owner.Destination = pu;
            }
            else
            {
                // At this time those are the only two possibilities, so we'll never hit this code 
                // block, but to provide some resilience against breaking changes, we'll throw 
                // an exception if we get here.
                throw new InvalidOperationException(
                    "Stream endpoint was expected to be either a source or destination but was neither");
            }

            // Flip back to the selecting state
            Core.App.ControlPalette.SwitchToSelect();

            ((AbstractStream)m_owner).UpdateStreamLocation();
        }

        public new void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public new void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        public void StateEnding()
        {
        }

        #endregion

        public void EndpointConnectionChanged(EndpointType newType, GenericProcessUnit oldPU,
            GenericProcessUnit newPU)
        {
            // Whenever the source or destination process units change, BOTH stream endpoints need to 
            // attach location change listeners
            
            if (null != oldPU)
            {
                // Detach listeners
                oldPU.LocationChanged -= PU_LocationChanged;
                oldPU.LocationChanged -= OtherEndpoint.PU_LocationChanged;
            }

            if (null != newPU)
            {
                // Attach new listeners
                newPU.LocationChanged += PU_LocationChanged;
                newPU.LocationChanged += OtherEndpoint.PU_LocationChanged;
            }
            
            // Store the new type
            m_type = newType;

            RebuildIcon();

            // Invoke the location change event to update positions (if needed)
            PU_LocationChanged(null, null);
        }

        private void PU_LocationChanged(object sender, EventArgs e)
        {
            if (EndpointType.StreamDestinationConnected == m_type)
            {
                RebuildIcon();
            }
            else if (EndpointType.StreamSourceConnected == m_type)
            {
                MathCore.Vector pos = MathCore.Vector.Normalize(m_owner.StreamVector) * 30.0;
                this.Location = ((new MathCore.Vector(m_owner.Source.Location)) + pos).ToPoint();
            }
        }

        private DraggableStreamEndpoint OtherEndpoint
        {
            get
            {
                return object.ReferenceEquals(this, m_owner.SourceDragIcon) ?
                    m_owner.DestinationDragIcon : m_owner.SourceDragIcon;
            }
        }

        /// <summary>
        /// Called from the mouse-up event when we've determined that the drag that's being completed is 
        /// invalid. In this case we have to restore things back to the way they were before the drag 
        /// started. This may potentially involve reattching to a process unit.
        /// Calling this function implies that we are denying the user's drag-drop action and restoring 
        /// to a state that leaves a net-change of zero. Therefore there are no undos created because 
        /// they are not needed (we didn't change anything, so nothing to undo).
        /// </summary>
        private void FinishBadDrag(GenericProcessUnit unit)
        {
            // Restore the location of this process unit to where it was on mouse-down
            Location = m_locationOnLMBDown;

            if (null != m_connectedToOnMouseDown)
            {
                // This means that we were connected to a process unit on mouse-down and we need to 
                // reattach
                if (EndpointType.StreamSourceNotConnected == m_type)
                {
                    unit.AttachOutgoingStream(m_owner);
                    m_owner.Source = m_connectedToOnMouseDown;
                }
                else
                {
                    unit.AttachIncomingStream(m_owner);
                    m_owner.Destination = m_connectedToOnMouseDown;
                }
            }

            // Update the stream's visual stuff
            m_owner.UpdateStreamLocation();
            
            // Flip back to the default state
            Core.App.ControlPalette.SwitchToSelect();
        }
    }
}
