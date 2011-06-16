/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
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

using ChemProV.PFD.Streams.PropertiesTable.Heat;
using ChemProV.PFD.Streams.PropertiesTable;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    public class HeatTableAdapter : ITableAdapter
    {
       
        private HeatStreamPropertiesTable table;

        public IPropertiesTable Table
        {
            get { return table as IPropertiesTable; }
        }

        public HeatTableAdapter(HeatStreamPropertiesTable itable)
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
