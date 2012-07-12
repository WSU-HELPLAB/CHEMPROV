/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace ChemProV.Logic
{
    /// <summary>
    /// Object representation of a data row in the properties table for chemical streams.
    /// </summary>
    public class ChemicalStreamData : IStreamData, INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        
        private string label = "";
        private string quantity = string.Empty;
        private string feedback = string.Empty;
        private string toolTipMessage = "";
        

        private string m_compound = string.Empty;

        private ObservableCollection<string> m_compoundOptions = new ObservableCollection<string>();

        private string m_selectedUnits = string.Empty;

        private ObservableCollection<string> m_unitOptions = new ObservableCollection<string>();

        private bool m_userHasRenamed = false;

        public ChemicalStreamData()
        {
            foreach (string unit in ChemicalUnitOptions.ShortNames)
            {
                m_unitOptions.Add(unit);
            }

            // Create the list of compound options. We want the first to be "Overall" and then the 
            // rest should be from the ChemicalCompounds enumeration.
            m_compoundOptions.Add("Overall");
            //foreach (ChemicalCompounds compound in Enum.GetValues(typeof(ChemicalCompounds)))
            foreach (string compound in ChemicalCompoundOptions.All)
            {
                m_compoundOptions.Add(compound);
            }
        }

        /// <summary>
        /// Constructs from a ChemicalStreamData XML node
        /// </summary>
        public ChemicalStreamData(XElement loadFromMe)
            : this()
        {
            if (!loadFromMe.Name.LocalName.Equals("ChemicalStreamData"))
            {
                throw new InvalidOperationException();
            }

            label = loadFromMe.Element("Label").Value;
            quantity = loadFromMe.Element("Quantity").Value;

            // Check for element that is only in older files, but needs to be loaded if present 
            // to keep compatibility with previous versions
            XElement oldFormatCompoundIdEl = loadFromMe.Element("CompoundId");
            if (null != oldFormatCompoundIdEl)
            {
                // The file format was unfortunately designed such that an integer is stored for the 
                // compound identifier. The data structure code was updated to use a string for the 
                // selected compound and saving code will write the string and not the integer. 
                // However, we still need to support the old format so we need to convert:
                //  int -> Enum -> string
                int SelectedCompoundId = Convert.ToInt32(oldFormatCompoundIdEl.Value);
                if (Enum.IsDefined(typeof(ChemicalCompounds), (byte)SelectedCompoundId))
                {
                    m_compound = ChemicalCompoundsFormatter.ToPrettyString((ChemicalCompounds)SelectedCompoundId);
                }
            }
            else
            {
                // Expecting the newer file format if we come here. In this case there should be 
                // a SelectedCompound element.
                m_compound = loadFromMe.Element("SelectedCompound").Value;
            }

            XElement suEl = loadFromMe.Element("SelectedUnits");
            if (null != suEl)
            {
                m_selectedUnits = suEl.Value;
            }
            else
            {
                // Check for item that was used in older versions
                XElement uidEl = loadFromMe.Element("UnitId");
                int unitEnumInt = Convert.ToInt32(uidEl.Value);
                if (unitEnumInt >=0 && unitEnumInt < ChemicalUnitOptions.ShortNames.Length)
                {
                    m_selectedUnits = ChemicalUnitOptions.ShortNames[unitEnumInt];
                }
            }

            XElement uhr = loadFromMe.Element("UserHasRenamed");
            if (null != uhr)
            {
                m_userHasRenamed = bool.Parse(uhr.Value);
            }
        }

        public object GetColumnUIObject(int columnIndex, out string propertyName)
        {
            switch (columnIndex)
            {
                case 0:
                    propertyName = "SelectedCompound";
                    return m_compoundOptions;

                case 1:
                    propertyName = "Label";
                    return label;

                case 2:
                    propertyName = "Quantity";
                    return quantity;

                case 3:
                    propertyName = "SelectedUnits";
                    return m_unitOptions;

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
                if (label == value)
                {
                    // No change
                    return;
                }
                
                label = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Label"));
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
                if (quantity == value)
                {
                    // No change
                    return;
                }
                
                quantity = value;
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

        public ObservableCollection<string> Units
        {
            get
            {
                return m_unitOptions;
            }
        }

        public ObservableCollection<string> Compounds
        {
            get
            {
                return m_compoundOptions;
            }
        }

        /// <summary>
        /// Gets or sets the currently selected compound. This should be one of the values 
        /// from the "Compounds" collection.
        /// </summary>
        public string SelectedCompound
        {
            get
            {
                return m_compound;
            }
            set
            {
                if (m_compound == value)
                {
                    // No change
                    return;
                }

                m_compound = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedCompound"));
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

        public int CompareTo(object obj)
        {
            if (!(obj is ChemicalStreamData))
            {
                return -1;
            }
            ChemicalStreamData other = obj as ChemicalStreamData;
            return this.Label.CompareTo(other.Label);
        }

        /// <summary>
        /// Allows getting and setting of cells in the data row
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return m_compound;

                    case 1:
                        return label;

                    case 2:
                        return quantity;

                    case 3:
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
                        SelectedCompound = value as string;
                        break;

                    case 1:
                        Label = value as string;
                        break;

                    case 2:
                        Quantity = value as string;
                        break;

                    case 3:
                        SelectedUnits = value as string;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether or not the user has manually 
        /// changed the label for this row. This defaults to false and must be set to 
        /// true by the UI layer when the user renames the row.
        /// </summary>
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

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ChemicalStreamData");
            
            writer.WriteStartElement("Label");
            writer.WriteString(Label);
            writer.WriteEndElement();

            writer.WriteStartElement("Quantity");
            writer.WriteString(Quantity);
            writer.WriteEndElement();

            // The old file format wrote integers for elements CompoundId and UnitId. We want to 
            // get rid of this but I'll comment it out instead of deleting it entirely while I 
            // get the new file format in place and do tests.
            //writer.WriteStartElement("UnitId");
            //writer.WriteString(SelectedUnitId.ToString());
            //writer.WriteEndElement();
            //writer.WriteStartElement("CompoundId");
            //writer.WriteString(SelectedCompoundId.ToString());
            //writer.WriteEndElement();

            writer.WriteStartElement("SelectedUnits");
            writer.WriteString(m_selectedUnits);
            writer.WriteEndElement();

            writer.WriteStartElement("SelectedCompound");
            writer.WriteString(m_compound);
            writer.WriteEndElement();

            writer.WriteStartElement("Feedback");
            writer.WriteString(Feedback);
            writer.WriteEndElement();

            writer.WriteStartElement("ToolTipMessage");
            writer.WriteString(ToolTipMessage);
            writer.WriteEndElement();

            writer.WriteElementString("UserHasRenamed", m_userHasRenamed.ToString());

            // End "ChemicalStreamData"
            writer.WriteEndElement();
        }
    }
}