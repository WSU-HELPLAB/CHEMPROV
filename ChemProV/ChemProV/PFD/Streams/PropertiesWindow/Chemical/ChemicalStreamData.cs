/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    /// <summary>
    /// Object representation of the data present in the PropertiesWindow for
    /// chemical streams.
    /// </summary>
    public class ChemicalStreamData : IStreamData, INotifyPropertyChanged, IComparable, IXmlSerializable
    {
        private string label = "";
        private string quantity = "?";
        private int units = 0;
        private int compound = 24;
        private string temperature = "?";
        private int tempUnits = 0;
        private string feedback = "";
        private string toolTipMessage = "";
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

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

        public ChemicalUnits Unit
        {
            get
            {
                return (ChemicalUnits)UnitId;
            }
            set
            {
                UnitId = (int)value;
            }
        }

        public int UnitId
        {
            get
            {
                return units;
            }
            set
            {
                units = value;
                CheckIfEnabled("Units");
            }
        }

        public ChemicalCompounds Compound
        {
            get
            {
                return (ChemicalCompounds)CompoundId;
            }
            set
            {
                CompoundId = (int)value;
            }
        }

        public int CompoundId
        {
            get
            {
                return compound;
            }
            set
            {
                compound = value;
                CheckIfEnabled("Compound");
            }
        }

        public string Temperature
        {
            get
            {
                return temperature;
            }
            set
            {
                temperature = value;
                CheckIfEnabled("Temperature");
            }
        }

        public int TempUnits
        {
            get
            {
                return tempUnits;
            }
            set
            {
                tempUnits = value;
                CheckIfEnabled("TempUnits");
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
            if (CompoundId != 24)
            {
                Enabled = true;
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Enabled = false;
            }
        }

        public bool Enabled
        {
            get;
            set;
        }

        public Color BackgroundColor
        {
            get
            {
                if (Enabled)
                {
                    return Colors.White;
                }
                return Colors.LightGray;
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
            writer.WriteStartElement("Label");
            writer.WriteString(Label);
            writer.WriteEndElement();

            writer.WriteStartElement("Quantity");
            writer.WriteString(Quantity);
            writer.WriteEndElement();

            writer.WriteStartElement("UnitId");
            writer.WriteString(UnitId.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("CompoundId");
            writer.WriteString(CompoundId.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Temperature");
            writer.WriteString(Temperature);
            writer.WriteEndElement();

            writer.WriteStartElement("TemperatureUnits");
            writer.WriteString(TempUnits.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Feedback");
            writer.WriteString(Feedback);
            writer.WriteEndElement();

            writer.WriteStartElement("ToolTipMessage");
            writer.WriteString(ToolTipMessage);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Creates a new StickyNote based on the supplied XML element
        /// </summary>
        /// <param name="xmlNote">The xml for a StickyNote</param>
        /// <returns></returns>
        public static ChemicalStreamData FromXml(XElement xmlNote)
        {
            /*
            StickyNote note = new StickyNote();

            //pull out content & color
            note.Note.Text = xmlNote.Element("Content").Value;
            note.ColorChange(StickyNoteColorsFromString(xmlNote.Element("Color").Value));

            //use LINQ to find us the X,Y coords
            var location = from c in xmlNote.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            note.SetValue(Canvas.LeftProperty, Convert.ToDouble(location.ElementAt(0).x));
            note.SetValue(Canvas.TopProperty, Convert.ToDouble(location.ElementAt(0).y));

            //return the processed note
            return note;
             * */
            return new ChemicalStreamData();
        }

        #endregion IXmlSerializable Members
    }
}