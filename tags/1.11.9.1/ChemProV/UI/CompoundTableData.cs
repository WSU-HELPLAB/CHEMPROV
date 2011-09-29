/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.UI
{
    public class CompoundTableData
    {
        private string elementName;

        public string ElementName
        {
            get { return elementName; }
        }

        private double quantity;

        public double Quantity
        {
            get { return quantity; }
        }

        public CompoundTableData(string elementName, double quantity)
        {
            this.elementName = elementName;
            this.quantity = quantity;
        }
    }
}