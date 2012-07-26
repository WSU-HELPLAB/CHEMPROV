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
using ChemProV.UI;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.Logic;

namespace ChemProV.PFD.Streams
{
    /// <summary>
    /// Control that serves as a UI element for and AbstractStream. Manages its own collection of child 
    /// controls on the DrawingCanvas including comment sticky notes.
    /// </summary>
    public partial class StreamControl : UserControl, IPfdElement, UI.ICommentControlManager, ICanvasElement
    {
        #region instance vars

        public event EventHandler SelectionChanged = null;

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
        private AbstractProcessUnit m_destEventItem = null;

        /// <summary>
        /// This is the draggable stream destination icon that the user can drag and drop over a 
        /// process unit to make a connection (if allowed). It is created in the constructor and 
        /// stays on the drawing canvas for as long as this stream control does, but it is hidden 
        /// when the stream has a non-null destination.
        /// </summary>
        private DraggableStreamEndpoint m_dstDragIcon = null;

        /// <summary>
        /// Set to true after a call to "RemoveSelfFromCanvas". After such a call this stream 
        /// control is considered to be disposed and should not be used. This value is checked in 
        /// various places to ensure this.
        /// </summary>
        private bool m_hasBeenRemoved = false;

        public bool m_isSelected = false;

        /// <summary>
        /// Stores the mid-point value that was computed in the most recent call to 
        /// ComputeLineSegments
        /// </summary>
        private MathCore.Vector m_lastMidPoint = new MathCore.Vector(
            double.NegativeInfinity, double.NegativeInfinity);

        /// <summary>
        /// The collection of lines used to draw the stream. This control takes care of these lines 
        /// on the DrawingCanvas and must add/remove as this collection is altered.
        /// </summary>
        public List<Line> m_lines = new List<Line>();

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
        private AbstractProcessUnit m_sourceEventItem = null;

        private Shape m_square = null;

        /// <summary>
        /// The list of sticky note controls that have been created to represent comments for this 
        /// stream. Remember that the actual comments for the stream are stored in the 
        /// stream data structure (m_stream) and this list updates as the list of comments in that 
        /// object change.
        /// We store this because the stream is responsible for creating and removing its sticky 
        /// note controls on the drawing canvas.
        /// </summary>
        private List<StickyNoteControl> m_stickyNotes = new List<StickyNoteControl>();

        private AbstractStream m_stream;

        /// <summary>
        /// Brush used for the stream line when it is not selected. Built in the constructor and 
        /// will depend on the type of stream.
        /// </summary>
        private Brush m_streamLineNotSelected;

        protected UI.StreamTableControl m_table = null;

        private bool m_tableMinimized = false;

        private bool m_updatingLocation = false;

        #endregion instance vars

        private static readonly Brush s_streamLineSelected = new SolidColorBrush(Colors.Yellow);

        /// <summary>
        /// Gets or sets the stream's unique ID number
        /// TODO: Remove
        /// </summary>
        public String Id
        {
            get 
            {
                return null;
            }
            set { }
        }

        public DraggableStreamEndpoint DestinationDragIcon
        {
            get
            {
                return m_dstDragIcon;
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

        /// <summary>
        /// Computes line segments for the stream lines that should be rendered. The first point (A) 
        /// in the first segment will be the source location and the last point (B) in the last 
        /// segment will be the destination location. The number of lines in the array can be any 
        /// value greater than 1.
        /// </summary>
        private MathCore.LineSegment[] ComputeLineSegments(out MathCore.Vector midpoint, 
            out MathCore.Vector sourceIconLocation)
        {
            MathCore.Vector s = (null == m_stream.Source) ? 
                m_stream.SourceLocation : m_stream.Source.Location;
            MathCore.Vector d = (null == m_stream.Destination) ?
                m_stream.DestinationLocation : m_stream.Destination.Location;

            // Set the default source icon location. Will get changed below if need be
            sourceIconLocation = s;
            
            // Start with the simplest case of 1 straight line
            MathCore.LineSegment simplest = new MathCore.LineSegment(
                m_stream.SourceLocation, m_stream.DestinationLocation);

            // If it doesn't intersect any other process units then we'll use it
            if (!m_canvas.IntersectsAnyPU(simplest, m_stream.Source, m_stream.Destination))
            {
                midpoint = (s + d) / 2.0;
                m_lastMidPoint = midpoint;
                if (null != m_stream.Source)
                {
                    sourceIconLocation = s + (MathCore.Vector.Normalize(d - s) * 30.0);
                }
                return new MathCore.LineSegment[] { simplest };
            }

            // If that doesn't work, try two lines that make a 90 degree angle
            MathCore.Vector corner = new MathCore.Vector(s.X, d.Y);
            MathCore.LineSegment a = new MathCore.LineSegment(s, corner);
            MathCore.LineSegment b = new MathCore.LineSegment(corner, d);
            if (!m_canvas.IntersectsAnyPU(a, m_stream.Source, m_stream.Destination) &&
                !m_canvas.IntersectsAnyPU(b, m_stream.Source, m_stream.Destination))
            {
                // These lines will work
                midpoint = corner;
                m_lastMidPoint = midpoint;
                if (null != m_stream.Source)
                {
                    sourceIconLocation = s + (MathCore.Vector.Normalize(a.Direction) * 30.0);
                }
                return new MathCore.LineSegment[] { a, b };
            }
            // Try the other variant
            corner = new MathCore.Vector(d.X, s.Y);
            a = new MathCore.LineSegment(s, corner);
            b = new MathCore.LineSegment(corner, d);
            if (!m_canvas.IntersectsAnyPU(a, m_stream.Source, m_stream.Destination) &&
                !m_canvas.IntersectsAnyPU(b, m_stream.Source, m_stream.Destination))
            {
                // These lines will work
                midpoint = corner;
                m_lastMidPoint = midpoint;
                if (null != m_stream.Source)
                {
                    sourceIconLocation = s + (MathCore.Vector.Normalize(a.Direction) * 30.0);
                }
                return new MathCore.LineSegment[] { a, b };
            }

            // If we still don't have it then try a 3-line box
            double dx = Math.Abs(s.X - d.X);
            double dy = Math.Abs(s.Y - d.Y);
            if (dy > dx)
            {
                double xPos = Math.Min(s.X, d.X - 35.0);
                corner = new MathCore.Vector(xPos, s.Y);
                MathCore.Vector corner2 = new MathCore.Vector(xPos, d.Y);
                a = new MathCore.LineSegment(s, corner);
                b = new MathCore.LineSegment(corner, corner2);
                MathCore.LineSegment c = new MathCore.LineSegment(corner2, d);

                if (!m_canvas.IntersectsAnyPU(a, m_stream.Source, m_stream.Destination) &&
                    !m_canvas.IntersectsAnyPU(b, m_stream.Source, m_stream.Destination) &&
                    !m_canvas.IntersectsAnyPU(c, m_stream.Source, m_stream.Destination))
                {
                    // These lines will work
                    midpoint = (corner + corner2) / 2.0;
                    m_lastMidPoint = midpoint;
                    if (null != m_stream.Source)
                    {
                        sourceIconLocation = s + (MathCore.Vector.Normalize(a.Direction) * 30.0);
                    }
                    return new MathCore.LineSegment[] { a, b, c };
                }
            }
            else
            {
                double yPos = Math.Min(s.Y, d.Y) - 35.0;
                corner = new MathCore.Vector(s.X, yPos);
                MathCore.Vector corner2 = new MathCore.Vector(d.X, yPos);
                a = new MathCore.LineSegment(s, corner);
                b = new MathCore.LineSegment(corner, corner2);
                MathCore.LineSegment c = new MathCore.LineSegment(corner2, d);

                if (!m_canvas.IntersectsAnyPU(a, m_stream.Source, m_stream.Destination) &&
                    !m_canvas.IntersectsAnyPU(b, m_stream.Source, m_stream.Destination) &&
                    !m_canvas.IntersectsAnyPU(c, m_stream.Source, m_stream.Destination))
                {
                    // These lines will work
                    midpoint = (corner + corner2) / 2.0;
                    m_lastMidPoint = midpoint;
                    if (null != m_stream.Source)
                    {
                        sourceIconLocation = s + (MathCore.Vector.Normalize(a.Direction) * 30.0);
                    }
                    return new MathCore.LineSegment[] { a, b, c };
                }
            }

            // Out of options if we come here, so use the straight line
            midpoint = (s + d) / 2.0;
            m_lastMidPoint = midpoint;
            if (null != m_stream.Source)
            {
                sourceIconLocation = s + (MathCore.Vector.Normalize(d - s) * 30.0);
            }
            return new MathCore.LineSegment[] { simplest };
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

                // Set the color of the lines based on selection state
                foreach (Line line in m_lines)
                {
                    line.Stroke = m_isSelected ?
                        s_streamLineSelected : m_streamLineNotSelected;
                }

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
            return m_stream.IsValidSource((unit as ProcessUnitControl).ProcessUnit);
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
            return m_stream.IsValidDestination((unit as ProcessUnitControl).ProcessUnit);
        }

        private static void SetLineLocation(Line line, MathCore.Vector a, MathCore.Vector b)
        {
            line.X1 = a.X;
            line.Y1 = a.Y;
            line.X2 = b.X;
            line.Y2 = b.Y;
        }

        private void SourceSquare_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (null == m_canvas.CurrentState)
            {
                // If we've clicked a drag endpoint them select the stream
                m_canvas.SelectedElement = this;

                // Set state to moving endpoint state
                m_canvas.CurrentState = new UI.DrawingCanvasStates.MovingStreamEndpoint(
                    m_sourceDragIcon, m_canvas, false);
            }
        }

        /// <summary>
        /// Private constructor. This is used only to get designed view to work. IT MUST NOT BE 
        /// MADE PUBLIC. For correct initialization the stream needs a DrawingCanvas reference.
        /// </summary>
        private StreamControl()
            : this(null, null)
        {

        }

        private StreamControl(DrawingCanvas canvas, AbstractStream stream)
        {
            m_canvas = canvas;
            m_stream = stream;
            
            InitializeComponent();

            if (null != canvas && null != stream)
            {
                // Create the brush for the stream line
                if (stream is HeatStream)
                {
                    m_streamLineNotSelected = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    m_streamLineNotSelected = new SolidColorBrush(Colors.Black);
                }
                
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
                // Set the fill
                m_arrow.Fill = m_streamLineNotSelected;
                // Add it to the canvas
                m_canvas.AddNewChild(m_arrow);
                // Set the Z-index
                m_arrow.SetValue(Canvas.ZIndexProperty, 2);
                // Add 3 points/vertices
                m_arrow.Points.Add(new Point());
                m_arrow.Points.Add(new Point());
                m_arrow.Points.Add(new Point());
                // Hide it if there's no destination
                if (null == m_stream.Destination)
                {
                    m_arrow.Visibility = Visibility.Collapsed;
                }
                // Setup mouse events
                if (!(m_stream.Destination is HeatExchangerWithUtility))
                {
                    m_arrow.MouseLeftButtonDown += new MouseButtonEventHandler(DestinationArrow_MouseLeftButtonDown);
                }

                UpdateStreamLocation();

                // Create the table
                CreatePropertiesTable(m_stream.PropertiesTable);

                // Watch for when the table location changes
                m_stream.PropertiesTable.PropertyChanged += new PropertyChangedEventHandler(PropertiesTable_PropertyChanged);

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
            }
        }

        private void PropertiesTable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Location"))
            {
                UpdateStreamLocation();
            }
        }

        public static StreamControl CreateOnCanvas(DrawingCanvas canvas, AbstractStream stream)
        {
            StreamControl streamControl = new StreamControl(canvas, stream);
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
                m_canvas.CurrentState = new UI.DrawingCanvasStates.MovingStreamEndpoint(
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
                m_canvas.CurrentState = new UI.DrawingCanvasStates.MovingStreamEndpoint(
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
            if (m_hasBeenRemoved)
            {
                throw new InvalidOperationException();
            }
            
            UpdateStreamLocation();
        }

        /// <summary>
        /// Invoked when the comment collection in m_stream changes. This method updates the set of 
        /// sticky note controls on the drawing canvas that are used to represent comments for 
        /// the process unit.
        /// </summary>
        private void Comments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Start by deleting any sticky note controls that represent comments that are no longer 
            // in the comment collection. While we do this, build a list of all comments that we 
            // have a sticky note control for (used in next step).
            List<StickyNote> existing = new List<StickyNote>();
            for (int i = 0; i < m_stickyNotes.Count; i++)
            {
                if (!m_stream.Comments.Contains(m_stickyNotes[i].StickyNote))
                {
                    // Remove the sticky note and its connecting line from the drawing canvas
                    m_canvas.RemoveChild(m_stickyNotes[i].LineToParent);
                    m_canvas.RemoveChild(m_stickyNotes[i]);

                    // Remove it from our collection as well and then back up the index
                    m_stickyNotes.RemoveAt(i);
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
                    m_stickyNotes.Add(StickyNoteControl.CreateOnCanvas(
                        m_canvas, m_stream.Comments[i], this));
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

        protected void CreatePropertiesTable(StreamPropertiesTable table)
        {
            DrawingCanvas canvas = Core.App.Workspace.DrawingCanvas;
            m_table = new UI.StreamTableControl(table, this, canvas.GetWorkspace(), canvas);
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

            // Hide the full size table and the line
            m_table.Visibility = System.Windows.Visibility.Collapsed;
            TableLine.Visibility = System.Windows.Visibility.Collapsed;

            // Make sure the mini-table is visible
            m_miniTable.Visibility = System.Windows.Visibility.Visible;

            m_tableMinimized = true;
            UpdateStreamLocation();            
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

        public double GetShortestDistanceFromLines(MathCore.Vector location)
        {
            if (0 == m_lines.Count)
            {
                // If there are no lines we'll say it's very far away
                return double.MaxValue;
            }

            double d = double.MaxValue;
            foreach (Line line in m_lines)
            {
                MathCore.LineSegment ls = new MathCore.LineSegment(
                    line.X1, line.Y1, line.X2, line.Y2);
                double tempD = ls.GetDistance(location);
                if (tempD < d)
                {
                    d = tempD;
                }
            }

            return d;
        }
        
        public double GetShortestDistanceFromLines(Point location)
        {
            return GetShortestDistanceFromLines(new MathCore.Vector(location.X, location.Y));
        }

        public static string GetIconSource(Type streamType)
        {
            // Determine the icon from the process unit type    
            if (typeof(HeatStream) == streamType)
            {
                return "/UI/Icons/pu_heat_stream.png";
            }
            return "/UI/Icons/pu_stream.png";
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

        public void SetLineBrush(Brush brush)
        {
            foreach (Line line in m_lines)
            {
                line.Stroke = brush;
                line.Fill = brush;
            }
        }

        public AbstractStream Stream
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
                if (double.NegativeInfinity.Equals(m_lastMidPoint.X) ||
                    double.NegativeInfinity.Equals(m_lastMidPoint.Y))
                {
                    return new Point(
                        (m_stream.SourceLocation.X + m_stream.DestinationLocation.X) / 2.0,
                        (m_stream.SourceLocation.Y + m_stream.DestinationLocation.Y) / 2.0);
                }
                return new Point(m_lastMidPoint.X, m_lastMidPoint.Y);
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
            foreach (StickyNoteControl snc in m_stickyNotes)
            {
                snc.Hide();
            }
        }

        public void ShowAllComments()
        {
            foreach (StickyNoteControl snc in m_stickyNotes)
            {
                snc.Show();
            }
        }

        /// <summary>
        /// Removes this control from the canvas. This includes removal of controls that are 
        /// "attached" to this control, such as the comment sticky note controls.
        /// This is considered disposal of the control and it should not be used anymore 
        /// after calling this method.
        /// </summary>
        public void RemoveSelfFromCanvas(UI.DrawingCanvas canvas)
        {
            // Unsubscribe from events (important)
            m_stream.PropertyChanged -= this.Stream_PropertyChanged;
            m_stream.Comments.CollectionChanged -= this.Comments_CollectionChanged;
            if (null != m_sourceEventItem)
            {
                m_sourceEventItem.PropertyChanged -= this.SourceOrDest_PropertyChanged;
                m_sourceEventItem = null;
            }
            if (null != m_destEventItem)
            {
                m_destEventItem.PropertyChanged -= this.SourceOrDest_PropertyChanged;
                m_destEventItem = null;
            }
            if (null != m_table)
            {
                m_stream.PropertiesTable.PropertyChanged -= this.PropertiesTable_PropertyChanged;
            }
            
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

            // Remove all the lines
            foreach (Line line in m_lines)
            {
                canvas.RemoveChild(line);
            }
            m_lines.Clear();

            foreach (StickyNoteControl snc in m_stickyNotes)
            {
                canvas.RemoveChild(snc.LineToParent);
                canvas.RemoveChild(snc);
            }
            m_stickyNotes.Clear();

            // Set the value to indiate that we've removed
            m_hasBeenRemoved = true;
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

        public UI.StreamTableControl Table
        {
            get
            {
                return m_table;
            }
        }

        /// <summary>
        /// Recomputes location of all relevant controls within the stream (endpoint icons, 
        /// stream lines, etc.)
        /// </summary>
        public virtual void UpdateStreamLocation()
        {
            // Error check: we shouldn't be here if the control has been removed from the canvas
            if (m_hasBeenRemoved)
            {
                throw new InvalidOperationException(
                    "Stream control that was removed from the PFD is being notified to update its location.");
            }
            
            if (m_updatingLocation)
            {
                return;
            }
            m_updatingLocation = true;

            MathCore.Vector sPt = m_stream.SourceLocation;
            MathCore.Vector dPt = m_stream.DestinationLocation;
            MathCore.Vector lineMidpoint = (sPt + dPt) / 2.0;

            // Compute the line segments for our stream lines
            MathCore.Vector mid, sIconPt;
            MathCore.LineSegment[] lines = ComputeLineSegments(out mid, out sIconPt);
            // Get the list of UI lines to the right size
            while (m_lines.Count > lines.Length)
            {
                int index = m_lines.Count - 1;
                m_canvas.RemoveChild(m_lines[index]);
                m_lines.RemoveAt(index);
            }
            while (m_lines.Count < lines.Length)
            {
                Line line = new Line();
                m_lines.Add(line);
                m_canvas.AddNewChild(line);

                // Make sure the line has a low z-index
                line.SetValue(Canvas.ZIndexProperty, -1);

                // Remember to set the line color
                Brush b = m_isSelected ? s_streamLineSelected : m_streamLineNotSelected;
                line.Fill = b;
                line.Stroke = b;
            }
            // Set the positions
            for (int i = 0; i < lines.Length; i++)
            {
                SetLineLocation(m_lines[i], lines[i].A, lines[i].B);
            }

            // Now do the table line if necessary
            if (null != m_table && !m_tableMinimized)
            {
                TableLine.X1 = mid.X;
                TableLine.Y1 = mid.Y;
                TableLine.X2 = m_table.Location.X;
                TableLine.Y2 = m_table.Location.Y;
            }
            
            // Check if we're minimized then we need to position the mini table
            if (m_tableMinimized)
            {
                m_miniTable.SetValue(Canvas.LeftProperty, mid.X);
                m_miniTable.SetValue(Canvas.TopProperty, mid.Y);
            }

            // Take care of the source drag icon
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
                m_square.SetValue(Canvas.LeftProperty, sIconPt.X - 5.0);
                m_square.SetValue(Canvas.TopProperty, sIconPt.Y - 5.0);
            }
            else
            {
                m_sourceDragIcon.Location = new Point(
                    m_stream.SourceLocation.X, m_stream.SourceLocation.Y);
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

            // If we have a non-null destination, update the arrow
            if (null != m_stream.Destination)
            {
                // Where the last line segment intersects the destination process unit is where 
                // the tip of the arrow is
                Point pt = new Point(
                    m_stream.Destination.Location.X - 20.0,
                    m_stream.Destination.Location.Y - 20.0);
                MathCore.Rectangle destRect = MathCore.Rectangle.CreateFromCanvasRect(pt, 40.0, 40.0);
                MathCore.Vector[] isects = destRect.GetIntersections(lines[lines.Length - 1]);
                MathCore.LineSegment lastLine = lines[lines.Length - 1];

                if (0 == isects.Length)
                {
                    // No clue what to do here
                }

                double minDist = double.MaxValue;
                foreach (MathCore.Vector isectPt in isects)
                {
                    double tempDist = (isectPt - lastLine.A).Length;
                    if (tempDist < minDist)
                    {
                        minDist = tempDist;
                    }
                }

                MathCore.Vector dirNorm = MathCore.Vector.Normalize(lastLine.Direction);
                MathCore.Vector tip = lastLine.A + dirNorm * minDist;
                MathCore.Vector perp1 = MathCore.Vector.Normalize(
                    MathCore.Vector.GetPerpendicular1(lastLine.Direction));
                MathCore.Vector perp2 = MathCore.Vector.Normalize(
                    MathCore.Vector.GetPerpendicular2(lastLine.Direction));
                MathCore.Vector[] pts = new MathCore.Vector[]{
                    tip,
                    tip - (dirNorm * 10.0) + (perp1 * 10.0),
                    tip - (dirNorm*10.0) + (perp2 * 10.0) };

                // Set the vertices
                for (int i = 0; i < 3; i++)
                {
                    m_arrow.Points[i] = new Point(pts[i].X, pts[i].Y);
                }

                m_arrow.Visibility = System.Windows.Visibility.Visible;
            }

            // Lastly, tell the comment sticky notes to update
            foreach (StickyNoteControl sn in m_stickyNotes)
            {
                sn.UpdateLineToParent();
            }

            m_updatingLocation = false;
        }
    }
}