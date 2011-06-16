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

using ChemProV.PFD.Streams.PropertiesTable;

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
        IPropertiesTable Table
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
        /// This returns the number of rows in the table minus one becase the last row is never enabled
        /// </summary>
        /// <returns></returns>
        int GetRowCount();
    }
}
