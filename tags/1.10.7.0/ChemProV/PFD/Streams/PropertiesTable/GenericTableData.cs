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

namespace ChemProV.PFD.Streams.PropertiesTable
{
    public class GenericTableData
    {
        string label;

        public string Label
        {
            get { return label; }
        }
        string units;

        public string Units
        {
            get { return units; }
        }
        string quantity;

        public string Quantity
        {
            get { return quantity; }
        }
        string compound;

        public string Compound
        {
            get { return compound; }
        }

        public GenericTableData(string label, string units, string quantity, string compound)
        {
            this.label = label;
            this.units = units;
            this.quantity = quantity;
            this.compound = compound;
        }
    }

}
