// This class is a work in progress that's part of a long-term effort to 
// de-couple the logic and UI layers

/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using ChemProV.Core;

// Temporary until further refactoring
using ChemProV.Logic;

namespace ChemProV.Core
{
    /// <summary>
    /// Abstract base class for a UI-independent process unit. "UI-independent" in this case means there are no 
    /// dependencies on frameworks like Silverlight, WPF, WinForms etc. There are still UI-oriented properties 
    /// in these process units.
    /// This outlines all of the basic functionality encapsulated by any process unit.
    /// </summary>
    public abstract class AbstractProcessUnit : INotifyPropertyChanged
    {
        #region Member variables

        private ObservableCollection<StickyNote> m_comments = 
            new ObservableCollection<StickyNote>();

        /// <summary>
        /// Unique identifier for the process unit
        /// </summary>
        private int m_uid;

        private List<AbstractStream> m_incomingStreams = new List<AbstractStream>();

        private string m_label = string.Empty;

        private MathCore.Vector m_location = new MathCore.Vector();

        private List<AbstractStream> m_outgoingStreams = new List<AbstractStream>();
        
        private string m_subprocess = "#FFffFFff";

        #endregion

        private static int s_idCounter = 1;

        public AbstractProcessUnit(int id, string label)
        {
            m_uid = id;
            m_label = label;
        }

        public AbstractProcessUnit(XElement loadFromMe)
        {
            // Make sure we have the correct type of XElement
            string elNameLwr = loadFromMe.Name.LocalName.ToLower();
            if (!elNameLwr.Equals("genericprocessunit"))
            {
                throw new ArgumentException("XElement for a process unit must be a " +
                    "<GenericProcessUnit>. Element was named: " + elNameLwr);
            }
            
            // Parse out the integer ID. Older versions of the app saved the integer identifier 
            // in a string of the format "GPU_#" (where # is the ID). Since we can't change the 
            // file format at this point, we have to maintain compatibility with this system.
            string idString = loadFromMe.Attribute("Id").Value;
            string[] parts = idString.Split('_');
            m_uid = Convert.ToInt32(parts[1]);

            // Load the label
            m_label = loadFromMe.Attribute("Name").Value;

            //set the correct coordinates for the object
            var location = from c in loadFromMe.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            m_location.X = Convert.ToDouble(location.ElementAt(0).x);
            m_location.Y = Convert.ToDouble(location.ElementAt(0).y);

            // Load the subgroup, if present. We default to "#FFffFFff" if we can't load it.
            m_subprocess = "#FFffFFff";
            XElement subgroupEl = loadFromMe.Element("Subgroup");
            if (null != subgroupEl)
            {
                XAttribute sgAttr = subgroupEl.Attribute("Color");
                if (null != sgAttr)
                {
                    m_subprocess = sgAttr.Value;
                }
            }

            // Load any comments that are present
            XElement cmtElement = loadFromMe.Element("Comments");
            if (null != cmtElement)
            {
                foreach (XElement child in cmtElement.Elements())
                {
                    m_comments.Add(new StickyNote(child, idString));
                }
            }
        }

        /// <summary>
        /// Attaches a new incoming stream to the process unit, if possible.
        /// </summary>
        /// <param name="stream">The stream to attach. Upon success, this stream is added to 
        /// the collection of incoming streams for the process unit.</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        public virtual bool AttachIncomingStream(AbstractStream stream)
        {
            if (CanAcceptIncomingStream(stream))
            {
                m_incomingStreams.Add(stream);
                StreamsChanged(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attaches a new outgoing stream to the process unit
        /// </summary>
        /// <param name="stream">The stream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        public virtual bool AttachOutgoingStream(AbstractStream stream)
        {
            if (CanAcceptOutgoingStream(stream))
            {
                m_outgoingStreams.Add(stream);
                StreamsChanged(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets whether or not the process unit can accept the specified stream as an incoming stream 
        /// in its current state.
        /// </summary>
        public virtual bool CanAcceptIncomingStream(AbstractStream stream)
        {
            // We can never accept a single stream as both incoming and outgoing (meaning this 
            // process unit would be both the source and destination of the stream), so check 
            // for this first.
            if (this == stream.Source)
            {
                return false;
            }

            //first, figure out how many units we have
            var result = from c in m_incomingStreams
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
        /// Gets whether or not the process unit can accept the specified stream as an outgoing stream 
        /// in its current state.
        /// </summary>
        public virtual bool CanAcceptOutgoingStream(AbstractStream stream)
        {
            // We can never accept a single stream as both incoming and outgoing (meaning this 
            // process unit would be both the source and destination of the stream), so check 
            // for this first.
            if (this == stream.Destination)
            {
                return false;
            }
            
            //first, figure out how many units we have
            var result = from c in m_outgoingStreams
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
        /// A short description of the process unit. Not more than a few words in length.
        /// </summary>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Detaches an incoming stream from the process unit
        /// </summary>
        /// <param name="stream">The stream to detach</param>
        public virtual void DetachIncomingStream(AbstractStream stream)
        {
            m_incomingStreams.Remove(stream);
            StreamsChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Dettaches an outgoing stream to the process unit
        /// </summary>
        /// <param name="stream">The stream to dettach</param>
        public virtual void DetachOutgoingStream(AbstractStream stream)
        {
            m_outgoingStreams.Remove(stream);
            StreamsChanged(this, EventArgs.Empty);
        }

        public static int GetNextUID()
        {
            return s_idCounter++;
        }
        
        /// <summary>
        /// Gets a unique integer identifier for this process unit. Logic outside of this class must 
        /// enforce the uniqueness of process unit IDs.
        /// </summary>
        public int Id
        {
            get
            {
                return m_uid;
            }
        }

        public int IncomingStreamCount
        {
            get
            {
                return m_incomingStreams.Count;
            }
        }

        /// <summary>
        /// Gets a read-only collection of incoming streams. Use the attach/detach methods if you wish 
        /// to alter the collection of incoming streams.
        /// </summary>
        public IEnumerable<AbstractStream> IncomingStreams
        {
            get
            {
                return m_incomingStreams;
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not this processing unit should be available 
        /// with the specified difficulty setting.
        /// </summary>
        /// <param name="difficulty">Difficulty setting</param>
        /// <returns>True if available with the difficulty setting, false otherwise.</returns>
        public abstract bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty);

        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                if (m_label == value || string.IsNullOrEmpty(value))
                {
                    // No change or invalid
                    return;
                }

                m_label = value;

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Label"));
                }
            }
        }

        /// <summary>
        /// Gets or sets process unit's location within the UI. It is up to the UI layer how to 
        /// use this value to position the process unit.
        /// </summary>
        public MathCore.Vector Location
        {
            get
            {
                return m_location;
            }
            set
            {
                if (m_location.Equals(value))
                {
                    // No change
                    return;
                }

                // Store the new value
                m_location = value;

                // Invoke the property-changed event if non-null
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Location"));
                }
            }
        }

        /// <summary>
        /// Gets the total number of incoming heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public abstract int MaxIncomingHeatStreams
        {
            get;
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public abstract int MaxIncomingStreams
        {
            get;
        }

        /// <summary>
        /// Total number of outgoing heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public abstract int MaxOutgoingHeatStreams
        {
            get;
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public abstract int MaxOutgoingStreams
        {
            get;
        }

        public int OutgoingStreamCount
        {
            get
            {
                return m_outgoingStreams.Count;
            }
        }

        /// <summary>
        /// Gets a read-only collection of outgoing streams. Use the attach/detach methods if you wish 
        /// to alter the collection of outgoing streams.
        /// </summary>
        public IEnumerable<AbstractStream> OutgoingStreams
        {
            get
            {
                return m_outgoingStreams;
            }
        }

        /// <summary>
        /// All process units must support parsing from XML
        /// </summary>
        /// <param name="xpu"></param>
        /// <returns></returns>
        //GenericProcessUnit FromXml(XElement xpu, GenericProcessUnit targetUnit);

        /// <summary>
        /// String of the form "#AARRGGBB" that represents the subprocess color. The default value is 
        /// "#FFffFFff" which is white and indicates that there is no subprocess.
        /// </summary>
        public string Subprocess
        {
            get
            {
                return m_subprocess;
            }
            set
            {
                // Make sure it's not null
                if (null == value)
                {
                    throw new ArgumentNullException(
                        "Subprocess string for a process unit cannot be null");
                }

                // Do a quick check to make sure it's a valid color string
                if (9 != value.Length)
                {
                    throw new ArgumentException(
                        "Subprocess value must be a color string of the form \"#AARRGGBB\"");
                }
                
                if (m_subprocess.Equals(value))
                {
                    // No change
                    return;
                }

                // Store the new value
                m_subprocess = value;

                // Invoke the property-changed event if non-null
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Subprocess"));
                }
            }
        }

        public string UIDString
        {
            get
            {
                return "GPU_" + m_uid.ToString();
            }
        }

        /// <summary>
        /// String for the identifier that gets written to the XML file. This string identifies 
        /// the type of process unit.
        /// </summary>
        public abstract string UnitTypeString
        {
            get;
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            // Write the process unit's unique ID number. The old file format wrote 
            // this as an attribute string that was of the form "GPU_#", so we have 
            // to stick with that to maintain compatibility.
            writer.WriteAttributeString("Id", "GPU_" + m_uid.ToString());

            // Write a string that identifies the type of process unit
            writer.WriteAttributeString("ProcessUnitType", UnitTypeString);

            // Write the "Name" (label)
            writer.WriteAttributeString("Name", m_label);

            // Write the location values
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", m_location.X.ToString());
            writer.WriteElementString("Y", m_location.Y.ToString());
            writer.WriteEndElement();

            // Write the subprocess string
            writer.WriteStartElement("Subgroup");
            writer.WriteAttributeString("Color", Subprocess);
            writer.WriteEndElement();

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
        }

        #region Events

        /// <summary>
        /// Invoked when the collection of incoming or outgoing streams changes
        /// </summary>
        public event EventHandler StreamsChanged = delegate { };
        
        public event PropertyChangedEventHandler PropertyChanged = null;

        #endregion
    }
}