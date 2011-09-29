/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    /// <summary>
    /// This is the interface for all TableAdapters
    /// </summary>
    public interface ITableAdapter
    {
        /// <summary>
        /// the table which the TableAdapter is the adapting too
        /// </summary>
        IPropertiesWindow Table
        {
            get;
        }

        /// <summary>
        /// This returns the value for the Unit Column at the specificed row
        /// </summary>
        /// <param name="row">the row from which data will been taken from</param>
        /// <returns>returns the value, or if the row is not enabled then returns "", or returns "?" if wildcard</returns>
        string GetUnitAtRow(int row);

        /// <summary>
        /// This returns the value for the Quantity Column at the specificed row
        /// </summary>
        /// <param name="row">the row from which data will been taken from</param>
        /// <returns>returns the value, or if the row is not enabled then returns "", or returns "?" if wildcard</returns>
        string GetQuantityAtRow(int row);

        /// <summary>
        /// This returns the value for the Compound Column at the specificed row
        /// </summary>
        /// <param name="row">the row from which data will been taken from</param>
        /// <returns>returns the value, or if the row is not enabled then returns "", or returns "?" if wildcard</returns>
        string GetCompoundAtRow(int row);

        /// <summary>
        /// This returns the value for the Label Column at the specificed row
        /// </summary>
        /// <param name="row">the row from which data will been taken from</param>
        /// <returns>returns the value, or if the row is not enabled then returns "", or returns "?" if wildcard</returns>
        string GetLabelAtRow(int row);

        /// <summary>
        /// This returns the value for the Temperature Column at the specificed row
        /// </summary>
        /// <returns>returns the value, or if the row is not enabled then returns "", or returns "?" if wildcard</returns>
        string GetTemperature();

        /// <summary>
        /// This returns the value for the Temperature Units at the specificed row
        /// </summary>
        /// <param name="row">the row from which data will been taken from</param>
        /// <returns>returns the value, or if the row is not enabled then returns "", or returns "?" if wildcard</returns>
        string GetTemperatureUnits();

        double GetActuallQuantityAtRow(int row);

        /// <summary>
        /// This returns the number of rows in the table minus one becase the last row is never enabled
        /// </summary>
        /// <returns></returns>
        int GetRowCount();

        TableType GetTableType();
    }
}