/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using ChemProV.Core;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.PFD.Streams
{
    /// <summary>
    /// TODO: Rename to StreamControl. This class is not abstract and is the only stream control.
    /// </summary>
    public partial class AbstractStream : UserControl, IPfdElement, UI.ICommentControlManager, ICanvasElement
    {
        #region instance vars

        public event EventHandler SelectionChanged = null;
        public bool m_isSelected = false;

        private Polygon m_arrow;

        /// <summary>
        /// The stream keeps a reference to its parent drawing canvas. Note that this will be 
        /// null if you use the default constructor.
        /// </summary>
        protected DrawingCanvas m_canvas = null;

        /// <summary>
        /// Keeps a reference to the destination process unit for the stream whose property 
        /// change events are being monitored. When the destination of the stream changes, we 
        /// need to unsubscribe from this item and then set the reference to the new destination.
        /// </summary>
        private Core.AbstractProcessUnit m_destEventItem = null;

        /// <summary>
        /// This is the draggable stream destination icon that the user can drag and drop over a 
        /// process unit to make a connection (if allowed). It is created in the constructor and 
        /// stays on the drawing canvas for as long as this stream control does, but it is hidden 
        /// when the stream has a non-null destination.
        /// </summary>
        private DraggableStreamEndpoint m_dstDragIcon = null;

        /// <summary>
        /// A reference to the control to use as the table when it is minimized. This stays null 
        /// until first minimization and which point it is created.
        /// </summary>
        private UI.MiniStreamTable m_miniTable = null;

        /// <summary>
        /// This is the source drag icon. The user can click on it (when it's visible) to initiate 
        /// dragging and can drag it over process units to connect to them. After construction this 
        /// is never null, since it is used for both connected and unconnected source states.
        /// </summary>
        private DraggableStreamEndpoint m_sourceDragIcon = null;

        /// <summary>
        /// Keeps a reference to the source process unit for the stream whose property change 
        /// events are being monitored. When the source of the stream changes, we need to 
        /// unsubscribe from this item and then set the reference to the new source.
        /// </summary>
        private Core.AbstractProcessUnit m_sourceEventItem = null;

        private Shape m_square = null;

        /// <summary>
        /// The list of sticky note controls that have been created to represent comments for this 
        /// stream. Remember that the actual comments for the stream are stored in the 
        /// stream data structure (m_stream) and this list updates as the list of comments in that 
        /// object change.
        /// We store this because the stream is responsible for creating and removing its sticky 
        /// note controls on the drawing canvas.
        /// </summary>
        private List<StickyNote.StickyNoteControl> m_stickyNotes = new List<StickyNote.StickyNoteControl>();

        private ChemProV.Core.AbstractStream m_stream;

        /// <summary>
        /// Brush used for the stream line when it is not selected. Built in the constructor and 
        /// will depend on the type of stream.
        /// </summary>
        private Brush m_streamLineNotSelected;

        protected UI.StreamTableControl m_table = null;

        private bool m_tableMinimized = false;

        private bool m_updatingLocation = false;

        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// TODO: Remove
        /// </summary>
        private string streamId;

        #endregion instance vars

        #region ChemProV.PFD.Streams.AbstractStream Members

        /// <summary>
        /// Gets or sets the stream's unique ID number
        /// TODO: Remove
        /// </summary>
        public String Id
        {
            get
            {
                return streamId;
            }
            set
            {
                streamId = value;
            }
        }

        public MathCore.Vector StreamVector
        {
            get
            {
                return m_stream.DestinationLocation - m_stream.SourceLocation;
            }
        }

        ///// <summary>
        ///// Reference to the stream's source PFD element
        ///// </summary>
        //public GenericProcessUnit Source
        //{
        //    get
        //    {
        //        return m_source;
        //    }
        //    set
        //    {
        //        GenericProcessUnit old = m_source as GenericProcessUnit;
                
        //        //remove the event listener from the old source
        //        if (m_source != null)
        //        {
        //            m_source.LocationChanged -= new EventHandler(AttachedLocationChanged);
        //        }

        //        //set new source, attach new listener
        //        m_source = value;
        //        TemporaryProcessUnit tpu = value as TemporaryProcessUnit;
        //        if (null != tpu)
        //        {
        //            // This really means that it's an unconnected endpoint
        //            m_srcDragIcon.EndpointConnectionChanged(
        //                DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected,
        //                old, value as GenericProcessUnit);
        //            m_source = null;
        //            UpdateStreamLocation();
        //        }
        //        else if (m_source != null)
        //        {
        //            // Update the source connection draggable icon
        //            m_srcDragIcon.EndpointConnectionChanged(
        //                DraggableStreamEndpoint.EndpointType.StreamSourceConnected,
        //                old, value as GenericProcessUnit);
                    
        //            m_source.LocationChanged += new EventHandler(AttachedLocationChanged);
        //            UpdateStreamLocation();
        //        }
        //        else
        //        {
        //            if (null != old)
        //            {
        //                m_srcDragIcon.EndpointConnectionChanged(
        //                    DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected,
        //                    old, null);

        //                UpdateStreamLocation();
        //            }

        //            // Note that the "else" case here is that it was already null, in which case 
        //            // we shouldn't need to change anything
        //        }
        //    }
        //}

        ///// <summary>
        ///// Reference to the stream's destination PFD element
        ///// </summary>
        //public GenericProcessUnit Destination
        //{
        //    get
        //    {
        //        return m_destination;
        //    }
        //    set
        //    {
        //        GenericProcessUnit old = m_destination as GenericProcessUnit;
                
        //        // Remove the event listener from the old destination
        //        if (m_destination != null)
        //        {
        //            m_destination.LocationChanged -= new EventHandler(AttachedLocationChanged);
        //        }

        //        // Add the new destination and attach listener
        //        m_destination = value;
        //        if (m_destination != null)
        //        {
        //            m_dstDragIcon.EndpointConnectionChanged(
        //                DraggableStreamEndpoint.EndpointType.StreamDestinationConnected,
        //                old, value as GenericProcessUnit);
                    
        //            m_destination.LocationChanged += new EventHandler(AttachedLocationChanged);
        //            UpdateStreamLocation();
        //        }
        //        else
        //        {
        //            if (null != old)
        //            {
        //                m_dstDragIcon.EndpointConnectionChanged(
        //                    DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected,
        //                    old, value as GenericProcessUnit);
        //                UpdateStreamLocation();
        //            }

        //            // Note that the "else" case here is that it was already null, in which case 
        //            // we shouldn't need to change anything
        //        }
        //    }
        //}

        public DraggableStreamEndpoint DestinationDragIcon
        {
            get
            {
                return m_dstDragIcon;
            }
        }

        public UI.StreamTableControl Table
        {
            get
            {
                return m_table;
            }
        }

        /// <summary>
        /// Gets the point on the border of the destination process unit where the stream line 
        /// connects to. This is only relevant if the stream has a non-null destination process 
        /// unit. If the destination is null then the point (0,0) is returned.
        /// </summary>
        public MathCore.Vector DestinationConnectionPoint
        {
            get
            {
                if (null == m_stream.Destination)
                {
                    return new MathCore.Vector();
                }

                // Get the center point for the source (which could be a process unit if 
                // we're connected to one or a drag-handle if we're not)
                MathCore.Vector srcCenter = m_stream.SourceLocation;

                ProcessUnitControl dstUnit = m_canvas.GetProcessUnitControl(m_stream.Destination);

                // Make a rectangle for the destination process unit
                MathCore.Rectangle rDest;
                if (null == dstUnit)
                {
                    Point topLeft = new Point(
                        m_stream.DestinationLocation.X - 20.0,
                        m_stream.DestinationLocation.Y - 20.0);
                    rDest = MathCore.Rectangle.CreateFromCanvasRect(topLeft, 40.0, 40.0);
                }
                else
                {
                    rDest = MathCore.Rectangle.CreateFromCanvasRect(
                        new Point((double)dstUnit.GetValue(Canvas.LeftProperty),
                            (double)dstUnit.GetValue(Canvas.TopProperty)),
                        dstUnit.Width, dstUnit.Height);
                }

                // Build the line segment between the two process units
                MathCore.LineSegment ls = new MathCore.LineSegment(
                    srcCenter, m_stream.DestinationLocation);

                MathCore.Vector[] pts = rDest.GetIntersections(ls);
                if (0 == pts.Length)
                {
                    return m_stream.DestinationLocation;
                }

                return pts[0];
            }
        }

        public MathCore.Vector[] GetArrowVertices()
        {
            MathCore.Vector[] pts = new MathCore.Vector[3];

            // Start by getting the normalized connection line vector
            MathCore.Vector v = MathCore.Vector.Normalize(StreamVector);

            // Also get the connection point
            MathCore.Vector tip = DestinationConnectionPoint;

            // Build perpendicular MathCore.Vectors
            MathCore.Vector perp1 = MathCore.Vector.GetPerpendicular1(v) * 15.0;
            MathCore.Vector perp2 = MathCore.Vector.GetPerpendicular2(v) * 15.0;

            // Build the arrow
            pts[0] = tip;
            pts[1] = ((tip - (v * 15.0)) + perp1);
            pts[2] = ((tip - (v * 15.0)) + perp2);

            return pts;
        }

        /// <summary>
        /// Gets or sets the selection flag for the stream
        /// </summary>
        public bool Selected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                if (m_isSelected.Equals(value))
                {
                    // No change -> nothing to do
                    return;
                }

                m_isSelected = value;

                // Set the stem color based on selection state
                this.Stem.Stroke = m_isSelected ?
                    new SolidColorBrush(Colors.Yellow) : m_streamLineNotSelected;

                if (null != SelectionChanged)
                {
                    SelectionChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid source. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more outgoing 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid source, false otherwise.</returns>
        public bool IsValidSource(ProcessUnitControl unit)
        {
            return m_stream.IsValidSource(
                (unit as ChemProV.PFD.ProcessUnits.ProcessUnitControl).ProcessUnit);
        }

        /// <summary>
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid destination. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more incoming 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid destination, false otherwise.</returns>
        public bool IsValidDestination(ProcessUnitControl unit)
        {
            return m_stream.IsValidDestination(
                (unit as ChemProV.PFD.ProcessUnits.ProcessUnitControl).ProcessUnit);
        }

        #endregion ChemProV.PFD.Streams.AbstractStream Members

        /// <summary>
        /// Can be called to manually update the stream's location
        /// </summary>
        public virtual void UpdateStreamLocation()
        {
            if (m_updatingLocation)
            {
                return;
            }
            m_updatingLocation = true;

            MathCore.Vector sPt = m_stream.SourceLocation;
            MathCore.Vector dPt = m_stream.DestinationLocation;
            MathCore.Vector lineMidpoint = (sPt + dPt) / 2.0;
            
            // Start by positioning the stem line that connects source and destination
            // TODO: Rework this for boxed lines
            Stem.X1 = sPt.X;
            Stem.Y1 = sPt.Y;
            Stem.X2 = dPt.X;
            Stem.Y2 = dPt.Y;

            // Take care of the source drag icon
            MathCore.Vector sIconPt = MathCore.Vector.Normalize(dPt - sPt) * 32.0;
            m_sourceDragIcon.Location = new Point(sPt.X, sPt.Y);
            if (null != m_stream.Source)
            {
                m_sourceDragIcon.Visibility = System.Windows.Visibility.Collapsed;
                // We need to show the square drag handle and position it
                if (null == m_square)
                {
                    m_square = new System.Windows.Shapes.Rectangle()
                    {
                        Fill = m_streamLineNotSelected,
                        Width = 10.0,
                        Height = 10.0
                    };
                    m_canvas.AddNewChild(m_square);
                    m_square.MouseLeftButtonDown += new MouseButtonEventHandler(SourceSquare_MouseLeftButtonDown);
                }
                m_square.Visibility = System.Windows.Visibility.Visible;
                MathCore.Vector location = sPt + (MathCore.Vector.Normalize(dPt - sPt) * 30.0);
                m_square.SetValue(Canvas.LeftProperty, location.X - 5.0);
                m_square.SetValue(Canvas.TopProperty, location.Y - 5.0);
            }
            else
            {
                m_sourceDragIcon.Visibility = System.Windows.Visibility.Visible;
                if (null != m_square)
                {
                    m_square.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            // Take care of the destination drag icon as well
            if (null == m_stream.Destination)
            {
                m_dstDragIcon.Location = new Point(dPt.X, dPt.Y);
                m_dstDragIcon.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                m_dstDragIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Now do the table line
            if (null != m_table && !m_tableMinimized)
            {
                TableLine.X1 = lineMidpoint.X;
                TableLine.Y1 = lineMidpoint.Y;
                TableLine.X2 = m_table.Location.X;
                TableLine.Y2 = m_table.Location.Y;
            }

            // If we're minimized then we need to position the mini table in the middle of the 
            // stream line
            if (m_tableMinimized)
            {
                m_miniTable.SetValue(Canvas.LeftProperty, lineMidpoint.X);
                m_miniTable.SetValue(Canvas.TopProperty, lineMidpoint.Y);
            }

            if (null != m_stream.Destination)
            {
                UpdateAndShowArrow();
            }

            m_updatingLocation = false;
        }

        private void SourceSquare_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (null == m_canvas.CurrentState)
            {
                // If we've clicked a drag endpoint them select the stream
                m_canvas.SelectedElement = this;

                // Set state to moving endpoint state
                m_canvas.CurrentState = new UI.DrawingCanvas.States.MovingStreamEndpoint(
                    m_sourceDragIcon, m_canvas, false);
            }
        }

        private void UpdateAndShowArrow()
        {
            // Get the points array for the arrow's vertices
            MathCore.Vector[] pts = GetArrowVertices();

            if (3 != m_arrow.Points.Count)
            {
                // We need to add.
                foreach (MathCore.Vector pt in pts)
                {
                    m_arrow.Points.Add(new Point(pt.X, pt.Y));
                }
            }
            else
            {
                // We need to set. I assume that setting is more efficient than clearing and re-adding, 
                // but it may not be that big of a deal.
                for (int i = 0; i < 3; i++)
                {
                    m_arrow.Points[i] = new Point(pts[i].X, pts[i].Y);
                }
            }

            m_arrow.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Private constructor. This is used only to get designed view to work. IT MUST NOT BE 
        /// MADE PUBLIC. For correct initialization the stream needs a DrawingCanvas reference.
        /// </summary>
        private AbstractStream()
            : this(null, null)
        {

        }

        private AbstractStream(DrawingCanvas canvas, ChemProV.Core.AbstractStream stream)
        {
            m_canvas = canvas;
            m_stream = stream;
            
            InitializeComponent();

            if (null != canvas && null != stream)
            {
                // Create the brush for the stream line
                if (stream is Core.HeatStream)
                {
                    m_streamLineNotSelected = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    m_streamLineNotSelected = new SolidColorBrush(Colors.Black);
                }
                this.Stem.Stroke = m_streamLineNotSelected;
                
                // Create the drag handle for the source
                m_sourceDragIcon = new DraggableStreamEndpoint(
                    DraggableStreamEndpoint.EndpointType.StreamSource,
                    this, canvas);
                m_sourceDragIcon.Width = m_sourceDragIcon.Height = 20.0;
                m_sourceDragIcon.Location = new Point(m_stream.SourceLocation.X, m_stream.SourceLocation.Y);
                // Add it to the canvas
                m_canvas.AddNewChild(m_sourceDragIcon);
                // Set the Z-index
                m_sourceDragIcon.SetValue(Canvas.ZIndexProperty, 2);
                // Setup mouse events
                m_sourceDragIcon.MouseLeftButtonDown += new MouseButtonEventHandler(DragIcon_MouseLeftButtonDown);

                // Create the drag handle for the destination
                m_dstDragIcon = new DraggableStreamEndpoint(
                    DraggableStreamEndpoint.EndpointType.StreamDestination,
                    this, canvas);
                // Add it to the canvas
                m_canvas.AddNewChild(m_dstDragIcon);
                // Set the Z-index
                m_dstDragIcon.SetValue(Canvas.ZIndexProperty, 2);
                // Show it if there's no destination, hide it otherwise
                m_dstDragIcon.Visibility = (null == m_stream.Destination) ? 
                    Visibility.Visible : System.Windows.Visibility.Collapsed;
                // Setup mouse events
                m_dstDragIcon.MouseLeftButtonDown += new MouseButtonEventHandler(DragIcon_MouseLeftButtonDown);

                // Create the arrow polygon control
                m_arrow = new Polygon();
                // Use the same fill as the stem
                m_arrow.Fill = Stem.Fill;
                // Add it to the canvas
                m_canvas.AddNewChild(m_arrow);
                // Set the Z-index
                m_arrow.SetValue(Canvas.ZIndexProperty, 2);
                // Update the vertices
                UpdateAndShowArrow();
                // Hide it if there's no destination
                if (null == m_stream.Destination)
                {
                    m_arrow.Visibility = Visibility.Collapsed;
                }
                // Setup mouse events
                if (!(m_stream.Destination is Core.HeatExchangerWithUtility))
                {
                    m_arrow.MouseLeftButtonDown += new MouseButtonEventHandler(DestinationArrow_MouseLeftButtonDown);
                }

                // Make sure the stem line has a low z-index
                Stem.SetValue(Canvas.ZIndexProperty, -1);

                UpdateStreamLocation();

                // Create the table
                CreatePropertiesTable(m_stream.PropertiesTable);

                // Watch for when the table location changes
                m_stream.PropertiesTable.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName.Equals("Location"))
                    {
                        UpdateStreamLocation();
                    }
                };

                // Add it to the canvas and set it up
                m_canvas.AddNewChild(m_table as UIElement);

                UserControl tableAsUiElement = m_table as UserControl;

                //This sets the tables index to the greatest so it will be above everything
                tableAsUiElement.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 3);

                tableAsUiElement.MouseLeftButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonDownHandler);
                tableAsUiElement.MouseLeftButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonUpHandler);
            
                // Hook up event listeners to the stream if non-null. We need to monitor:
                //  1. Changes in the comment collection
                //  2. Changes to the source process units properties (position)
                //  3. Changes to the destination process units properties (position)
                //  4. Changes to the source or destination references
                //  5. Changes to source or destination location changes (if source or destination are non-null)

                // 1.
                // Setup the event listener for the comment collection. It is the responsibility of this 
                // control to create and manage the sticky note controls for its comments
                m_stream.Comments.CollectionChanged += Comments_CollectionChanged;
                // Invoke the callback to create sticky notes for comments
                Comments_CollectionChanged(m_stream.Comments, null);

                // 2.
                if (null != stream.Source)
                {
                    m_sourceEventItem = stream.Source;
                    stream.Source.PropertyChanged += this.SourceOrDest_PropertyChanged;
                }

                // 3.
                if (null != stream.Destination)
                {
                    m_destEventItem = stream.Destination;
                    stream.Destination.PropertyChanged += this.SourceOrDest_PropertyChanged;
                }

                // 4.
                stream.PropertyChanged += new PropertyChangedEventHandler(Stream_PropertyChanged);

                // 5.
                if (null != m_stream.Source)
                {
                    m_stream.Source.PropertyChanged += this.SourceOrDest_PropertyChanged;
                }
                if (null != m_stream.Destination)
                {
                    m_stream.Destination.PropertyChanged += this.SourceOrDest_PropertyChanged;
                }
            }
        }

        public static AbstractStream CreateOnCanvas(DrawingCanvas canvas,
            ChemProV.Core.AbstractStream stream)
        {
            AbstractStream streamControl = new AbstractStream(canvas, stream);
            canvas.AddNewChild(streamControl);
            return streamControl;
        }

        #region Event handlers

        private void DragIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // The icons should be hidden when we have a connection so ignore the event i
            
            if (null == m_canvas.CurrentState)
            {
                // If we've clicked a drag endpoint them select the stream
                m_canvas.SelectedElement = this;

                // Set state to moving endpoint state
                m_canvas.CurrentState = new UI.DrawingCanvas.States.MovingStreamEndpoint(
                    sender as DraggableStreamEndpoint, m_canvas, false);
            }
        }

        private void DestinationArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (null == m_canvas.CurrentState)
            {
                // If we've clicked a drag endpoint them select the stream
                m_canvas.SelectedElement = this;

                // Set state to moving endpoint state
                m_canvas.CurrentState = new UI.DrawingCanvas.States.MovingStreamEndpoint(
                    m_dstDragIcon, m_canvas, false);
            }
        }

        private bool m_ignoreStreamPropertyChanges = false;
        
        private void Stream_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If we're told to ignore these then just return
            if (m_ignoreStreamPropertyChanges)
            {
                return;
            }

            // Set the ignore flag to avoid recursion
            m_ignoreStreamPropertyChanges = true;
            
            if (e.PropertyName.Equals("Source"))
            {
                // Unsubscribe from the old source
                if (null != m_sourceEventItem)
                {
                    m_sourceEventItem.PropertyChanged -= this.SourceOrDest_PropertyChanged;
                }

                // Keep the reference to the new source
                m_sourceEventItem = m_stream.Source;

                // Subscribe to the new source
                if (null != m_stream.Source)
                {
                    m_stream.Source.PropertyChanged += this.SourceOrDest_PropertyChanged;

                    // Set the source location to that of the source process unit
                    m_stream.SourceLocation = m_stream.Source.Location;
                }

                // Update the stream's location
                UpdateStreamLocation();
            }
            else if (e.PropertyName.Equals("Destination"))
            {
                // Unsubscribe from the old destination
                if (null != m_destEventItem)
                {
                    m_destEventItem.PropertyChanged -= this.SourceOrDest_PropertyChanged;
                }

                // Keep the reference to the new destination
                m_destEventItem = m_stream.Destination;

                // Subscribe to the new destination
                if (null != m_stream.Destination)
                {
                    m_stream.Destination.PropertyChanged += this.SourceOrDest_PropertyChanged;

                    // Hide the destination drag icon. We need to use the arrow.
                    m_dstDragIcon.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    // Null destination means we need to show the drag icon and not the arrow
                    m_dstDragIcon.Visibility = System.Windows.Visibility.Visible;
                    m_arrow.Visibility = System.Windows.Visibility.Collapsed;
                }

                // Update the stream's location
                UpdateStreamLocation();
            }
            else if (e.PropertyName.Equals("SourceLocation"))
            {
                // Position the drag icon if the source is null
                if (null == m_stream.Source)
                {
                    m_sourceDragIcon.Location = new Point(
                        m_stream.SourceLocation.X, m_stream.SourceLocation.Y);
                }

                UpdateStreamLocation();
            }
            else if (e.PropertyName.Equals("DestinationLocation"))
            {
                // Position the drag icon if the destination is null
                if (null == m_stream.Destination)
                {
                    m_dstDragIcon.Location = new Point(
                        m_stream.DestinationLocation.X, m_stream.DestinationLocation.Y);
                }

                UpdateStreamLocation();
            }
            else if (e.PropertyName.Equals("Id"))
            {
                if (null != m_miniTable)
                {
                    m_miniTable.NameTextBlock.Text = "S" + m_stream.Id.ToString();
                }
            }

            // Reset the flag
            m_ignoreStreamPropertyChanges = false;
        }

        private void SourceOrDest_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateStreamLocation();
        }

        /// <summary>
        /// Invoked when the comment collection in m_pu changes. This method updates the set of 
        /// sticky note controls on the drawing canvas that are used to represent comments for 
        /// the process unit.
        /// </summary>
        private void Comments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Start by deleting any sticky note controls that represent comments that are no longer 
            // in the comment collection. While we do this, build a list of all comments that we 
            // have a sticky note control for (used in next step).
            List<StickyNote.StickyNote_UIIndependent> existing =
                new List<StickyNote.StickyNote_UIIndependent>();
            for (int i = 0; i < m_stickyNotes.Count; i++)
            {
                if (!m_stream.Comments.Contains(m_stickyNotes[i].StickyNote))
                {
                    // Remove the sticky note and its connecting line from the drawing canvas
                    m_canvas.RemoveChild(m_stickyNotes[i].LineToParent);
                    m_canvas.RemoveChild(m_stickyNotes[i]);

                    // Decrement the index because we've deleted an item
                    i--;
                }
                else
                {
                    existing.Add(m_stickyNotes[i].StickyNote);
                }
            }

            // Now add new sticky note controls for comments that don't have them
            for (int i = 0; i < m_stream.Comments.Count; i++)
            {
                if (!existing.Contains(m_stream.Comments[i]))
                {
                    StickyNote.StickyNoteControl.CreateOnCanvas(m_canvas, m_stream.Comments[i], this);
                }
            }
        }

        #endregion

        public DraggableStreamEndpoint SourceDragIcon
        {
            get
            {
                return m_sourceDragIcon;
            }
        }

        protected void CreatePropertiesTable(Core.StreamPropertiesTable table)
        {
            DrawingCanvas canvas = Core.App.Workspace.DrawingCanvas;
            m_table = new UI.StreamTableControl(table, canvas.GetWorkspace(), canvas);
            m_table.MinimizeButton.Click += new RoutedEventHandler(MinimizeTableButtonClick);
            UpdateStreamLocation();
        }

        private void MinimizeTableButtonClick(object sender, RoutedEventArgs e)
        {
            // If the mini-table control hasn't been created yet, then create it now
            if (null == m_miniTable)
            {
                m_miniTable = new UI.MiniStreamTable();
                m_miniTable.NameTextBlock.Text = "S" + m_stream.Id.ToString();
                m_miniTable.ExpandButton.Click += new RoutedEventHandler(ExpandTableButton_Click);
                m_canvas.Children.Add(m_miniTable);
            }

            // Hide the table and the line
            m_table.Visibility = System.Windows.Visibility.Collapsed;
            TableLine.Visibility = System.Windows.Visibility.Collapsed;

            // Make sure the mini-table is visible
            m_miniTable.Visibility = System.Windows.Visibility.Visible;

            // Position the mini table
            MathCore.Vector lineMidpoint = (m_stream.SourceLocation + m_stream.DestinationLocation) / 2.0;
            m_miniTable.SetValue(Canvas.LeftProperty, lineMidpoint.X);
            m_miniTable.SetValue(Canvas.TopProperty, lineMidpoint.Y);

            m_tableMinimized = true;
        }

        /// <summary>
        /// Expands from the mini-table back up to the full size table
        /// </summary>
        private void ExpandTableButton_Click(object sender, RoutedEventArgs e)
        {
            // First hide the mini table
            if (null != m_miniTable)
            {
                m_miniTable.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Show the regular table and the line
            m_table.Visibility = System.Windows.Visibility.Visible;
            TableLine.Visibility = System.Windows.Visibility.Visible;

            m_tableMinimized = false;
            UpdateStreamLocation();
        }

        [Obsolete("Will be removed or re-written in the near future")]
        public void HighlightFeedback(bool highlight)
        {
            //if (m_table != null)
            //{
            //    m_table.HighlightFeedback(highlight);
            //}
        }

        [Obsolete("Will be removed or re-written in the near future")]
        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            //if (m_table != null)
            //{
            //    m_table.SetFeedback(feedbackMessage, errorNumber);
            //}
        }

        [Obsolete("Will be removed or re-written in the near future")]
        public void RemoveFeedback()
        {
            //if (m_table != null)
            //{
            //    m_table.RemoveFeedback();
            //}
        }

        public ChemProV.Core.AbstractStream Stream
        {
            get
            {
                return m_stream;
            }
        }

        public Point StreamLineMidpoint
        {
            get
            {
                return new Point((Stem.X1 + Stem.X2) / 2.0, (Stem.Y1 + Stem.Y2) / 2.0);
            }
        }

        public void HideTable()
        {
            TableLine.Visibility = System.Windows.Visibility.Collapsed;
            if (null != m_table)
            {
                (m_table as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void ShowTable(bool autoPosition)
        {
            TableLine.Visibility = System.Windows.Visibility.Visible;
            if (null != m_table)
            {
                (m_table as UIElement).Visibility = System.Windows.Visibility.Visible;
            }

            // Position it if it was requested
            if (autoPosition)
            {
                MathCore.Vector mid = (m_stream.SourceLocation + m_stream.DestinationLocation) / 2.0;
                mid += new MathCore.Vector(0.0, 40.0);
                m_table.Data.Location = mid;

                // UI events will position the table control after we set m_table.Data.Location
                // Check to make sure the control isn't going off the edge
                double left = (double)m_table.GetValue(Canvas.LeftProperty);
                if (left < 0.0)
                {
                    m_table.SetValue(Canvas.LeftProperty, 0.0);
                }
                if ((double)m_table.GetValue(Canvas.TopProperty) < 0.0)
                {
                    m_table.SetValue(Canvas.TopProperty, 0.0);
                }
            }
        }

        public void HideAllComments()
        {
            foreach (StickyNote.StickyNoteControl snc in m_stickyNotes)
            {
                snc.Hide();
            }
        }

        public void ShowAllComments()
        {
            foreach (StickyNote.StickyNoteControl snc in m_stickyNotes)
            {
                snc.Show();
            }
        }

        /// <summary>
        /// Removes this control from the canvas. This includes removal of controls that are 
        /// "attached" to this control, such as the comment sticky note controls.
        /// </summary>
        public void RemoveSelfFromCanvas(UI.DrawingCanvas.DrawingCanvas canvas)
        {
            canvas.RemoveChild(this);
            canvas.RemoveChild(m_sourceDragIcon);
            canvas.RemoveChild(m_dstDragIcon);
            canvas.RemoveChild(m_table);
            if (null != m_miniTable)
            {
                canvas.RemoveChild(m_miniTable);
            }
            if (null != m_square)
            {
                canvas.RemoveChild(m_square);
            }
            if (null != m_arrow)
            {
                canvas.RemoveChild(m_arrow);
            }

            foreach (StickyNote.StickyNoteControl snc in m_stickyNotes)
            {
                canvas.RemoveChild(snc.LineToParent);
                canvas.RemoveChild(snc);
            }
        }

        public Point Location
        {
            get
            {
                return StreamLineMidpoint;
            }
            set
            {
            }
        }
    }
}