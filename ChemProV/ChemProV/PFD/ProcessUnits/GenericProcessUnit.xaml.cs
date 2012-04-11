/*
Copyright 2010, 2011 HELP Lab @ Washington State University

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
    /// </summary>
    public partial class GenericProcessUnit : UserControl, IProcessUnit
    {
        public event EventHandler StreamsChanged = delegate { };

        #region Instance Variables

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// </summary>
        private string description;

        /// <summary>
        /// Total number of incoming streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxIncomingStreams;

        /// <summary>
        /// Total number of outgoing streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxOutgoingStreams;

        /// <summary>
        /// Total number of incoming streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxIncomingHeatStreams;

        /// <summary>
        /// Total number of outgoing streams allowed.  A value of zero is taken to mean unlimited.
        /// </summary>
        private int maxOutgoingHeatStreams;

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
        {
            InitializeComponent();

            //Create bindings that listen for changes in the object's location
            SetBinding(Canvas.LeftProperty, new Binding("LeftProperty") { Source = this, Mode = BindingMode.TwoWay });
            SetBinding(Canvas.TopProperty, new Binding("TopProperty") { Source = this, Mode = BindingMode.TwoWay });

            processUnitIdCounter++;
            Id = "GPU_" + processUnitIdCounter;
        }

        #region IProcessUnit Members

        public Point MidPoint
        {
            get
            {
                return new Point((double)this.GetValue(Canvas.LeftProperty) + this.ActualWidth / 2, (double)this.GetValue(Canvas.TopProperty) + this.ActualHeight / 2);
            }
        }

        /// <summary>
        /// Gets/Sets the icon dependency property
        /// </summary>
        public virtual Image Icon
        {
            get
            {
                return ProcessUnitImage;
            }
            set
            {
                ProcessUnitImage.Source = value.Source;
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
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public int MaxIncomingStreams
        {
            get
            {
                return maxIncomingStreams;
            }
            set
            {
                maxIncomingStreams = value;
            }
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public int MaxOutgoingStreams
        {
            get
            {
                return maxOutgoingStreams;
            }
            set
            {
                maxOutgoingStreams = value;
            }
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public int MaxIncomingHeatStreams
        {
            get
            {
                return maxIncomingHeatStreams;
            }
            set
            {
                maxIncomingHeatStreams = value;
            }
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public int MaxOutgoingHeatStreams
        {
            get
            {
                return maxOutgoingHeatStreams;
            }
            set
            {
                maxOutgoingHeatStreams = value;
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
            writer.WriteAttributeString(
                                        "ProcessUnitType",
                                        ProcessUnitFactory.GetProcessUnitType(this).ToString()
                                        );

            //the process units location
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", GetValue(Canvas.LeftProperty).ToString());
            writer.WriteElementString("Y", GetValue(Canvas.TopProperty).ToString());
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
            return targetUnit;
        }

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

        /// <summary>
        /// Uber-hack used to track changes in the process unit's position.
        /// Should not be called directly.  Instead, use Canvas.LeftProperty.
        /// </summary>
        public Double TopProperty
        {
            get
            {
                return Convert.ToDouble(GetValue(Canvas.TopProperty));
            }
            set
            {
                LocationChanged(this, new EventArgs());
            }
        }

        #endregion non-inherited properties
    }
}