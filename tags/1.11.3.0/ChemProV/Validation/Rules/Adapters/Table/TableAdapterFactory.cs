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

using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;

namespace ChemProV.Validation.Rules.Adapters.Table
{
    /// <summary>
    /// This class creates a TablesAdapter based upon the table passed in.
    /// </summary>
    public class TableAdapterFactory
    {
        /// <summary>
        /// This creates a TableAdapter for the table passed.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ITableAdapter CreateTableAdapter(IPropertiesWindow table)
        {
            if (table is ChemicalStreamPropertiesWindow)
            {
                return new ChemicalTableAdapter(table as ChemicalStreamPropertiesWindow);
            }
            else if (table is HeatStreamPropertiesWindow)
            {
                return new HeatTableAdapter(table as HeatStreamPropertiesWindow);
            }
            return new NullTableAdapter();
        }
        
    }
}
