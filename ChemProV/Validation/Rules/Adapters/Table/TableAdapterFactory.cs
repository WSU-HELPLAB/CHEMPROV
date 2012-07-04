/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;

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
        public static ITableAdapter CreateTableAdapter(Core.StreamPropertiesTable table)
        {
            if (StreamType.Chemical == table.StreamType)
            {
                return new ChemicalTableAdapter(table);
            }
            else if (StreamType.Heat == table.StreamType)
            {
                return new HeatTableAdapter(table);
            }
            return new NullTableAdapter();
        }
    }
}