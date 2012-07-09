// This class is a work in progress that's part of a long-term effort to 
// de-couple the logic and UI layers

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ChemProV.Core;
using System.Xml.Linq;
using ChemProV.Logic;

namespace ChemProV.Core
{
    public abstract class AbstractStream : INotifyPropertyChanged
    {
        private ObservableCollection<StickyNote> m_comments =
            new ObservableCollection<StickyNote>();

        private AbstractProcessUnit m_destination = null;

        /// <summary>
        /// Location of the stream destination. It is up to the UI layer how to use this.
        /// </summary>
        private MathCore.Vector m_destLoc = new MathCore.Vector();

        private AbstractProcessUnit m_source = null;

        /// <summary>
        /// Location of the stream source. It is up to the UI layer how to use this.
        /// </summary>
        private MathCore.Vector m_sourceLoc = new MathCore.Vector();

        private StreamPropertiesTable m_table = null;

        /// <summary>
        /// Unique identifier for the stream
        /// </summary>
        private int m_uid;

        private static int s_idCounter = 1;

        public AbstractStream() : this(GetNextUID()) { }

        public AbstractStream(int uniqueId)
        {
            m_uid = uniqueId;
        }

        /// <summary>
        /// Loads the stream from an XML element and attaches process units. All the process units 
        /// that could potentially be reference by the stream should be in the list.
        /// </summary>
        public AbstractStream(XElement loadFromMe, IList<AbstractProcessUnit> processUnits)
        {
            // Get the ID attribute and parse out the integer. Stream IDs are stored as strings 
            // of the form "S_#".
            string idString = (string)loadFromMe.Attribute("Id");
            m_uid = Convert.ToInt32(idString.Split('_')[1]);

            // Get the process unit type (not needed here; higher level code will use it)
            //string unitType = (string)loadFromMe.Attribute("StreamType");

            // Check for unconnected endpoints
            XElement usEl = loadFromMe.Element("UnattachedSource");
            if (null != usEl)
            {
                XAttribute locAttr = usEl.Attribute("Location");
                if (null != locAttr)
                {
                    ParsePoint(locAttr.Value, out m_sourceLoc.X, out m_sourceLoc.Y);
                }
            }
            XElement udEl = loadFromMe.Element("UnattachedDestination");
            if (null != udEl)
            {
                XAttribute locAttr = udEl.Attribute("Location");
                if (null != locAttr)
                {
                    ParsePoint(locAttr.Value, out m_destLoc.X, out m_destLoc.Y);
                }
            }

            // Read source and destination process unit IDs (if present)
            XElement srcEl = loadFromMe.Element("Source");
            if (null != srcEl)
            {
                foreach (AbstractProcessUnit apu in processUnits)
                {
                    if (apu.UIDString == srcEl.Value)
                    {
                        // Connect as source
                        Source = apu;
                        apu.AttachOutgoingStream(this);
                        m_sourceLoc = apu.Location;
                        break;
                    }
                }
            }
            XElement dstEl = loadFromMe.Element("Destination");
            if (null != dstEl)
            {
                foreach (AbstractProcessUnit apu in processUnits)
                {
                    if (apu.UIDString == dstEl.Value)
                    {
                        // Connect as destination
                        Destination = apu;
                        apu.AttachIncomingStream(this);
                        m_destLoc = apu.Location;
                        break;
                    }
                }
            }

            // Load any comments that are present
            XElement cmtElement = loadFromMe.Element("Comments");
            if (null != cmtElement)
            {
                foreach (XElement child in cmtElement.Elements())
                {
                    m_comments.Add(new StickyNote(child, UIDString));
                }
            }
        }

        public ObservableCollection<StickyNote> Comments
        {
            get
            {
                return m_comments;
            }
        }

        /// <summary>
        /// Utility function to see if there exists a comment with the specified location
        /// </summary>
        public bool ContainsCommentWithLocation(double x, double y)
        {
            foreach (StickyNote sn in m_comments)
            {
                if (sn.LocationX == x && sn.LocationY == y)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Gets or sets the destination process unit for this stream. This value may be null.
        /// </summary>
        public AbstractProcessUnit Destination
        {
            get
            {
                return m_destination;
            }
            set
            {
                if (m_destination == value)
                {
                    // No change
                    return;
                }

                // If we had a previous one, unsubscribe
                if (null != m_destination)
                {
                    m_destination.PropertyChanged -= this.DestinationUnitPropertyChanged;
                }

                // Store the new value
                m_destination = value;

                // Subscribe to the new process unit
                if (null != m_destination)
                {
                    m_destLoc = m_destination.Location;
                    m_destination.PropertyChanged += this.DestinationUnitPropertyChanged;
                }

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Destination"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the location of the destination for the stream. It is up to the UI layer how to 
        /// use this value. Logically it is an ordered (X, Y) pair. We use a vector object because it can 
        /// serve just fine as an ordered pair with the added benefit of being easily usable for 
        /// calculations that may useful when dealing with component locations.
        /// When the destination is non-null, this value is automatically updated as the location of the 
        /// destination process unit changes.
        /// </summary>
        public MathCore.Vector DestinationLocation
        {
            get
            {
                return m_destLoc;
            }
            set
            {
                if (m_destLoc.Equals(value))
                {
                    // No change
                    return;
                }

                m_destLoc = value;

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DestinationLocation"));
                }
            }
        }

        private void DestinationUnitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Location"))
            {
                m_destination.PropertyChanged -= this.DestinationUnitPropertyChanged;
                DestinationLocation = m_destination.Location;
                m_destination.PropertyChanged += this.DestinationUnitPropertyChanged;
            }
        }

        public static int GetNextUID()
        {
            return s_idCounter++;
        }

        /// <summary>
        /// Gets or sets a unique integer identifier for this stream. Logic outside of this class 
        /// must enforce the uniqueness of stream IDs.
        /// </summary>
        public int Id
        {
            get
            {
                return m_uid;
            }
            set
            {
                if (value == m_uid)
                {
                    // No change
                    return;
                }
                
                m_uid = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
        }

        public abstract bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty);

        /// <summary>
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid source. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more outgoing 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid source, false otherwise.</returns>
        public abstract bool IsValidSource(AbstractProcessUnit unit);

        /// <summary>
        /// E.O.
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid destination. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more incoming 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid destination, false otherwise.</returns>
        public abstract bool IsValidDestination(AbstractProcessUnit unit);

        /// <summary>
        /// Parses X and Y values out of a string of the form "X,Y". On failure, the 
        /// values are set to 0.0 and false is returned.
        /// </summary>
        private static bool ParsePoint(string pointString, out double x, out double y)
        {
            // The expected format of the string is "X,Y"
            if (null == pointString || !pointString.Contains(","))
            {
                x = 0.0;
                y = 0.0;
                return false;
            }

            string[] components = pointString.Split(',');
            if (null == components || components.Length < 2)
            {
                x = 0.0;
                y = 0.0;
                return false;
            }

            if (double.TryParse(components[0], out x) && double.TryParse(components[1], out y))
            {
                return true;
            }

            x = 0.0;
            y = 0.0;
            return false;
        }

        public StreamPropertiesTable PropertiesTable
        {
            get
            {
                return m_table;
            }
            set
            {
                if (object.ReferenceEquals(m_table, value))
                {
                    // No change
                    return;
                }

                m_table = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PropertiesTable"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the source process unit for this stream. This value may be null.
        /// </summary>
        public AbstractProcessUnit Source
        {
            get
            {
                return m_source;
            }
            set
            {
                if (m_source == value)
                {
                    // No change
                    return;
                }

                // If we had a previous one, unsubscribe
                if (null != m_source)
                {
                    m_source.PropertyChanged -= this.SourceUnitPropertyChanged;
                }

                // Store the new value
                m_source = value;

                // Subscribe to the new process unit
                if (null != m_source)
                {
                    m_sourceLoc = m_source.Location;
                    m_source.PropertyChanged += this.SourceUnitPropertyChanged;
                }

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Source"));
                }
            }
        }

        /// <summary>
        /// Represents the location of the source for the stream. It is up to the UI layer how to use 
        /// this value. Logically it is an ordered (X, Y) pair. We use a vector object because it can 
        /// serve just fine as an ordered pair with the added benefit of being easily usable for 
        /// calculations that may useful when dealing with component locations.
        /// When the source is non-null, this value is automatically updated as the location of the 
        /// source process unit changes.
        /// </summary>
        public MathCore.Vector SourceLocation
        {
            get
            {
                return m_sourceLoc;
            }
            set
            {
                if (m_sourceLoc.Equals(value))
                {
                    // No change
                    return;
                }

                m_sourceLoc = value;

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SourceLocation"));
                }
            }
        }

        private void SourceUnitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Location"))
            {
                m_source.PropertyChanged -= this.SourceUnitPropertyChanged;
                SourceLocation = m_source.Location;
                m_source.PropertyChanged += this.SourceUnitPropertyChanged;
            }
        }

        /// <summary>
        /// The string type that gets written to files and identifies the type of 
        /// of inheriting class.
        /// </summary>
        public abstract string StreamType
        {
            get;
        }

        public abstract string Title
        {
            get;
        }

        public string UIDString
        {
            get
            {
                return "S_" + m_uid.ToString();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            // The file format is such that the unique identifer gets written as a string of the 
            // form "S_#".
            writer.WriteAttributeString("Id", "S_" + m_uid.ToString());

            // Also write the stream type as an attribute
            writer.WriteAttributeString("StreamType", StreamType);

            // Write the source and destination process unit info
            if (null == m_source)
            {
                writer.WriteStartElement("UnattachedSource");
                writer.WriteAttributeString("Location", m_sourceLoc.X.ToString() + 
                    "," + m_sourceLoc.Y.ToString());
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString("Source", m_source.UIDString);
            }
            if (null == m_destination)
            {
                writer.WriteStartElement("UnattachedDestination");
                writer.WriteAttributeString("Location", m_destLoc.X.ToString() +
                    "," + m_destLoc.Y.ToString());
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString("Destination", m_destination.UIDString);
            }

            // Write any and all comments
            if (m_comments.Count > 0)
            {
                writer.WriteStartElement("Comments");
                for (int i = 0; i < m_comments.Count; i++)
                {
                    writer.WriteStartElement("Comment");
                    m_comments[i].WriteXml(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            // Note that we don't write the stream properties data. That happens elsewhere (this 
            // was in place long ago and we just have to maintain compatibility with the file 
            // format
        }

        public event PropertyChangedEventHandler PropertyChanged = null;
    }
}
