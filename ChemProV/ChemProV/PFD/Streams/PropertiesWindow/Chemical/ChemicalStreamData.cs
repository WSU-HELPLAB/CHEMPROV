/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    /// <summary>
    /// Object representation of the data present in the PropertiesWindow for
    /// chemical streams.
    /// </summary>
    public class ChemicalStreamData : IStreamData, INotifyPropertyChanged, IComparable
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
    }
}