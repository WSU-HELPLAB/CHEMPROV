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
    /// This is the NullTableAdapter the idea is to do nothing without breaking anything
    /// </summary>
    public class NullTableAdapter : ITableAdapter
    {
        /// <summary>
        /// This always return null and is not setable
        /// </summary>
        public IPropertiesWindow Table
        {
            get { return null; }
        }

        /// <summary>
        /// This always returns the string ""
        /// </summary>
        /// <param name="row">This is not used</param>
        /// <returns>Emptry string</returns>
        public string GetUnitAtRow(int row)
        {
            return "";
        }

        /// <summary>
        /// This always returns the string ""
        /// </summary>
        /// <param name="row">This is not used</param>
        /// <returns>Emptry string</returns>
        public string GetQuantityAtRow(int row)
        {
            return "";
        }

        /// <summary>
        /// This always returns the string ""
        /// </summary>
        /// <param name="row">This is not used</param>
        /// <returns>Emptry string</returns>
        public string GetCompoundAtRow(int row)
        {
            return "";
        }

        /// <summary>
        /// This always returns the string ""
        /// </summary>
        /// <param name="row">This is not used</param>
        /// <returns>Emptry string</returns>
        public string GetLabelAtRow(int row)
        {
            return "";
        }

        /// <summary>
        /// This always returns 0
        /// </summary>
        /// <returns></returns>
        public int GetRowCount()
        {
            return 0;
        }

        /// <summary>
        /// This always returns ""
        /// </summary>
        /// <param name="row">This is not used</param>
        /// <returns>Emptry string</returns>
        public string GetTemperature()
        {
            return "";
        }

        /// <summary>
        /// This always retruns ""
        /// </summary>
        /// <param name="row">This is not used</param>
        /// <returns>Emptry string</returns>
        public string GetTemperatureUnits()
        {
            return "";
        }

        public double GetActuallQuantityAtRow(int row)
        {
            return 0;
        }

        public TableType GetTableType()
        {
            //chemical is default
            return TableType.Chemcial;
        }
    }
}