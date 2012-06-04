/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using ChemProV.Core;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.PFD.Streams
{
    public abstract partial class AbstractStream : UserControl, IStream, ICommentCollection
    {
        #region instance vars

        private IProcessUnit m_source;
        private IProcessUnit m_destination;
        protected IPropertiesWindow m_table = null;
        public event EventHandler LocationChanged = delegate { };
        public event EventHandler SelectionChanged = null;
        public bool m_isSelected = false;

        /// <summary>
        /// E.O.
        /// This is the draggable stream destination icon that the user can drag and drop over a 
        /// process unit to make a connection (if allowed).
        /// </summary>
        private DraggableStreamEndpoint m_dstDragIcon = null;

        /// <summary>
        /// E.O.
        /// This is the draggable stream source icon that the user can drag and drop over a 
        /// process unit to make a connection (if allowed).
        /// </summary>
        private DraggableStreamEndpoint m_srcDragIcon = null;

        /// <summary>
        /// The stream keeps a reference to its parent drawing canvas. Note that this will be 
        /// null if you use the default constructor.
        /// </summary>
        protected DrawingCanvas m_canvas = null;

        /// <summary>
        /// E.O.
        /// List of comments used for the implementation of Core.ICommentCollection
        /// </summary>
        private List<Core.IComment> m_comments = new List<Core.IComment>();

        private bool m_updatingLocation = false;

        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int s_streamIdCounter = 0;
        private string streamId;

        #endregion instance vars

        #region IStream Members

        /// <summary>
        /// Gets or sets the stream's unique ID number
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
                MathCore.Vector srcPos;
                if (null == m_source)
                {
                    srcPos = new MathCore.Vector(m_srcDragIcon.Location);
                }
                else
                {
                    srcPos = new MathCore.Vector(m_source.Location);
                }
                MathCore.Vector dstPos;
                if (null == m_destination)
                {
                    dstPos = new MathCore.Vector(m_dstDragIcon.Location);
                }
                else
                {
                    dstPos = new MathCore.Vector(m_destination.Location);
                }
                return dstPos - srcPos;
            }
        }

        /// <summary>
        /// Reference to the stream's source PFD element
        /// </summary>
        public IProcessUnit Source
        {
            get
            {
                return m_source;
            }
            set
            {
                GenericProcessUnit old = m_source as GenericProcessUnit;
                
                //remove the event listener from the old source
                if (m_source != null)
                {
                    m_source.LocationChanged -= new EventHandler(AttachedLocationChanged);
                }

                //set new source, attach new listener
                m_source = value;
                TemporaryProcessUnit tpu = value as TemporaryProcessUnit;
                if (null != tpu)
                {
                    // This really means that it's an unconnected endpoint
                    m_srcDragIcon.EndpointConnectionChanged(
                        DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected,
                        old, value as GenericProcessUnit);
                    m_source = null;
                    UpdateStreamLocation();
                }
                else if (m_source != null)
                {
                    // Update the source connection draggable icon
                    m_srcDragIcon.EndpointConnectionChanged(
                        DraggableStreamEndpoint.EndpointType.StreamSourceConnected,
                        old, value as GenericProcessUnit);
                    
                    m_source.LocationChanged += new EventHandler(AttachedLocationChanged);
                    UpdateStreamLocation();
                }
                else
                {
                    if (null != old)
                    {
                        m_srcDragIcon.EndpointConnectionChanged(
                            DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected,
                            old, null);

                        UpdateStreamLocation();
                    }

                    // Note that the "else" case here is that it was already null, in which case 
                    // we shouldn't need to change anything
                }
            }
        }

        /// <summary>
        /// Gets the draggable control that is on the canvas in order to allow the user to 
        /// connect the source of the stream to a process unit. If this stream has a null 
        /// source, then this object will just be an icon that can be dragged around and 
        /// dropped on a process unit to make a connection (if allowed). Otherwise (non-null 
        /// source process unit) this icon will be draggable such that the current connection 
        /// to the source process unit connection can be broken and the icon can be dropped on 
        /// another process unit to make a new connection or drop in blank space and leave the 
        /// connection severed.
        /// </summary>
        public DraggableStreamEndpoint SourceDragIcon
        {
            get
            {
                return m_srcDragIcon;
            }
        }

        /// <summary>
        /// Reference to the stream's destination PFD element
        /// </summary>
        public IProcessUnit Destination
        {
            get
            {
                return m_destination;
            }
            set
            {
                GenericProcessUnit old = m_destination as GenericProcessUnit;
                
                // Remove the event listener from the old destination
                if (m_destination != null)
                {
                    m_destination.LocationChanged -= new EventHandler(AttachedLocationChanged);
                }

                // Add the new destination and attach listener
                m_destination = value;
                if (m_destination != null)
                {
                    m_dstDragIcon.EndpointConnectionChanged(
                        DraggableStreamEndpoint.EndpointType.StreamDestinationConnected,
                        old, value as GenericProcessUnit);
                    
                    m_destination.LocationChanged += new EventHandler(AttachedLocationChanged);
                    UpdateStreamLocation();
                }
                else
                {
                    if (null != old)
                    {
                        m_dstDragIcon.EndpointConnectionChanged(
                            DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected,
                            old, value as GenericProcessUnit);
                        UpdateStreamLocation();
                    }

                    // Note that the "else" case here is that it was already null, in which case 
                    // we shouldn't need to change anything
                }
            }
        }

        public DraggableStreamEndpoint DestinationDragIcon
        {
            get
            {
                return m_dstDragIcon;
            }
        }

        public IPropertiesWindow Table
        {
            get
            {
                return m_table;
            }
            set
            {
                if (m_table != null)
                {
                    m_table.LocationChanged -= new EventHandler(AttachedLocationChanged);

                    // Remove the old one from the canvas
                    m_canvas.RemoveChild(m_table as UIElement);
                }

                m_table = value;
                if (m_table != null)
                {
                    m_table.LocationChanged += new EventHandler(AttachedLocationChanged);

                    // Add the new one to the canvas
                    m_canvas.AddNewChild(m_table as UIElement);
                }
            }
        }

        /// <summary>
        /// Gets the point on the border of the destination process unit where the stream line 
        /// connects to. This is only relevant if the stream has a non-null destination process 
        /// unit. If the destination is null then the point (0,0) is returned.
        /// </summary>
        public Point DestinationConnectionPoint
        {
            get
            {
                if (null == m_destination)
                {
                    return new Point(0.0, 0.0);
                }

                GenericProcessUnit dstUnit = m_destination as GenericProcessUnit;

                // Get the center point for the source (which could be a process unit if 
                // we're connected to one or a drag-handle if we're not)
                MathCore.Vector srcCenter;
                if (null == m_source)
                {
                    srcCenter = new MathCore.Vector(m_srcDragIcon.Location);
                }
                else
                {
                    GenericProcessUnit srcUnit = m_source as GenericProcessUnit;
                    srcCenter = new MathCore.Vector(srcUnit.Location);
                }

                // Make a rectangle for the destination process unit
                MathCore.Rectangle rDest = MathCore.Rectangle.CreateFromCanvasRect(
                    new Point((double)dstUnit.GetValue(Canvas.LeftProperty),
                        (double)dstUnit.GetValue(Canvas.TopProperty)),
                    dstUnit.Width, dstUnit.Height);

                // Build the line segment between the two process units
                MathCore.LineSegment ls = new MathCore.LineSegment(
                    srcCenter, new MathCore.Vector(m_destination.Location));

                MathCore.Vector[] pts = rDest.GetIntersections(ls);
                if (0 == pts.Length)
                {
                    return m_destination.Location;
                }

                return pts[0].ToPoint();
            }
        }

        public Point[] GetArrowVertices()
        {
            Point[] pts = new Point[3];

            // Start by getting the normalized connection line vector
            MathCore.Vector v = MathCore.Vector.Normalize(StreamVector);

            // Also get the connection point
            MathCore.Vector tip = new MathCore.Vector(DestinationConnectionPoint);

            // Build perpendicular MathCore.Vectors
            MathCore.Vector perp1 = MathCore.Vector.GetPerpendicular1(v) * 15.0;
            MathCore.Vector perp2 = MathCore.Vector.GetPerpendicular2(v) * 15.0;

            // Build the arrow
            pts[0] = tip.ToPoint();
            pts[1] = ((tip - (v * 15.0)) + perp1).ToPoint();
            pts[2] = ((tip - (v * 15.0)) + perp2).ToPoint();

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
                    new SolidColorBrush(Colors.Yellow) : StreamLineNotSelected;

                if (null != SelectionChanged)
                {
                    SelectionChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// E.O.
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid source. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more outgoing 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid source, false otherwise.</returns>
        public abstract bool IsValidSource(IProcessUnit unit);

        /// <summary>
        /// E.O.
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid destination. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more incoming 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid destination, false otherwise.</returns>
        public abstract bool IsValidDestination(IProcessUnit unit);

        #endregion IStream Members

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            //write element's unique id
            writer.WriteAttributeString("Id", Id);

            //also write the stream type
            writer.WriteAttributeString("StreamType", StreamFactory.StreamTypeFromStream(this).ToString());

            //write the source and destination process unit's id
            if (null == m_source)
            {
                writer.WriteStartElement("UnattachedSource");
                writer.WriteAttributeString("Location", m_srcDragIcon.Location.X.ToString() +
                    "," + m_srcDragIcon.Location.Y.ToString());
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString("Source", Source.Id);
            }
            if (null == m_destination)
            {
                writer.WriteStartElement("UnattachedDestination");
                writer.WriteAttributeString("Location", m_dstDragIcon.Location.X.ToString() +
                    "," + m_dstDragIcon.Location.Y.ToString());
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString("Destination", Destination.Id);
            }

            // E.O.
            // Write any and all comments
            if (m_comments.Count > 0)
            {
                writer.WriteStartElement("Comments");
                for (int i = 0; i < m_comments.Count; i++)
                {
                    StickyNote.StickyNote sn = m_comments[i] as StickyNote.StickyNote;
                    writer.WriteStartElement("Comment");
                    sn.WriteXml(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        #endregion IXmlSerializable Members

        /// <summary>
        /// To be called whenever we detect a change in location in either the source
        /// or destination PFD elements.  Will update the stream's location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttachedLocationChanged(object sender, EventArgs e)
        {
            UpdateStreamLocation();
        }

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
            
            // Start by positioning the stem line that connects source and destination
            Point a, b;
            if (null == m_source)
            {
                a = m_srcDragIcon.Location;
            }
            else
            {
                a = m_source.Location;
            }
            if (null == m_destination)
            {
                b = m_dstDragIcon.Location;
            }
            else
            {
                b = m_destination.Location;
            }
            Stem.X1 = a.X;
            Stem.Y1 = a.Y;
            Stem.X2 = b.X;
            Stem.Y2 = b.Y;

            // Now do the table line
            if (null != m_table)
            {
                TableLine.X1 = (Stem.X1 + Stem.X2) / 2.0;
                TableLine.Y1 = (Stem.Y1 + Stem.Y2) / 2.0;
                TableLine.X2 = m_table.Location.X;
                TableLine.Y2 = m_table.Location.Y;
            }

            // Let any interested parties know that we've updated our location
            LocationChanged(this, new EventArgs());

            m_updatingLocation = false;
        }

        /// <summary>
        /// Private constructor. This is used only to get designed view to work. It MUST NOT BE 
        /// MADE PUBLIC. For correct initialization the stream needs a DrawingCanvas reference.
        /// </summary>
        private AbstractStream()
            : this(null)
        {

        }

        /// <summary>
        /// Make all classes that inherit from AbstractStream call this base constructor. In your inherting 
        /// class you should still have a default constructor that will be used to create a "factory instance" 
        /// that can give information about the type of stream (methods like IsAvailableWithDifficulty). So 
        /// you should have a constructor with no paramaters and a constructor with a single DrawingCanvas 
        /// paramater and they should both call this as the base constructor (use a null DrawingCanvas 
        /// reference in the case of the default constructor).
        /// </summary>
        public AbstractStream(DrawingCanvas canvas)
        {
            m_canvas = canvas;
            
            Brush transperent = new SolidColorBrush(Colors.Transparent);
            InitializeComponent();

            //set the stream's id number
            s_streamIdCounter++;
            Id = "S_" + s_streamIdCounter;

            // Create the draggable source and destination icons
            m_srcDragIcon = new DraggableStreamEndpoint(
                DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected, this,
                canvas);
            m_dstDragIcon = new DraggableStreamEndpoint(
                DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected, this,
                canvas);
            // Set their sizes
            m_srcDragIcon.Width = m_srcDragIcon.Height = 40.0;
            m_dstDragIcon.Width = m_dstDragIcon.Height = 40.0;
            // Set positions
            m_srcDragIcon.Location = new Point();
            m_dstDragIcon.Location = new Point();
            // Hide them by default
            m_srcDragIcon.Visibility = System.Windows.Visibility.Collapsed;
            m_dstDragIcon.Visibility = System.Windows.Visibility.Collapsed;

            if (null != canvas)
            {
                // Add drag icons to the drawing canvas
                canvas.AddNewChild(m_srcDragIcon);
                canvas.AddNewChild(m_dstDragIcon);
                m_srcDragIcon.SetValue(Canvas.ZIndexProperty, 2);
                m_dstDragIcon.SetValue(Canvas.ZIndexProperty, 2);

                // Make sure the stem line has a low z-index
                Stem.SetValue(Canvas.ZIndexProperty, -1);

                UpdateStreamLocation();

                // Create the table (abstract method so inheriting class will do it)
                CreatePropertiesTable();

                // Position it
                m_table.Location = CalculateTablePositon(m_srcDragIcon.Location, m_dstDragIcon.Location);

                // Setup events for the table
                m_table.LocationChanged += delegate(object sender, EventArgs args)
                {
                    UpdateStreamLocation();
                };

                // Add it to the canvas and set it up
                m_canvas.AddNewChild(m_table as UIElement);

                UserControl puAsUiElement = m_table as UserControl;

                //width and height needed to calculate position, for some reaons it did not like puAsUiElemnt.Width had to
                //go with ActualWidth and ActualHeight but everything else had to b e Width and Height.
                double width = puAsUiElement.ActualWidth;
                double height = puAsUiElement.ActualHeight;

                //This sets the tables index to the greatest so it will be above everything
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 3);

                m_table.TableDataChanged -= new TableDataEventHandler((canvas as DrawingCanvas).TableDataChanged);
                m_table.TableDataChanged += new TableDataEventHandler((canvas as DrawingCanvas).TableDataChanged);

                m_table.TableDataChanging += new EventHandler((canvas as DrawingCanvas).TableDataChanging);

                puAsUiElement.MouseLeftButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonDownHandler);
                puAsUiElement.MouseLeftButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonUpHandler);
            }
        }

        protected abstract void CreatePropertiesTable();

        /// <summary>
        /// Calculates where the table should be placed given the Source and Destination as points
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Destination"></param>
        /// <returns>Top Left Point of where table should be placed</returns>
        public Point CalculateTablePositon(Point Source, Point Destination)
        {
            Point distance = new Point((Source.X - Destination.X), (Source.Y - Destination.Y));
            Point TableLocation = new Point();
            if (distance.X > 0 && distance.Y > 0)
            {
                distance.X = Math.Abs(distance.X) / 2;
                distance.Y = Math.Abs(distance.Y) / 2;
                TableLocation.X = Source.X - distance.X;
                TableLocation.Y = Source.Y - distance.Y;
            }
            else if (distance.X > 0 && distance.Y < 0)
            {
                distance.X = Math.Abs(distance.X) / 2;
                distance.Y = Math.Abs(distance.Y) / 2;
                TableLocation.X = Source.X - distance.X;
                TableLocation.Y = Source.Y + distance.Y;
            }
            else if (distance.X < 0 && distance.Y > 0)
            {
                distance.X = Math.Abs(distance.X) / 2;
                distance.Y = Math.Abs(distance.Y) / 2;
                TableLocation.X = Source.X + distance.X;
                TableLocation.Y = Source.Y - distance.Y;
            }
            else if (distance.X < 0 && distance.Y < 0)
            {
                distance.X = Math.Abs(distance.X) / 2;
                distance.Y = Math.Abs(distance.Y) / 2;
                TableLocation.X = Source.X + distance.X;
                TableLocation.Y = Source.Y + distance.Y;
            }
            else if (distance.X == 0 && distance.Y == 0)
            {
                TableLocation.X = Source.X;
                TableLocation.Y = Source.Y;
            }
            TableLocation.Y = TableLocation.Y + 10;
            return (TableLocation);
        }

        public void HighlightFeedback(bool highlight)
        {
            if (m_table != null)
            {
                m_table.HighlightFeedback(highlight);
            }
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            if (m_table != null)
            {
                m_table.SetFeedback(feedbackMessage, errorNumber);
            }
        }

        public void RemoveFeedback()
        {
            if (m_table != null)
            {
                m_table.RemoveFeedback();
            }
        }

        public virtual Brush StreamLineNotSelected
        {
            get
            {
                // Default to black
                return new SolidColorBrush(Colors.Black);
            }
        }

        public Point StreamLineMidpoint
        {
            get
            {
                return new Point((Stem.X1 + Stem.X2) / 2.0, (Stem.Y1 + Stem.Y2) / 2.0);
            }
        }

        #region ICommentCollection Members

        public bool AddComment(Core.IComment comment)
        {
            // Future versions might have some sort of permissions check here, but for 
            // now we just add it
            m_comments.Add(comment);

            return true;
        }

        public int CommentCount
        {
            get { return m_comments.Count; }
        }

        public Core.IComment GetCommentAt(int index)
        {
            if (index < 0 || index >= m_comments.Count)
            {
                return null;
            }

            return m_comments[index];
        }

        public bool InsertCommentAt(Core.IComment comment, int insertionIndex)
        {
            if (insertionIndex < 0 || insertionIndex > m_comments.Count)
            {
                return false;
            }

            // If index == count then we add
            if (insertionIndex == m_comments.Count)
            {
                return AddComment(comment);
            }

            m_comments.Insert(insertionIndex, comment);
            return true;
        }

        public bool RemoveCommentAt(int index)
        {
            if (index < 0 || index >= m_comments.Count)
            {
                // Return false if the index is invalid
                return false;
            }

            // Future versions might have some sort of permissions check here, but for 
            // now we just remove it
            m_comments.RemoveAt(index);

            return true;
        }

        public bool ReplaceCommentAt(int index, Core.IComment newComment)
        {
            if (index < 0 || index >= m_comments.Count)
            {
                // Return false if the index is invalid
                return false;
            }

            // Future versions might have some sort of permissions check here, but for 
            // now we just replace it
            m_comments[index] = newComment;
            return true;
        }

        #endregion
    }
}