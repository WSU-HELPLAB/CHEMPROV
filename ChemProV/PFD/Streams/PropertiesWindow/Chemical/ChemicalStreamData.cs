/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

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
        private int selectedUnit = 0;
        private int selectedCompound = 24;
        private string temperature = "?";
        private int tempUnits = 0;
        private string feedback = "";
        private string toolTipMessage = "";
        private ObservableCollection<ChemicalUnits> units;
        private ObservableCollection<ChemicalCompounds> compounds;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ChemicalStreamData()
        {
            units = new ObservableCollection<ChemicalUnits>();
            foreach (ChemicalUnits unit in Enum.GetValues(typeof(ChemicalUnits)))
            {
                units.Add(unit);
            }

            compounds = new ObservableCollection<ChemicalCompounds>();
            foreach (ChemicalCompounds compound in Enum.GetValues(typeof(ChemicalCompounds)))
            {
                compounds.Add(compound);
            }

            SelectedCompoundId = -1;
            SelectedUnitId = -1;
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

        public ChemicalUnits SelectedUnit
        {
            get
            {
                return (ChemicalUnits)SelectedUnitId;
            }
            set
            {
                SelectedUnitId = (int)value;
                CheckIfEnabled("SelectedUnit");
            }
        }

        public ObservableCollection<ChemicalUnits> Units
        {
            get
            {
                return units;
            }
        }

        public int SelectedUnitId
        {
            get
            {
                return selectedUnit;
            }
            set
            {
                selectedUnit = value;
                CheckIfEnabled("UnitId");
            }
        }

        public ObservableCollection<ChemicalCompounds> Compounds
        {
            get
            {
                return compounds;
            }
        }

        public ChemicalCompounds SelectedCompound
        {
            get
            {
                return (ChemicalCompounds)SelectedCompoundId;
            }
            set
            {
                SelectedCompoundId = (int)value;
                CheckIfEnabled("SelectedCompound");
            }
        }

        public int SelectedCompoundId
        {
            get
            {
                return selectedCompound;
            }
            set
            {
                selectedCompound = value;
                CheckIfEnabled("CompoundId");
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
            if (SelectedCompoundId != 24)
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
            writer.WriteString(SelectedUnitId.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("CompoundId");
            writer.WriteString(SelectedCompoundId.ToString());
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

        #endregion IXmlSerializable Members
    }
}