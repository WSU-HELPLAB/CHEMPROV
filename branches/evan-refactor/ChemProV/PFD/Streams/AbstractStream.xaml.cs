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
using System.Windows.Media;
using System.Windows.Shapes;

using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.PFD.Streams
{
    public abstract partial class AbstractStream : UserControl, IStream
    {
        #region instance vars

        private IProcessUnit m_source;
        private IProcessUnit m_destination;
        protected IPropertiesWindow m_table;
        private StreamDestinationIcon streamDestination;
        public event EventHandler LocationChanged = delegate { };
        public event EventHandler SelectionChanged = null;
        public event MouseButtonEventHandler Arrow_MouseButtonLeftDown = delegate { };
        public event MouseButtonEventHandler Tail_MouseButtonLeftDown = delegate { };
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

        public StreamDestinationIcon StreamDestination
        {
            get
            {
                return streamDestination;
            }
            set
            {
                streamDestination = value;
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
                IProcessUnit old = m_source;
                
                //remove the event listener from the old source
                if (m_source != null)
                {
                    m_source.LocationChanged -= new EventHandler(AttachedLocationChanged);
                }

                //set new source, attach new listener
                m_source = value;
                if (m_source != null)
                {
                    // Update the source connection draggable icon. It will be positioned in 
                    // UpdateStreamLocation
                    m_srcDragIcon.SetType(DraggableStreamEndpoint.EndpointType.StreamSourceConnected);
                    
                    m_source.LocationChanged += new EventHandler(AttachedLocationChanged);
                    UpdateStreamLocation();
                }
                else
                {
                    if (null != old)
                    {
                        m_srcDragIcon.SetType(DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected);

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
                IProcessUnit old = m_destination;
                
                // Remove the event listener from the old destination
                if (m_destination != null)
                {
                    m_destination.LocationChanged -= new EventHandler(AttachedLocationChanged);
                }

                // Add the new destination and attach listener
                m_destination = value;
                if (m_destination != null)
                {
                    m_dstDragIcon.SetType(DraggableStreamEndpoint.EndpointType.StreamDestinationConnected);
                    
                    m_destination.LocationChanged += new EventHandler(AttachedLocationChanged);
                    UpdateStreamLocation();
                }
                else
                {
                    if (null != old)
                    {
                        m_dstDragIcon.SetType(DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected);
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
                }

                m_table = value;
                if (m_table != null)
                {
                    m_table.LocationChanged += new EventHandler(AttachedLocationChanged);
                }
            }
        }

        private bool destinationArrorVisbility;

        public bool DestinationArrorVisbility
        {
            get
            {
                return destinationArrorVisbility;
            }
            set
            {
                if (value == true)
                {
                    this.Arrow.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Arrow.Visibility = Visibility.Collapsed;
                }
                destinationArrorVisbility = value;
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
                    srcCenter = new MathCore.Vector(
                        (double)srcUnit.GetValue(Canvas.LeftProperty),
                        (double)srcUnit.GetValue(Canvas.TopProperty));
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
                this.Stem.Stroke = new SolidColorBrush(m_isSelected ? Colors.Yellow : Colors.Black);

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

        public void ReadXml(System.Xml.XmlReader reader)
        {
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            //write element's unique id
            writer.WriteAttributeString("Id", Id);

            //also write the stream type
            writer.WriteAttributeString("StreamType", StreamFactory.StreamTypeFromStream(this).ToString());

            //write the source and destination process unit's id
            writer.WriteElementString("Source", Source.Id);
            writer.WriteElementString("Destination", Destination.Id);
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
            ////   Brush black = new SolidColorBrush(Colors.Black);
            //RotateTransform rt = new RotateTransform();
            //Point temp = new Point();

            ////   ((AbstractStream)stream).HitArea.Fill = black;
            ////recast source and dest as UIElement.  *REALLY* should make this a
            ////stronger relationship
            //UIElement source = (UIElement)Source;
            //UIElement destination = (UIElement)Destination;

            //if (source != null && destination != null)
            //{
            //    /////////////////////////////////////////////////////////////////////////////////
            //    ///////build our reference line, relative to the center of our Source unit///////

            //    Line referenceLine = new Line();
            //    referenceLine.X1 = Convert.ToInt32(source.GetValue(Canvas.LeftProperty)) + (Convert.ToInt32(source.GetValue(Control.WidthProperty)) / 2);
            //    referenceLine.X2 = referenceLine.X1;
            //    referenceLine.Y1 = Convert.ToInt32(source.GetValue(Canvas.TopProperty)) + (Convert.ToInt32(source.GetValue(Control.HeightProperty)) / 2);
            //    referenceLine.Y2 = referenceLine.Y1 - (Convert.ToInt32(source.GetValue(Control.HeightProperty)));

            //    //stretch out the stream's stem from the beginning to the end point
            //    Stem.X1 = referenceLine.X1;
            //    Stem.Y1 = referenceLine.Y1;
            //    /////////////////////////////////////////////////////////////////////////////////

            //    /////////////////////////////////////////////////////////////////////////////////
            //    ////////////Find and set destination point//////////////////////////////////////

            //    if (destinationArrorVisbility == true)
            //    {
            //        temp = toEdgeOfObject((UserControl)source, (UserControl)destination, Arrow.Height / 2);
            //    }
            //    else
            //    {
            //        temp = toEdgeOfObject((UserControl)source, (UserControl)destination, 0);
            //    }
            //    Stem.X2 = temp.X;
            //    Stem.Y2 = temp.Y;
            //    Stem.SetValue(Canvas.ZIndexProperty, -1);
            //    //s  element.SetValue(Canvas.ZIndexProperty, -1);
            //    //Stem.X2 = (double)destination.GetValue(Canvas.LeftProperty);
            //    //Stem.Y2 = (double)destination.GetValue(Canvas.TopProperty);
            //    /////////////////////////////////////////////////////////////////////////////////

            //    /////////////////////////////////////////////////////////////////////////////////
            //    ////////place the arrow at the end of the stem///////////////////////////

            //    Arrow.SetValue(Canvas.LeftProperty, Stem.X2 - (Arrow.Width / 2));
            //    Arrow.SetValue(Canvas.TopProperty, Stem.Y2 - (Arrow.Height / 2));

            //    //build the hypotenuse, used to compute the angle of rotation
            //    Line hypotenuse = new Line();
            //    hypotenuse.X1 = referenceLine.X2;
            //    hypotenuse.X2 = Stem.X2;
            //    hypotenuse.Y1 = referenceLine.Y2;
            //    hypotenuse.Y2 = Stem.Y2;

            //    //compute the angle of rotation
            //    ArrowRotateTransform.Angle = RadiansToDegrees(ComputeAngleRadians(referenceLine, Stem, hypotenuse));
            //    /////////////////////////////////////////////////////////////////////////////////

            //    /////////////////////////////////////////////////////////////////////////////////
            //    ////////place the HitArea Box///////////////////////////////////////////////////

            //    //-90 because math was made for arrow pointing up but the box horizontal "pointing" to the right so -90 to make them the same
            //    rt.Angle = RadiansToDegrees(ComputeAngleRadians(referenceLine, Stem, hypotenuse)) - 90;

            //    //set the location of hte HitArea box -10 on Y value so box is centered
            //    HitArea.SetValue(Canvas.LeftProperty, referenceLine.X1 + (double)LayoutRoot.GetValue(Canvas.LeftProperty));
            //    HitArea.SetValue(Canvas.TopProperty, referenceLine.Y1 + (double)LayoutRoot.GetValue(Canvas.TopProperty) - 10);

            //    //Calulate distance for length
            //    HitArea.Width = Pyth(referenceLine.X1, Stem.X2, referenceLine.Y1, Stem.Y2);

            //    //20 is height of the box
            //    HitArea.Height = 20;

            //    //CenterY = 10 so that it rotates in the middle of the box and not at an edge
            //    rt.CenterY = 10;

            //    //apply tranformation
            //    HitArea.RenderTransform = rt;
            //    /////////////////////////////////////////////////////////////////////////////////

            //    /////////////////////////////////////////////////////////////////////////////////
            //    //Find and draw the line from the middle of the line to its table////////////////

            //    if (table != null)
            //    {
            //        TableLine.X1 = (Stem.X2 - Stem.X1) / 2;
            //        TableLine.Y1 = (Stem.Y2 - Stem.Y1) / 2;
            //        if (TableLine.X1 >= 0 && TableLine.Y1 >= 0)
            //        {
            //            TableLine.X1 = Math.Abs(TableLine.X1);
            //            TableLine.Y1 = Math.Abs(TableLine.Y1);
            //            TableLine.X1 = Stem.X1 + TableLine.X1;
            //            TableLine.Y1 = Stem.Y1 + TableLine.Y1;
            //        }
            //        else if (TableLine.X1 >= 0 && TableLine.Y1 < 0)
            //        {
            //            TableLine.X1 = Math.Abs(TableLine.X1);
            //            TableLine.Y1 = Math.Abs(TableLine.Y1);
            //            TableLine.X1 = Stem.X1 + TableLine.X1;
            //            TableLine.Y1 = Stem.Y1 - TableLine.Y1;
            //        }
            //        else if (TableLine.X1 < 0 && TableLine.Y1 >= 0)
            //        {
            //            TableLine.X1 = Math.Abs(TableLine.X1);
            //            TableLine.Y1 = Math.Abs(TableLine.Y1);
            //            TableLine.X1 = Stem.X1 - TableLine.X1;
            //            TableLine.Y1 = Stem.Y1 + TableLine.Y1;
            //        }
            //        else if (TableLine.X1 < 0 && TableLine.Y1 < 0)
            //        {
            //            TableLine.X1 = Math.Abs(TableLine.X1);
            //            TableLine.Y1 = Math.Abs(TableLine.Y1);
            //            TableLine.X1 = Stem.X1 - TableLine.X1;
            //            TableLine.Y1 = Stem.Y1 - TableLine.Y1;
            //        }

            //        TableLine.X2 = (double)((UIElement)table).GetValue(Canvas.LeftProperty) + ((UserControl)table).ActualWidth / 2;
            //        TableLine.Y2 = (double)((UIElement)table).GetValue(Canvas.TopProperty) + ((UserControl)table).ActualHeight / 2;
            //    }
            //    /////////////////////////////////////////////////////////////////////////////////

            //    /////////////////////////////////////////////////////////////////////////////////
            //    ///////////////////////////Set Location of rectangle////////////////////////////////
            //    if (sourceRectangleVisbility == true)
            //    {
            //        temp = toEdgeOfObject((UserControl)destination, (UserControl)source, rectangle.Width / 2);
            //    }
            //    else
            //    {
            //        temp = toEdgeOfObject((UserControl)destination, (UserControl)source, 0);
            //    }
            //    rectangle.SetValue(Canvas.LeftProperty, temp.X - rectangle.Width / 2);
            //    rectangle.SetValue(Canvas.TopProperty, temp.Y - rectangle.Height / 2);
            //    /////////////////////////////////////////////////////////////////////////////////

                
            //}

            // E.O.
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

            // If we are connected to a source, then we need to position the draggable icon
            if (null != m_source)
            {
                MathCore.Vector pos = MathCore.Vector.Normalize(StreamVector) * 30.0;
                m_srcDragIcon.Location = ((new MathCore.Vector(m_source.Location)) + pos).ToPoint();
            }
            // Similar thing with destination
            if (null != m_destination)
            {
                MathCore.Vector pos = MathCore.Vector.Normalize(StreamVector) * -30.0;
                m_dstDragIcon.Location = ((new MathCore.Vector(m_destination.Location)) + pos).ToPoint();
            }

            // Let any interested parties know that we've updated our location
            LocationChanged(this, new EventArgs());
        }

        /// <summary>
        /// Calculates the Pythagorean Theorem and returns the answer
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>

        private double Pyth(double x1, double x2, double y1, double y2)
        {
            return (Math.Sqrt(Math.Pow(x2 - x1, 2) + (Math.Pow(y2 - y1, 2))));
        }

        /// <summary>
        /// Computes the degree opposite Line c in radians
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c">The hypotenuse</param>
        /// <returns>The angle opposite to line c in radians</returns>
        private double ComputeAngleRadians(Line a, Line b, Line c)
        {
            //compute the distance of each line
            double lengthA = ComputeDistance(a);
            double lengthB = ComputeDistance(b);
            double lengthC = ComputeDistance(c);

            //fancy math to determine the angle
            if (lengthA == 0 || lengthB == 0 || lengthC == 0)
                return (0);

            double cosine = (Math.Pow(lengthA, 2.0) + Math.Pow(lengthB, 2.0) - Math.Pow(lengthC, 2.0)) / (2.0 * lengthA * lengthB);
            double radians = Math.Acos(cosine);

            //the above calculation only goes from -PI to PI, we need a full 360 degrees
            //of rotation, so if the "a" line is behind the "c" line, then consider that
            //to be negative space
            if (c.X2 < a.X1)
            {
                radians = -radians;
            }
            return radians;
        }

        /// <summary>
        /// Converts radians into degrees, which is used by SilverLight
        /// </summary>
        /// <param name="radians"></param>
        private double RadiansToDegrees(double radians)
        {
            return (180 * radians) / Math.PI;
        }

        /// <summary>
        /// Computes the distance of the given line
        /// </summary>
        /// <param name="l1">The line that whose distance we want to calculate</param>
        /// <returns>The distance (in pixels)</returns>
        private double ComputeDistance(Line l1)
        {
            double a = l1.X2 - l1.X1;
            double b = l1.Y2 - l1.Y1;
            double c = Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            return c;
        }

        /// <summary>
        /// Private constructor. This is used only to get designed view to work. It MUST NOT BE 
        /// MADE PUBLIC. For correct initialization the stream needs a DrawingCanvas reference.
        /// </summary>
        private AbstractStream()
            : this(null, new Point())
        {

        }

        /// <summary>
        /// Make all classes that inherit from AbstractStream call this base constructor. In your inherting 
        /// class you should still have a default constructor that will be used to create a "factory instance" 
        /// that can give information about the type of stream (methods like IsAvailableWithDifficulty). So 
        /// you should have a constructor with no paramaters and a constructor with a single DrawingCanvas 
        /// paramater and they should both call this as the base constructor (use a null reference and point 
        /// (0,0) for parameters in the case of the default constructor).
        /// </summary>
        public AbstractStream(DrawingCanvas canvas, Point locationOnCanvas)
        {
            m_canvas = canvas;
            
            Brush transperent = new SolidColorBrush(Colors.Transparent);
            InitializeComponent();

            streamDestination = new StreamDestinationIcon(this, Arrow);

            Arrow.MouseLeftButtonDown += new MouseButtonEventHandler(ArrowMouseLeftButtonDown);

            //set the stream's id number
            s_streamIdCounter++;
            Id = "S_" + s_streamIdCounter;

            if (null != canvas)
            {
                // E.O.
                // Create the draggable source and destination icons
                m_srcDragIcon = new DraggableStreamEndpoint(
                    DraggableStreamEndpoint.EndpointType.StreamSourceNotConnected, this,
                    canvas);
                m_dstDragIcon = new DraggableStreamEndpoint(
                    DraggableStreamEndpoint.EndpointType.StreamDestinationNotConnected, this,
                    canvas);

                // Make sure the stem line has a low z-index
                Stem.SetValue(Canvas.ZIndexProperty, -1);

                // Set their sizes
                m_srcDragIcon.Width = m_srcDragIcon.Height = 40.0;
                m_dstDragIcon.Width = m_dstDragIcon.Height = 40.0;

                // Add them to the drawing canvas
                canvas.AddNewChild(m_srcDragIcon);
                canvas.AddNewChild(m_dstDragIcon);
                //LayoutRoot.Children.Add(m_srcDragIcon);
                //LayoutRoot.Children.Add(m_dstDragIcon);

                // Set positions
                m_srcDragIcon.Location = locationOnCanvas;
                m_dstDragIcon.Location = new Point(locationOnCanvas.X + 20.0, locationOnCanvas.Y);

                // Show them by default
                m_srcDragIcon.Visibility = System.Windows.Visibility.Visible;
                m_dstDragIcon.Visibility = System.Windows.Visibility.Visible;

                UpdateStreamLocation();

                // Create the table (abstract method so inheriting class will do it)
                CreatePropertiesTable();

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
        /// Fired whenever the user clicks on the Stream's "Hit Area"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HitArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Selected = true;
            e.Handled = true;
        }

        /// <summary>
        /// This calcualtes the intersection point between the stem and the edge of the destination object and
        /// sets the end of the stem to the intersection point.
        /// It works don't touch it
        /// </summary>
        /// <param name="source">This is the source of the stream</param>
        /// <param name="destination">This is destination it assumes length is subtracted from this side</param>
        /// <param name="length">Finds a new point this much closer to source from the intersection point</param>
        private Point toEdgeOfObject(UserControl source, UserControl destination, double length)
        {
            Point TopLeftOfSource = new Point((double)source.GetValue(Canvas.LeftProperty), (double)source.GetValue(Canvas.TopProperty));
            Point TopLeftOfDest = new Point((double)destination.GetValue(Canvas.LeftProperty), (double)destination.GetValue(Canvas.TopProperty));
            Point MidOfSource = new Point(TopLeftOfSource.X + source.Width / 2, TopLeftOfSource.Y + source.Height / 2);
            Point MidOfDest = new Point(TopLeftOfDest.X + destination.Width / 2, TopLeftOfDest.Y + destination.Height / 2);
            Point DistBetweenSandD = new Point(MidOfDest.X - MidOfSource.X, MidOfDest.Y - MidOfSource.Y);
            Point Intersection = new Point();
            Point EndOfStem = new Point();

            double angle;
            /*
             * Source is in the middle on this graph and the destion goes around it when it does DistBetweenSandD.X and .Y change signs as follows:
                               |
                      -X  -Y   |  +X  -Y
                        -------|--------
                               |
                       -X  +Y  | +X   +Y
                               |
            */
            if (DistBetweenSandD.X > 0 && DistBetweenSandD.Y > 0)
            {
                //So destination is down and to the right of source
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                /*      S
                 *      |\
                 *      |A\
                 *      |  \
                 * B    |   \
                 *      |    \
                 *      |   F \ Intersection Point
                 *      |----|-\---|
                 *    E |____|__\  |
                 *         C |   D |
                 *           |-----|
                 *    S shows where the middle point of the Source object is and D shows the middle point of the Destination Object.
                 *    A is the angle we are using to calculate the intersction point
                 *    The side labeled B is the distBetweenSandD.Y and C is the distBetweenSandD.X
                 *    Using Tan with can find the angle A which is the variable angle
                 *    Then we can find the short side labed E because it is D's hight / 2.
                 *    Then using that and angle A we cand find Side F which starts at side B and goes horizontal till it hits the line
                 *    Then we can find the intersection point and do a similar thing again for the arrow / circle size and bob's ur unlce
                 *
                 */

                angle = Math.Atan(DistBetweenSandD.X / DistBetweenSandD.Y);

                if (angle < Math.PI / 4)
                {
                    Intersection.Y = MidOfDest.Y - destination.Height / 2;
                    Intersection.X = MidOfSource.X + ((DistBetweenSandD.Y - destination.Width / 2) * Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y - (Math.Cos(angle) * length);
                    EndOfStem.X = Intersection.X - (Math.Sin(angle) * length);
                }
                else if (angle > Math.PI / 4)
                {
                    Intersection.X = MidOfDest.X - destination.Width / 2;
                    Intersection.Y = MidOfSource.Y + ((DistBetweenSandD.X - destination.Height / 2) / Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y - (Math.Cos(angle) * length);
                    EndOfStem.X = Intersection.X - (Math.Sin(angle) * length);
                }
                else
                {
                    Intersection.X = MidOfDest.X - destination.Width / 2;
                    Intersection.Y = MidOfDest.Y - destination.Height / 2;
                    EndOfStem.Y = Intersection.Y - (Math.Cos(angle) * length);
                    EndOfStem.X = Intersection.X - (Math.Sin(angle) * length);
                }
            }

            else if (DistBetweenSandD.X > 0 && DistBetweenSandD.Y < 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                angle = Math.Atan(DistBetweenSandD.Y / DistBetweenSandD.X);

                if (angle > Math.PI / 4)
                {
                    Intersection.Y = MidOfDest.Y + destination.Height / 2;
                    Intersection.X = MidOfDest.X - ((destination.Width / 2) / Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y + Math.Sin(angle) * length;
                    EndOfStem.X = Intersection.X - Math.Cos(angle) * length;
                }
                else if (angle < Math.PI / 4)
                {
                    Intersection.X = MidOfDest.X - destination.Width / 2;
                    Intersection.Y = MidOfSource.Y - (DistBetweenSandD.X - (destination.Height / 2)) * Math.Tan(angle);
                    EndOfStem.Y = Intersection.Y + Math.Sin(angle) * length;
                    EndOfStem.X = Intersection.X - Math.Cos(angle) * length;
                }
                else
                {
                    Intersection.X = MidOfDest.X - destination.Width / 2;
                    Intersection.Y = MidOfDest.Y + destination.Height / 2;
                    EndOfStem.Y = Intersection.Y + (Math.Sin(angle) * length);
                    EndOfStem.X = Intersection.X - (Math.Cos(angle) * length);
                }
            }

            else if (DistBetweenSandD.X < 0 && DistBetweenSandD.Y > 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                angle = Math.Atan(DistBetweenSandD.X / DistBetweenSandD.Y);

                if (angle < Math.PI / 4)
                {
                    Intersection.Y = MidOfDest.Y - destination.Height / 2;
                    Intersection.X = MidOfSource.X - ((DistBetweenSandD.Y - destination.Width / 2) * Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y - (Math.Cos(angle) * length);
                    EndOfStem.X = Intersection.X + (Math.Sin(angle) * length);
                }
                else if (angle > Math.PI / 4)
                {
                    Intersection.X = MidOfDest.X + destination.Width / 2;
                    Intersection.Y = MidOfSource.Y + ((DistBetweenSandD.X - destination.Height / 2) / Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y - (Math.Cos(angle) * length);
                    EndOfStem.X = Intersection.X + (Math.Sin(angle) * length);
                }
                else
                {
                    Intersection.X = MidOfDest.X + destination.Width / 2;
                    Intersection.Y = MidOfDest.Y - destination.Height / 2;
                    EndOfStem.Y = Intersection.Y - (Math.Cos(angle) * length);
                    EndOfStem.X = Intersection.X + (Math.Sin(angle) * length);
                }
            }

            else if (DistBetweenSandD.X < 0 && DistBetweenSandD.Y < 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                angle = Math.Atan(DistBetweenSandD.Y / DistBetweenSandD.X);
                if (angle > Math.PI / 4)
                {
                    Intersection.Y = MidOfDest.Y + destination.Height / 2;
                    Intersection.X = MidOfSource.X - ((DistBetweenSandD.Y - destination.Width / 2) / Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y + Math.Sin(angle) * length;
                    EndOfStem.X = Intersection.X + Math.Cos(angle) * length;
                }
                else if (angle < Math.PI / 4)
                {
                    Intersection.X = MidOfDest.X + destination.Width / 2;
                    Intersection.Y = MidOfSource.Y - ((DistBetweenSandD.X - destination.Height / 2) * Math.Tan(angle));
                    EndOfStem.Y = Intersection.Y + Math.Sin(angle) * length;
                    EndOfStem.X = Intersection.X + Math.Cos(angle) * length;
                }
                else
                {
                    Intersection.X = MidOfDest.X + destination.Width / 2;
                    Intersection.Y = MidOfDest.Y + destination.Height / 2;
                    EndOfStem.Y = Intersection.Y + (Math.Sin(angle) * length);
                    EndOfStem.X = Intersection.X + (Math.Cos(angle) * length);
                }
            }
            else if (DistBetweenSandD.X == 0 && DistBetweenSandD.Y > 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                EndOfStem.X = MidOfSource.X;
                EndOfStem.Y = MidOfSource.Y + (DistBetweenSandD.Y - (destination.Height / 2 + length));
            }
            else if (DistBetweenSandD.X == 0 && DistBetweenSandD.Y < 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                EndOfStem.X = MidOfSource.X;
                EndOfStem.Y = MidOfSource.Y - (DistBetweenSandD.Y - (destination.Height / 2 + length));
            }
            else if (DistBetweenSandD.Y == 0 && DistBetweenSandD.X > 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                EndOfStem.Y = MidOfSource.Y;
                EndOfStem.X = MidOfSource.X + (DistBetweenSandD.X - (destination.Width / 2 + length));
            }
            else if (DistBetweenSandD.Y == 0 && DistBetweenSandD.X < 0)
            {
                DistBetweenSandD.Y = Math.Abs(DistBetweenSandD.Y);
                DistBetweenSandD.X = Math.Abs(DistBetweenSandD.X);

                EndOfStem.Y = MidOfSource.Y;
                EndOfStem.X = MidOfSource.X - (DistBetweenSandD.X - (destination.Width / 2 + length));
            }
            else if (DistBetweenSandD.X == 0 && DistBetweenSandD.Y == 0)
            {
                EndOfStem.Y = MidOfDest.Y;
                EndOfStem.X = MidOfDest.X;
            }
            return (EndOfStem);
        }

        /// <summary>
        /// Fires when arror is clicked on and in return fires Arrow_MouseButtonLeftDown for the DrawingCanvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Arrow_MouseButtonLeftDown(this, e);
        }

        private void CircleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tail_MouseButtonLeftDown(this, e);
        }

        private void Arrow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
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
    }
}