/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.ComponentModel;
using System.Windows.Media;

using System.Linq;
using System.Xml.Linq;

namespace ChemProV.Core
{
    /// <summary>
    /// Object representation of the data present in the PropertiesWindow for
    /// chemical streams.
    /// </summary>
    public class HeatStreamData : IStreamData, INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        
        private string label = string.Empty;
        private string quantity = string.Empty;
        private string feedback = string.Empty;
        private string toolTipMessage = string.Empty;

        private string m_selectedUnits = string.Empty;

        private static string[] s_energyUnits = { "BTU", "BTU/sec", "J", "W" };

        public HeatStreamData() { }

        public HeatStreamData(XElement loadFromMe)
        {
            if (!loadFromMe.Name.LocalName.Equals("HeatStreamData"))
            {
                throw new InvalidOperationException();
            }

            label = loadFromMe.Element("Label").Value;
            quantity = loadFromMe.Element("Quantity").Value;

            XElement selectedUnitsEl = loadFromMe.Element("SelectedUnits");
            if (null == selectedUnitsEl)
            {
                // This most likely means that we're loading a file that was created with 
                // an older version of the application. The older version wrote a <Units> 
                // element that had an integer value, representing the enumerated type 
                // ChemicalUnits.
                int i = Convert.ToInt32(loadFromMe.Element("Units").Value);
                if (Enum.IsDefined(typeof(ChemicalUnits), i))
                {
                    m_selectedUnits = ((ChemicalUnits)i).ToPrettyString();
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

        public object GetColumnUIObject(int columnIndex, out string propertyName)
        {
            switch (columnIndex)
            {
                case 0:
                    // Label column (string field)
                    propertyName = "Label";
                    return label;

                case 1:
                    // Quantity column (string field)
                    propertyName = "Quantity";
                    return quantity;

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
                return label;
            }
            set
            {
                label = value;
                CheckIfEnabled("Label");
            }
        }

        public string Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                quantity = value;
                CheckIfEnabled("Quantity");
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

        private void CheckIfEnabled(string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
            set
            {
            }
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

        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return label;

                    case 1:
                        return quantity;

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
                        label = value as string;
                        break;

                    case 1:
                        quantity = value as string;
                        break;

                    case 2:
                        m_selectedUnits = value as string;
                        break;
                }
            }
        }

        public bool UserHasRenamed
        {
            get;
            set;
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