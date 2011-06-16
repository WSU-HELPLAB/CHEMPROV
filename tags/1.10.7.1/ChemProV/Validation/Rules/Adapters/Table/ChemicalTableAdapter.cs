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
    public class ChemicalTableAdapter : ITableAdapter
    {
        private ChemicalStreamPropertiesTable table;

        public IPropertiesTable Table
        {
            get { return table as IPropertiesTable; }
        }

        public ChemicalTableAdapter(ChemicalStreamPropertiesTable itable)
        {
            table = itable;
        }

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

        public int GetRowCount()
        {
            return table.ItemSource.Count - 1;
        }


        public string GetCompoundAtRow(int row)
        {
            if (table.ItemSource[row].Enabled)
            {
                return new CompoundFormatter().ConvertFromIntToString(table.ItemSource[row].Compound);
            }
            else
            {
                return null;
            }
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
    }
}
