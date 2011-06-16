using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Xml;

using ChemProV.PFD;
using ChemProV.PFD.Streams;


namespace ChemProV.PFD.ProcessUnits
{
    /// <summary>
    /// A simple implementation of the IProcessUnit interface.  This class should work
    /// for any process unit that doesn't require any special functionality.
    /// </summary>
    public partial class GenericProcessUnit : UserControl, IProcessUnit
    {
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
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int processUnitIdCounter = 0;
        private string processUnitId;

        public event EventHandler LocationChanged = delegate { };
        public event EventHandler SelectionChanged = delegate { };

        #endregion

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

        /// <summary>
        /// Gets/Sets the icon dependency property
        /// </summary>
        public Image Icon
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
                    SetBorderColor(new SolidColorBrush(Colors.Yellow));
                    SelectionChanged(this, new EventArgs());
                }
                else
                {
                    SetBorderColor(new SolidColorBrush(Colors.Transparent));
                }
            }
        }

        /// <summary>
        /// Sets the border around the process unit
        /// </summary>
        /// <param name="brush"></param>
        public void SetBorderColor(Brush brush)
        {
            ProcessUnitBorder.BorderBrush = brush;
        }

        /// <summary>
        /// Gets whether or not the IProcessUnit is accepting new incoming streams
        /// </summary>
        public bool IsAcceptingIncomingStreams
        {
            get
            {
                //-1 because -1 means infinity
                if (maxIncomingStreams == -1)
                    return true;
                else
                    return MaxIncomingStreams > IncomingStreams.Count;
            }
        }

        /// <summary>
        /// Gets whether or not the IProcessUnit is accepting new outgoing streams
        /// </summary>
        public bool IsAcceptingOutgoingStreams
        {
            get
            {
                //-1 because -1 means infinity
                if (maxOutgoingStreams == -1)
                    return true;
                else
                    return MaxOutgoingStreams > outgoingStreams.Count;
            }
        }

        /// <summary>
        /// Attaches a new incoming stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        public bool AttachIncomingStream(IStream stream)
        {
            if (IsAcceptingIncomingStreams)
            {
                IncomingStreams.Add(stream);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attaches a new outgoing stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        public bool AttachOutgoingStream(IStream stream)
        {
            if (IsAcceptingOutgoingStreams)
            {
                OutgoingStreams.Add(stream);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Dettaches an incoming stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to dettach</param>
        public void DettachIncomingStream(IStream stream)
        {
            incomingStreams.Remove(stream);
            //incomingStreams.ItemRemoved();
        }

        /// <summary>
        /// Dettaches an outgoing stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to dettach</param>
        public void DettachOutgoingStream(IStream stream)
        {
            outgoingStreams.Remove(stream);
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


        #endregion

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

        public void WriteXml(XmlWriter writer)
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

        #endregion

    }
}
