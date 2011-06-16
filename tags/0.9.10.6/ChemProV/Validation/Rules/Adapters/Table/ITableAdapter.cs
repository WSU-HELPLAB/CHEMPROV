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
    public interface ITableAdapter
    {
        IPropertiesTable Table
        {
            get;
        }
        string GetUnitAtRow(int row);
        string GetQuantityAtRow(int row);
        string GetCompoundAtRow(int row);
        string GetLabelAtRow(int row);

        int GetRowCount();
    }
}
