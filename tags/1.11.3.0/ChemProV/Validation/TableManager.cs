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
using System.Collections.Generic;

using ChemProV.DotNetExtensions.Collections.Generic;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.Validation.Rules;

namespace ChemProV.Validation
{
    public class TableManager
    {
        private static TableManager instance;

        public event EventHandler TableDataChanged = delegate { };

        private EquationValidationChecker equationValidationChecker = EquationValidationChecker.GetInstance();

        /// <summary>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static TableManager GetInstance()
        {
            if (instance == null)


            {
                instance = new TableManager();
            }
            return instance;
        }

        private TableManager()
        {
            tableEntries = new Dictionary<string, ChemicalStreamData>();
            equationValidationChecker.ListofTablesData = tableEntries;
        }

        /// <summary>
        /// This keeps references to all the tables.
        /// </summary>
        Dictionary<string, ChemicalStreamData> tableEntries;

        /// <summary>
        /// This is called when a new stream is created because a new stream means a new table
        /// </summary>
        /// <param name="itable"></param>
        public void AddTableToList(IPropertiesTable itable)
        {
            foreach (ChemicalStreamData data in (itable as ChemicalStreamPropertiesTable).ItemSource)
            {
                if(!(tableEntries.ContainsKey(data.Label)))
                {
                    tableEntries.Add(data.Label, data);
                }
            }
            TableDataChanged(itable, new EventArgs());
        }

        /// <summary>
        /// This is called whenever ChemicalStreamData in an exisiting table is changed
        /// </summary>
        /// <param name="itable">This is the table who's data was changed</param>
        public void DataChanged(DataUpdatedEventArgs changedData, ChemicalStreamData data,IPropertiesTable itable)
        {
            if (changedData.OldData as string != null && (changedData.OldData as string) != "")
            {
                tableEntries.Remove(changedData.OldData as string);
                tableEntries.Add(data.Label, data);
            }
            else
            {
                tableEntries.Remove(data.Label);
                tableEntries.Add(data.Label, data);
            }
            TableDataChanged(itable, new EventArgs());
        }

    }
}
