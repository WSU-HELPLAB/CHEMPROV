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

namespace ChemProV.Validation.Rules.Adapters.Table
{
    public class NullTableAdapter : ITableAdapter
    {

        public PFD.Streams.PropertiesTable.IPropertiesTable Table
        {
            get { return null ; }
        }

        public string GetUnitAtRow(int row)
        {
           return "";
        }

        public string GetQuantityAtRow(int row)
        {
            return "";
        }

        public string GetCompoundAtRow(int row)
        {
            return "";
        }

        public string GetLabelAtRow(int row)
        {
            return "";
        }

        public int GetRowCount()
        {
            return 0;
        }
    }
}
