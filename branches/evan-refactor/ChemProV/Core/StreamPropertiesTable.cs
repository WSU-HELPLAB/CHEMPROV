/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.MathCore;
using System.ComponentModel;

namespace ChemProV.Core
{
    public class StreamPropertiesTable : INotifyPropertyChanged
    {
        private Vector m_location = new Vector();

        /// <summary>
        /// Reference to the parent stream
        /// </summary>
        private AbstractStream m_parent;

        private ObservableCollection<IStreamData> m_rows = new ObservableCollection<IStreamData>();

        /// <summary>
        /// Only used for chemical stream properties tables
        /// </summary>
        private string m_temperature = string.Empty;

        /// <summary>
        /// To maintain compatibility with the file format, I'm keeping this an integer value
        ///  0 = celsius
        ///  1 = fahrenheit
        /// </summary>
        private int m_temperatureUnits = 0;

        /// <summary>
        /// The design choice was to make one class that handles both chemical and heat streams as 
        /// opposed to making an abstract base class and two separate inherting classes. We use 
        /// this value to keep track of what type of stream properties table we have.
        /// </summary>
        private StreamType m_type;

        public StreamPropertiesTable(AbstractStream parentStream)
        {
            m_type = (parentStream is Core.HeatStream) ? StreamType.Heat : StreamType.Chemical;

            // Keep a reference to the parent stream
            m_parent = parentStream;

            if (StreamType.Chemical == m_type)
            {
                m_temperature = "TM" + parentStream.Id.ToString();
            }

            // Add a default row for heat streams
            if (StreamType.Heat == m_type)
            {
                AddNewRow();
            }
        }

        public StreamPropertiesTable(XElement loadFromMe, AbstractStream parentStream)
        {
            // Keep a reference to the parent stream
            m_parent = parentStream;
            
            if (loadFromMe.Name.LocalName.Equals("ChemicalStreamPropertiesWindow"))
            {
                // Set the type
                m_type = StreamType.Chemical;

                // Load the data rows
                XElement dataRowsEl = loadFromMe.Element("DataRows");
                if (null == dataRowsEl)
                {
                    throw new Exception("Chemical stream properties XML is missing \"DataRows\" element.");
                }

                // Load each <ChemicalStreamData>
                foreach (XElement csdEl in dataRowsEl.Elements("ChemicalStreamData"))
                {
                    AddRow(new ChemicalStreamData(csdEl));
                }

                // Load the location
                XElement locEl = loadFromMe.Element("Location");
                m_location.X = Convert.ToDouble(locEl.Element("X").Value);
                m_location.Y = Convert.ToDouble(locEl.Element("Y").Value);

                // Look for <Temperature> node
                XElement temperatureEl = loadFromMe.Element("Temperature");
                if (null != temperatureEl)
                {
                    m_temperature = temperatureEl.Element("Quantity").Value;
                    m_temperatureUnits = Convert.ToInt32(temperatureEl.Element("Units").Value);
                }
            }
            else if (loadFromMe.Name.LocalName.Equals("HeatStreamPropertiesWindow"))
            {
                // Set the type
                m_type = StreamType.Heat;

                // Load the data rows
                XElement dataRowsEl = loadFromMe.Element("DataRows");
                if (null == dataRowsEl)
                {
                    throw new Exception("Chemical stream properties XML is missing \"DataRows\" element.");
                }

                // Load each <HeatStreamData>
                foreach (XElement hsdEl in dataRowsEl.Elements("HeatStreamData"))
                {
                    AddRow(new HeatStreamData(hsdEl));
                }

                // Load the location
                XElement locEl = loadFromMe.Element("Location");
                m_location.X = Convert.ToDouble(locEl.Element("X").Value);
                m_location.Y = Convert.ToDouble(locEl.Element("Y").Value);
            }
            else
            {
                throw new Exception("Unknown stream property table element: " + loadFromMe.Name);
            }
        }

        public IStreamData AddNewRow()
        {
            IStreamData newRow;
            if (StreamType.Heat == m_type)
            {
                newRow = new HeatStreamData();
            }
            else
            {
                newRow = new ChemicalStreamData();
            }

            // Add the new row to the list
            AddRow(newRow);

            // Return the new row
            return newRow;
        }

        protected bool AddRow(IStreamData newRow)
        {
            // Deny the add if the exact same object (reference comparison) exists in the collection
            foreach (IStreamData row in m_rows)
            {
                if (object.ReferenceEquals(row, newRow))
                {
                    return false;
                }
            }

            // Add it to the collection
            m_rows.Add(newRow);

            // Hook up the event listener
            newRow.PropertyChanged += new PropertyChangedEventHandler(AnyRow_PropertyChanged);

            RowsChanged(this, EventArgs.Empty);

            return true;
        }

        private void AnyRow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RowPropertyChanged(sender, e);
        }

        public bool CanAddRemoveRows
        {
            get
            {
                return StreamType.Chemical == m_type;
            }
        }

        public Vector Location
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

                m_location = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Location"));
            }
        }

        public void RemoveRow(IStreamData row)
        {
            if (m_rows.Contains(row))
            {
                m_rows.Remove(row);

                // Unsubscribe from events
                row.PropertyChanged -= this.AnyRow_PropertyChanged;
                
                RowsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the number of rows of data in the table
        /// </summary>
        public int RowCount
        {
            get
            {
                return m_rows.Count;
            }
        }

        /// <summary>
        /// Gets the collection of data rows in the properties table
        /// </summary>
        public ReadOnlyCollection<IStreamData> Rows
        {
            get
            {
                return new ReadOnlyCollection<IStreamData>(m_rows);
            }
        }

        /// <summary>
        /// Gets a reference to the parent stream
        /// </summary>
        public AbstractStream Stream
        {
            get
            {
                return m_parent;
            }
        }

        public StreamType StreamType
        {
            get
            {
                return m_type;
            }
        }

        public string Temperature
        {
            get
            {
                return m_temperature;
            }
            set
            {
                if (m_temperature == value)
                {
                    // No change
                    return;
                }

                m_temperature = (null == value) ? string.Empty : value;

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Temperature"));
                }
            }
        }

        public string TemperatureUnits
        {
            get
            {
                return (0 == m_temperatureUnits) ? "celsius" : "fahrenheit";
            }
            set
            {
                int intVal = value.Equals("fahrenheit") ? 1 : 0;
                if (m_temperatureUnits == intVal)
                {
                    // No change
                    return;
                }

                m_temperatureUnits = intVal;
                PropertyChanged(this, new PropertyChangedEventArgs("TemperatureUnits"));
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer, string parentStreamIdentifier)
        {
            if (StreamType.Heat == m_type)
            {
                writer.WriteStartElement("HeatStreamPropertiesWindow");
            }
            else
            {
                writer.WriteStartElement("ChemicalStreamPropertiesWindow");
            }
            
            // Write the parent stream ID
            writer.WriteElementString("ParentStream", parentStreamIdentifier);

            // Write the data rows
            writer.WriteStartElement("DataRows");
            foreach (IStreamData row in Rows)
            {
                row.WriteXml(writer);
            }
            writer.WriteEndElement();

            // Write the location
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", m_location.X.ToString());
            writer.WriteElementString("Y", m_location.Y.ToString());
            writer.WriteEndElement();

            if (StreamType.Chemical == m_type)
            {
                // Write temperature stuff
                writer.WriteStartElement("Temperature");
                writer.WriteElementString("Quantity", m_temperature);
                writer.WriteElementString("Units", m_temperatureUnits.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Fired when any row in the table has any property changed. While you can monitor rows 
        /// individually, it is recommended that you use this event so as to avoid the pain of 
        /// subscribing and unsubscribing to each row as they are added and removed.
        /// The sender (first parameter) when this event is fired is the row object that changed, 
        /// not this table object.
        /// </summary>
        public event PropertyChangedEventHandler RowPropertyChanged = delegate { };

        public event EventHandler RowsChanged = delegate { };

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
