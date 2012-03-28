/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    /// <summary>
    /// This TableAdapter gets the infomation from a Chemical Table
    /// </summary>
    public class ChemicalTableAdapter : ITableAdapter
    {
        private ChemicalStreamPropertiesWindow table;

        /// <summary>
        /// This is the table that this adapter is getting the data from
        /// </summary>
        public IPropertiesWindow Table
        {
            get { return table as IPropertiesWindow; }
        }

        /// <summary>
        /// This is the contsructor
        /// </summary>
        /// <param name="itable">This is the table we want data from</param>
        public ChemicalTableAdapter(ChemicalStreamPropertiesWindow itable)
        {
            table = itable;
        }

        /// <summary>
        /// This gets what the value of the Unit Column is at the specificed row
        /// </summary>
        /// <param name="row">Must be less than count</param>
        /// <returns>returns empty string if row is not enabled, or ? if it is % otherwise it returns the value</returns>
        public string GetUnitAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                int unitIndex = table.ItemSource[row].UnitId;
                if (row == 0)
                {
                    //the first row does not have % so the index is off by one so must account for this
                    unitIndex++;
                }
                string units = new UnitsFormatter().ConvertFromIntToString(unitIndex);
                if (units == "%")
                {
                    //QuestionMark
                    return "?";
                }
                else
                {
                    return units;
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// This gets the value of the Quantity Row at the specificed row
        /// </summary>
        /// <param name="row">must be less than count</param>
        /// <returns>returns empty string if row is not enabled</returns>
        public string GetQuantityAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return (table.ItemSource[row].Quantity);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// This gets the row count
        /// </summary>
        /// <returns>returns the rowCount - 1 because we never care about the last row</returns>
        public int GetRowCount()
        {
            return table.ItemSource.Count;
        }

        /// <summary>
        /// This gets the value of the Compound Row at the specified row
        /// </summary>
        /// <param name="row">must be less than count</param>
        /// <returns>returns empty string if row is not enabled</returns>
        public string GetCompoundAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return new CompoundFormatter().ConvertFromIntToString(table.ItemSource[row].CompoundId);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// This gets the value of the Compound Row at the specified row
        /// </summary>
        /// <param name="row">must be less than count</param>
        /// <returns>returns empty string if row is not enabled</returns>
        public string GetLabelAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return table.ItemSource[row].Label;
            }
            else
            {
                return "";
            }
        }

        public string GetTemperature()
        {
            if (table.ItemSource[0].Enabled)
            {
                return ConveretToCelsius(table.ItemSource[0].Temperature, GetTemperatureUnits());
            }
            else
            {
                return "";
            }
        }

        public string GetTemperatureUnits()
        {
            if (table.ItemSource[0].Enabled)
            {
                return new TempUnitsFormatter().ConvertFromIntToString(table.ItemSource[0].TempUnits);
            }
            else
            {
                return "";
            }
        }

        private string ConveretToCelsius(string tempStr, string tempUnits)
        {
            double temp;
            try
            {
                temp = double.Parse(tempStr);

                switch (tempUnits)
                {
                    case "celsius":
                        return temp.ToString();
                    //break; unreachable code if index have break uncommented

                    case "fahrenheit":
                        {
                            return ((temp - 32) * (5.0 / 9)).ToString();
                        }
                    //break; unreachable code if index have break uncommented
                }
            }
            catch
            {
                return tempStr;
            }
            return tempStr;
        }

        public double GetActuallQuantityAtRow(int row)
        {
            if (GetUnitAtRow(row) == "?")
            {
                //so we got a percent so gotta find the actual number.

                string overallStr = GetQuantityAtRow(0);
                string rowQuantityStr = GetQuantityAtRow(row);

                try
                {
                    double overall = double.Parse(overallStr);
                    double rowQuantity = double.Parse(rowQuantityStr);

                    return (overall * (rowQuantity / 100));
                }
                catch
                {
                    return double.NaN;
                }
            }
            else
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
        }

        public TableType GetTableType()
        {
            return TableType.Chemcial;
        }
    }
}