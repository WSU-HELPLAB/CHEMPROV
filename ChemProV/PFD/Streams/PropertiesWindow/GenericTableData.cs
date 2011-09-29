/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public enum TableType
    {
        Chemcial,
        Heat
    }

    public class GenericTableData
    {
        private IPropertiesWindow parent;

        public IPropertiesWindow Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        private TableType tabletype;

        public TableType Tabletype
        {
            get { return tabletype; }
        }

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

        string temperature;

        public string Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public GenericTableData(IPropertiesWindow parent, TableType tabletype, string label, string units, string quantity, string compound, string temp)
        {
            this.parent = parent;
            this.tabletype = tabletype;
            this.label = label;
            this.units = units;
            this.quantity = quantity;
            this.compound = compound;
            this.temperature = temp;
        }
    }
}