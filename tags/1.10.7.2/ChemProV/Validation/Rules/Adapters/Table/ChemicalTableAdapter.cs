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

using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.PFD.Streams.PropertiesTable;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    /// <summary>
    /// This TableAdapter gets the infomation from a Chemical Table
    /// </summary>
    public class ChemicalTableAdapter : ITableAdapter
    {
        private ChemicalStreamPropertiesTable table;

        /// <summary>
        /// This is the table that this adapter is getting the data from
        /// </summary>
        public IPropertiesTable Table
        {
            get { return table as IPropertiesTable; }
        }

        /// <summary>
        /// This is the contsructor
        /// </summary>
        /// <param name="itable">This is the table we want data from</param>
        public ChemicalTableAdapter(ChemicalStreamPropertiesTable itable)
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
                string units = new UnitsFormatter().ConvertFromIntToString(table.ItemSource[row].Units);
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
            return table.ItemSource.Count - 1;
        }

        /// <summary>
        /// This gets the value of the Compound Row at the specificed row
        /// </summary>
        /// <param name="row">must be less than count</param>
        /// <returns>returns empty string if row is not enabled</returns>
        public string GetCompoundAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return new CompoundFormatter().ConvertFromIntToString(table.ItemSource[row].Compound);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// This gets the value of the Compound Row at the specificed row
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
    }
}
