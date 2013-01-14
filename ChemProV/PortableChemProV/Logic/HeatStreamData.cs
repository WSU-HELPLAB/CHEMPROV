/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace ChemProV.Logic
{
    /// <summary>
    /// Object representation of the data present in the PropertiesWindow for
    /// chemical streams.
    /// </summary>
    public class HeatStreamData : IStreamDataRow, INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        
        private string m_quantity = string.Empty;
        private string feedback = string.Empty;
        private string toolTipMessage = string.Empty;

        private string m_label = string.Empty;

        private string m_selectedUnits = string.Empty;

        private bool m_userHasRenamed = false;

        private static readonly string[] s_energyUnits = { "BTU", "BTU/sec", "J", "W" };

        public HeatStreamData() { }

        public HeatStreamData(XElement loadFromMe)
        {
            if (!loadFromMe.Name.LocalName.Equals("HeatStreamData"))
            {
                throw new InvalidOperationException();
            }

            m_label = loadFromMe.Element("Label").Value;
            m_quantity = loadFromMe.Element("Quantity").Value;

            XElement selectedUnitsEl = loadFromMe.Element("SelectedUnits");
            if (null == selectedUnitsEl)
            {
                // This most likely means that we're loading a file that was created with 
                // an older version of the application. The older version wrote a <Units> 
                // element that had an integer value, representing an index into the 
                // energy units array.
                int i = Convert.ToInt32(loadFromMe.Element("Units").Value);
                if (i >= 0 && s_energyUnits.Length > i)
                {
                    m_selectedUnits = s_energyUnits[i];
                }
            }
            else
            {
                m_selectedUnits = selectedUnitsEl.Value;
            }

            // Load <Feedback> and <ToolTipMessage> (not sure if we need these)
            feedback = loadFromMe.Element("Feedback").Value;
            toolTipMessage = loadFromMe.Element("ToolTipMessage").Value;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is HeatStreamData))
            {
                return -1;
            }
            HeatStreamData other = obj as HeatStreamData;
            return this.Label.CompareTo(other.Label);
        }

        public static string[] EnergyUnitOptions
        {
            get
            {
                return s_energyUnits;
            }
        }

        public string Feedback
        {
            get
            {
                return feedback;
            }
            set
            {
                feedback = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Feedback"));
            }
        }

        public object GetColumnUIObject(int columnIndex, out string propertyName)
        {
            switch (columnIndex)
            {
                case 0:
                    // Label column (string field)
                    propertyName = "Label";
                    return m_label;

                case 1:
                    // Quantity column (string field)
                    propertyName = "Quantity";
                    return m_quantity;

                case 2:
                    // Energy units column (string collection field)
                    propertyName = "SelectedUnits";
                    return s_energyUnits;

                default:
                    propertyName = string.Empty;
                    return null;
            }
        }

        public string Label
        {
            get
            {
                return m_label;
            }
            set
            {
                if (m_label == value)
                {
                    // No change
                    return;
                }
                
                m_label = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Label"));
            }
        }

        public string Quantity
        {
            get
            {
                return m_quantity;
            }
            set
            {
                if (value == m_quantity)
                {
                    // No change
                    return;
                }
                
                m_quantity = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Quantity"));
            }
        }

        public string SelectedUnits
        {
            get
            {
                return m_selectedUnits;
            }
            set
            {
                if (m_selectedUnits == value)
                {
                    // No change
                    return;
                }

                m_selectedUnits = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedUnits"));
            }
        }

        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return m_label;

                    case 1:
                        return m_quantity;

                    case 2:
                        return m_selectedUnits;

                    default:
                        return null;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Label = value as string;
                        break;

                    case 1:
                        Quantity = value as string;
                        break;

                    case 2:
                        SelectedUnits = value as string;
                        break;
                }
            }
        }

        public string ToolTipMessage
        {
            get
            {
                return toolTipMessage;
            }
            set
            {
                toolTipMessage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ToolTipMessage"));
            }
        }

        public bool UserHasRenamed
        {
            get
            {
                return m_userHasRenamed;
            }
            set
            {
                if (m_userHasRenamed == value)
                {
                    // No change
                    return;
                }

                m_userHasRenamed = value;
                PropertyChanged(this, new PropertyChangedEventArgs("UserHasRenamed"));
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("HeatStreamData");
            
            writer.WriteStartElement("Label");
            writer.WriteString(Label);
            writer.WriteEndElement();

            writer.WriteStartElement("Quantity");
            writer.WriteString(Quantity);
            writer.WriteEndElement();

            writer.WriteStartElement("SelectedUnits");
            writer.WriteString(m_selectedUnits);
            writer.WriteEndElement();

            writer.WriteStartElement("Feedback");
            writer.WriteString(Feedback);
            writer.WriteEndElement();

            writer.WriteStartElement("ToolTipMessage");
            writer.WriteString(ToolTipMessage);
            writer.WriteEndElement();

            // End "HeatStreamData"
            writer.WriteEndElement();
        }
    }
}