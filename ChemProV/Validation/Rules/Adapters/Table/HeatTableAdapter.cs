/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    public class HeatTableAdapter : ITableAdapter
    {
        private HeatStreamPropertiesWindow table;

        public IPropertiesWindow Table
        {
            get { return table as IPropertiesWindow; }
        }

        public HeatTableAdapter(HeatStreamPropertiesWindow itable)
        {
            table = itable;
        }

        public string GetUnitAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                string units = new UnitsFormatter().ConvertFromIntToString(table.ItemSource[row].Units);
                return units;
            }
            else
            {
                return null;
            }
        }

        public string GetQuantityAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return (table.ItemSource[row].Quantity);
            }
            else
            {
                return null;
            }
        }

        public string GetCompoundAtRow(int row)
        {
            return "";
        }

        public string GetLabelAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return table.ItemSource[row].Label;
            }
            else
            {
                return null;
            }
        }

        public int GetRowCount()
        {
            return 1;
        }

        public string GetTemperature()
        {
            return "";
        }

        public string GetTemperatureUnits()
        {
            return "";
        }

        public double GetActuallQuantityAtRow(int row)
        {
            try
            {
                return double.Parse(GetQuantityAtRow(row));
            }
            catch
            {
                return double.NaN;
            }
        }

        public TableType GetTableType()
        {
            return TableType.Heat;
        }
    }
}