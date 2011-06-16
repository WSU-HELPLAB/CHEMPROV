using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ChemProV.PFD.Streams.PropertiesTable.Chemical
{
    /// <summary>
    /// Object representation of the data present in the PropertiesTable for
    /// chemical streams.
    /// </summary>
    public class ChemicalStreamData : INotifyPropertyChanged
    {
        private string label = "";
        private string quantity = "?";
        private int units = 0;
        private int compound = 0;
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
        public int Units 
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
        public int Compound 
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
            if (Compound != 0)
            {
                Enabled = true;
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
    }
}
