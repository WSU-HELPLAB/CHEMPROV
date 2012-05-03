/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using ChemProV.PFD.Streams;
using System.Xml.Linq;

namespace ChemProV.PFD.ProcessUnits
{
    public enum ProcessUnitBorderColor
    {
        Selected,
        NotAcceptingStreams,
        AcceptingStreams,
        NoBorder
    }

    /// <summary>
    /// A simple implementation of the IProcessUnit interface.  This class should work
    /// for any process unit that doesn't require any special functionality.
    /// E.O.
    /// The direction I'm trying to go in with my refactoring is to eventually make this an 
    /// abstract base class. Before my refactoring, generic process units were used as 
    /// draggable stream endpoint icons, but I'm working on logic to eliminate that.
    /// </summary>
    public partial class GenericProcessUnit : UserControl, IProcessUnit, Core.ICanvasElement
    {
        public event EventHandler StreamsChanged = delegate { };

        #region Instance Variables

        /// <summary>
        /// Total number of incoming streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxIncomingStreams = 1;

        /// <summary>
        /// Total number of outgoing streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxOutgoingStreams = 1;

        /// <summary>
        /// Total number of incoming streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxIncomingHeatStreams = 1;

        /// <summary>
        /// Total number of outgoing streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxOutgoingHeatStreams = 1;

        /// <summary>
        /// Collection of incoming streams
        /// </summary>
        protected ObservableCollection<IStream> incomingStreams = new ObservableCollection<IStream>();

        /// <summary>
        /// Collection of outgoing streams
        /// </summary>
        protected ObservableCollection<IStream> outgoingStreams = new ObservableCollection<IStream>();

        /// <summary>
        /// Private instance var used to keep track of whether or not we've been selected
        /// </summary>
        private bool isSelected = false;

        /// <summary>
        /// E.O.
        /// Specifies the source for the icon for this process unit
        /// </summary>
        private string m_iconSource;

        /// <summary>
        /// E.O.
        /// Specifies the color for the subprocess. White is default.
        /// </summary>
        protected Color m_subprocess = Colors.White;

        private Brush SelectedBorderBrush = new SolidColorBrush(Colors.Yellow);
        private Brush GreenBorderBrush = new SolidColorBrush(Colors.Green);
        private Brush RedBorderBrush = new SolidColorBrush(Colors.Red);
        private Brush TransparentBorderBrush = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int processUnitIdCounter = 0;
        private string processUnitId;

        public event EventHandler LocationChanged = delegate { };
        public event EventHandler SelectionChanged = delegate { };

        #endregion Instance Variables

        /// <summary>
        /// Default constructor
        /// </summary>
        public GenericProcessUnit()
            : this("/UI/Icons/pu_generic.png")
        {

        }

        public GenericProcessUnit(string iconSource)
        {
            InitializeComponent();

            //Create bindings that listen for changes in the object's location
            SetBinding(Canvas.LeftProperty, new Binding("LeftProperty") { Source = this, Mode = BindingMode.TwoWay });
            SetBinding(Canvas.TopProperty, new Binding("TopProperty") { Source = this, Mode = BindingMode.TwoWay });

            processUnitIdCounter++;
            Id = "GPU_" + processUnitIdCounter;

            // E.O.
            // Store the source
            m_iconSource = iconSource;
            // Create the icon image
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(m_iconSource, UriKind.Relative);
            ProcessUnitImage.SetValue(Image.SourceProperty, bmp);
        }

        #region IProcessUnit Members

        public Point MidPoint
        {
            get
            {
                if (double.IsNaN(Width) || double.IsNaN(Height))
                {
                    return new Point(
                        (double)this.GetValue(Canvas.LeftProperty) + this.ActualWidth / 2,
                        (double)this.GetValue(Canvas.TopProperty) + this.ActualHeight / 2);
                }

                return new Point(
                    (double)this.GetValue(Canvas.LeftProperty) + this.Width / 2,
                    (double)this.GetValue(Canvas.TopProperty) + this.Height / 2);
            }

            // E.O.
            set
            {
                this.SetValue(Canvas.LeftProperty, value.X - this.Width / 2.0);
                this.SetValue(Canvas.TopProperty, value.Y - this.Height / 2.0);
            }
        }

        /// <summary>
        /// Gets the icon dependency property
        /// </summary>
        public virtual Image Icon
        {
            get
            {
                return ProcessUnitImage;
            }
        }

        /// <summary>
        /// Use to reference the border around the process unit's icon.  
        /// </summary>
        public virtual Border IconBorder
        {
            get
            {
                return ProcessUnitBorder;
            }
            set
            {
                ProcessUnitBorder = value;
            }
        }

        public string IconSource
        {
            get
            {
                return m_iconSource;
            }
        }

        /// <summary>
        /// Gets or sets the IProcessUnit's unique ID number
        /// </summary>
        public String Id
        {
            get
            {
                return processUnitId;
            }
            set
            {
                //special condition when loading process units from file.
                //essentially, file ID elements will probably be larger than the current
                //counter, which means that we'll run into trouble if, by chance,
                //two process units get the same ID number.  To fix, check the
                //incoming ID number, if higher, than make the counter larger than the last ID number.
                string[] pieces = value.Split('_');
                int idNumber = Convert.ToInt32(pieces[1]);
                if (idNumber > processUnitIdCounter)
                {
                    processUnitIdCounter = idNumber + 1;
                }
                processUnitId = value;
            }
        }

        /// <summary>
        /// E.O.
        /// Returns a boolean value indicating whether or not the processing unit should be available 
        /// with the specified difficulty setting.
        /// TODO: Make this method abstract. It's already implemented in each inherting class at the 
        /// time of this writing, except for LabeledProcessUnit and TemporaryProcessUnit.
        /// A lot would have to change to make this class abstract so this is a low priority 
        /// item at the moment.
        /// </summary>
        public virtual bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // E.O.
            // Comparing string descriptions to determine unit type...
            // I hate to do things this way, but it'll at least be an improvement over what 
            // existed before. Eventually there should be refactoring that makes a central 
            // location for the logic of what's available under what difficulty level. There 
            // are a variety of ways to do this, but this enough of a step in that direction 
            // for now.

            // Everything is available under the highest difficulty
            if (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty)
            {
                return true;
            }

            // Under "medium" difficulty, the following are not available:
            //  - Heat exchanger with utility
            //  - Heat exchanger without utility
            if (OptionDifficultySetting.MaterialBalanceWithReactors == difficulty)
            {
                if (Description.Equals(ProcessUnitDescriptions.HeatExchanger) ||
                    Description.Equals(ProcessUnitDescriptions.HeatExchangerNoUtility))
                {
                    return false;
                }
                return true;
            }

            // Coming here implies that the difficulty is the simplest. Under this difficulty, 
            // the following are not available:
            //  - Heat exchanger with utility
            //  - Heat exchanger without utility
            //  - Reactor
            if (Description.Equals(ProcessUnitDescriptions.HeatExchanger) ||
                Description.Equals(ProcessUnitDescriptions.HeatExchangerNoUtility) ||
                Description.Equals(ProcessUnitDescriptions.Reactor))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shorthand for getting the IProcessUnit's integer component of its id
        /// </summary>
        public int ProcessUnitId
        {
            get
            {
                int id = 0;
                Int32.TryParse(this.Id.Split('_')[1], out id);
                return id;
            }
        }

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// E.O.
        /// This class should eventually be abstract and so should this property. But that's 
        /// pretty low priority at this point.
        /// </summary>
        public virtual string Description
        {
            get
            {
                return "Generic Process Unit";
            }
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxIncomingStreams
        {
            get
            {
                return maxIncomingStreams;
            }            
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxOutgoingStreams
        {
            get
            {
                return maxOutgoingStreams;
            }
        }

        /// <summary>
        /// Total number of incoming heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxIncomingHeatStreams
        {
            get
            {
                return maxIncomingHeatStreams;
            }
        }

        /// <summary>
        /// Total number of outgoing heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxOutgoingHeatStreams
        {
            get
            {
                return maxOutgoingHeatStreams;
            }
        }

        /// <summary>
        /// List of incoming streams
        /// </summary>
        public IList<IStream> IncomingStreams
        {
            get
            {
                return incomingStreams;
            }
        }

        /// <summary>
        /// List of outgoing streams
        /// </summary>
        public IList<IStream> OutgoingStreams
        {
            get
            {
                return outgoingStreams;
            }
        }

        /// <summary>
        /// Gets or sets the selection flag for the stream
        /// </summary>
        public Boolean Selected
        {
            get
            {
                return isSelected;
            }
            set
            {
                bool oldValue = isSelected;
                isSelected = value;

                //either turn the highlight on or off
                if (isSelected)
                {
                    SetBorderColor(ProcessUnitBorderColor.Selected);
                    SelectionChanged(this, new EventArgs());
                }
                else
                {
                    SetBorderColor(ProcessUnitBorderColor.NoBorder);
                }
            }
        }

        /// <summary>
        /// Sets the border around the process unit
        /// </summary>
        /// <param name="brush"></param>
        public void SetBorderColor(ProcessUnitBorderColor borderColor)
        {
            Brush brush;
            if (borderColor == ProcessUnitBorderColor.Selected)
            {
                brush = SelectedBorderBrush;
            }
            else if (borderColor == ProcessUnitBorderColor.NotAcceptingStreams)
            {
                brush = RedBorderBrush;
            }
            else if (borderColor == ProcessUnitBorderColor.AcceptingStreams)
            {
                brush = GreenBorderBrush;
            }
            else
            {
                brush = TransparentBorderBrush;
            }
            IconBorder.BorderThickness = new Thickness(2);
            IconBorder.BorderBrush = brush;
        }

        /// <summary>
        /// Gets whether or not the IProcessUnit is accepting new incoming streams
        /// </summary>
        public bool IsAcceptingIncomingStreams(IStream stream)
        {
            //first, figure out how many units we have
            var result = from c in incomingStreams
                         where c.GetType() == stream.GetType()
                         select c;
            int numResults = result.Count();

            //at this poing I'm breaking down and doing type checking.  I think that there's
            //a way to not check types, but for now, this is what I have to do
            int maxTypes = 0;
            if (stream is HeatStream)
            {
                maxTypes = MaxIncomingHeatStreams;
            }
            else if (stream is ChemicalStream)
            {
                maxTypes = MaxIncomingStreams;
            }

            //-1 because -1 means infinity
            if (maxTypes == -1)
                return true;
            else
                return maxTypes > numResults;
        }

        /// <summary>
        /// Gets whether or not the IProcessUnit is accepting new outgoing streams
        /// </summary>
        public bool IsAcceptingOutgoingStreams(IStream stream)
        {
            //first, figure out how many units we have
            var result = from c in outgoingStreams
                         where c.GetType() == stream.GetType()
                         select c;
            int numResults = result.Count();

            //at this poing I'm breaking down and doing type checking.  I think that there's
            //a way to not check types, but for now, this is what I have to do
            int maxTypes = 0;
            if (stream is HeatStream)
            {
                maxTypes = MaxOutgoingHeatStreams;
            }
            else if (stream is ChemicalStream)
            {
                maxTypes = MaxOutgoingStreams;
            }

            //-1 because -1 means infinity
            if (maxTypes == -1)
                return true;
            else
                return maxTypes > numResults;
        }

        /// <summary>
        /// Attaches a new incoming stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        public bool AttachIncomingStream(IStream stream)
        {
            if (IsAcceptingIncomingStreams(stream))
            {
                IncomingStreams.Add(stream);
                StreamsChanged(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attaches a new outgoing stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        public bool AttachOutgoingStream(IStream stream)
        {
            if (IsAcceptingOutgoingStreams(stream))
            {
                OutgoingStreams.Add(stream);
                StreamsChanged(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dettaches an incoming stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to dettach</param>
        public void DettachIncomingStream(IStream stream)
        {
            incomingStreams.Remove(stream);
            StreamsChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Dettaches an outgoing stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to dettach</param>
        public void DettachOutgoingStream(IStream stream)
        {
            outgoingStreams.Remove(stream);
            StreamsChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// IProcessUnit cannot have feedback associated with it so return null.
        /// Must impliment these functions as it is a part of IpfdElement
        /// </summary>
        /// <param name="highlight"></param>
        public void HighlightFeedback(bool highlight)
        {
            return;
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            return;
        }

        public void RemoveFeedback()
        {
            return;
        }

        /// <summary>
        /// Gets or sets the subprocess color
        /// </summary>
        public virtual Color Subprocess
        {
            get
            {
                return m_subprocess;    
            }
            set
            {
                if (value.Equals(m_subprocess))
                {
                    // No change
                    return;
                }
                
                m_subprocess = value;

                // We need to update the control's background color
                this.Background = new SolidColorBrush(m_subprocess);
            }
        }

        #endregion IProcessUnit Members

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// This isn't used as the IProcessUnitFactory is responsible for the creation
        /// of new process units.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            //the process unit's id number
            writer.WriteAttributeString("Id", Id);

            //the type of process unit
            writer.WriteAttributeString("ProcessUnitType",
                ProcessUnitFactory.GetProcessUnitType(this).ToString());

            //the process units location
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", GetValue(Canvas.LeftProperty).ToString());
            writer.WriteElementString("Y", GetValue(Canvas.TopProperty).ToString());
            writer.WriteEndElement();

            // E.O.
            // Write subprocess information, which right now is just an RGBA color
            writer.WriteStartElement("Subgroup");
            writer.WriteAttributeString("Color", Subprocess.ToString());
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members

        public virtual IProcessUnit FromXml(XElement xpu, IProcessUnit targetUnit)
        {
            UIElement puElement = targetUnit as UIElement;

            //pull the attribute
            string id = (string)xpu.Attribute("Id");
            targetUnit.Id = id;

            //set the correct coordinates for the object
            var location = from c in xpu.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            puElement.SetValue(Canvas.LeftProperty, Convert.ToDouble(location.ElementAt(0).x));
            puElement.SetValue(Canvas.TopProperty, Convert.ToDouble(location.ElementAt(0).y));

            // E.O.
            XElement subgroupEl = xpu.Element("Subgroup");
            if (null != subgroupEl)
            {
                XAttribute sgAttr = subgroupEl.Attribute("Color");
                if (null != sgAttr)
                {
                    Color clr;
                    if (!Core.App.TryParseColor(sgAttr.Value, out clr))
                    {
                        clr = Colors.White;
                    }
                    this.Subprocess = clr;
                }
            }

            return targetUnit;
        }

        #region ICanvasElement Members

        public virtual Point Location
        {
            get
            {
                return MidPoint;
            }
            set
            {
                if (!MidPoint.Equals(value))
                {
                    MidPoint = value;
                    if (null != LocationChanged)
                    {
                        LocationChanged(this, new EventArgs());
                    }
                }
            }
        }

        #endregion

        #region non-inherited properties

        /// <summary>
        /// Uber-hack used to track changes in the process unit's position.
        /// Should not be called directly.  Instead, use Canvas.LeftProperty.
        /// </summary>
        public Double LeftProperty
        {
            get
            {
                return Convert.ToDouble(GetValue(Canvas.LeftProperty));
            }
            set
            {
                LocationChanged(this, new EventArgs());
            }
        }

        

        #endregion non-inherited properties
    }
}