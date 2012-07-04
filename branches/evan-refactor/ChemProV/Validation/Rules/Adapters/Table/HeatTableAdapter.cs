/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    public class HeatTableAdapter : ITableAdapter
    {
        private Core.StreamPropertiesTable table;

        public IPropertiesWindow Table
        {
            get { return table as IPropertiesWindow; }
        }

        public HeatTableAdapter(Core.StreamPropertiesTable itable)
        {
            // Make sure it's the right kind of table
            if (StreamType.Heat != table.StreamType)
            {
                throw new ArgumentException();
            }
            
            table = itable;
        }

        public string GetUnitAtRow(int row)
        {
            return (table.Rows[row] as Core.HeatStreamData).SelectedUnits;
        }

        public string GetQuantityAtRow(int row)
        {
            return (table.Rows[row] as Core.HeatStreamData).Quantity;
        }

        public string GetCompoundAtRow(int row)
        {
            // There are no compound options for heat stream rows
            return string.Empty;
        }

        public string GetLabelAtRow(int row)
        {
            return (table.Rows[row] as Core.HeatStreamData).Label;
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