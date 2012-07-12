/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using System.ComponentModel;
using ChemProV.Logic;

namespace ChemProV.UI
{
    public partial class CompoundTable : UserControl, ChemProV.Core.IWorkspaceChangeListener
    {
        private Workspace m_ws = null;
        
        public CompoundTable()
        {
            InitializeComponent();
        }

        public event EventHandler ConstantClicked = delegate { };

        /// <summary>
        /// This is what updates the datagrid when the compound selection is changed
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void Compound_ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            List<CompoundTableData> elements = new List<CompoundTableData>();
            List<ConstantsTableData> constants = new List<ConstantsTableData>();
            Compound compound;

            if (Compound_ComboBox.SelectedItem != null)
            {
                compound = CompoundFactory.GetElementsOfCompound((Compound_ComboBox.SelectedItem as string).ToLower());

                foreach (KeyValuePair<Element, int> element in compound.elements)
                {
                    elements.Add(new CompoundTableData(element.Key.Name, element.Value));
                }

                constants.Add(new ConstantsTableData("Cp (kJ/mol-C)", "Cp" + compound.Abbr));
                constants.Add(new ConstantsTableData("Hf (kJ/mol)", "Hf" + compound.Abbr));
                constants.Add(new ConstantsTableData("Hv (kJ/mol)", "Hv" + compound.Abbr));
                constants.Add(new ConstantsTableData("Tb (C)", "Tb" + compound.Abbr));
                constants.Add(new ConstantsTableData("Tm (C)", "Tm" + compound.Abbr));

                Compound_DataGrid.ItemsSource = elements;
                Constants_DataGrid.ItemsSource = constants;
            }
            else
            {
                Compound_DataGrid.ItemsSource = elements;
                Constants_DataGrid.ItemsSource = constants;
            }
        }

        private void Constants_DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            string constantColumnContents = ((Constants_DataGrid.ItemsSource as List<ConstantsTableData>)[e.Row.GetIndex()].Constant);
            string toolTipMessage = "";
            TextBlock cell = new TextBlock();
            switch (constantColumnContents)
            {
                case "Cp (kJ/mol-C)":
                    {
                        DataGridColumn elementColumn = Constants_DataGrid.Columns[0];
                        cell = elementColumn.GetCellContent(e.Row) as TextBlock;
                        toolTipMessage = String.Format("This represents the heat capacity for {0}.", getCompoundComboBoxSelectedItemAsString());
                    }
                    break;
                case "Hf (kJ/mol)":
                    {
                        DataGridColumn elementColumn = Constants_DataGrid.Columns[0];
                        cell = elementColumn.GetCellContent(e.Row) as TextBlock;
                        toolTipMessage = String.Format("This represents the heat of formation for {0}.", getCompoundComboBoxSelectedItemAsString());
                    }
                    break;
                case "Hv (kJ/mol)":
                    {
                        DataGridColumn elementColumn = Constants_DataGrid.Columns[0];
                        cell = elementColumn.GetCellContent(e.Row) as TextBlock;
                        toolTipMessage = String.Format("This represents the heat of vaporization for {0}.", getCompoundComboBoxSelectedItemAsString());
                    }
                    break;
                case "Tb (C)":
                    {
                        DataGridColumn elementColumn = Constants_DataGrid.Columns[0];
                        cell = elementColumn.GetCellContent(e.Row) as TextBlock;
                        toolTipMessage = String.Format("This represents the boiling point for {0}.", getCompoundComboBoxSelectedItemAsString());
                    }
                    break;
                case "Tm (C)":
                    {
                        DataGridColumn elementColumn = Constants_DataGrid.Columns[0];
                        cell = elementColumn.GetCellContent(e.Row) as TextBlock;
                        toolTipMessage = String.Format("This represents the melting point for {0}.", getCompoundComboBoxSelectedItemAsString());
                    }
                    break;
            }
            ToolTipService.SetToolTip(cell, toolTipMessage);
        }

        private string getCompoundComboBoxSelectedItemAsString()
        {
            if (Compound_ComboBox.SelectedItem == null)
            {
                return "";
            }
            else
            {
                return Compound_ComboBox.SelectedItem.ToString();
            }
        }

        private void Constant_Symbol_Button_Click(object sender, RoutedEventArgs e)
        {
            ConstantClicked(sender, e);
        }

        #region IWorkspaceChangeListener Members

        public void SetWorkspace(Workspace workspace)
        {
            // If we have a previous workspace, then unsubscribe from events
            if (null != m_ws)
            {
                // Currently the application never does this. We get a workspace reference once 
                // as the application initializes and that's it.
                throw new NotImplementedException();
            }
            
            m_ws = workspace;

            // We need to watch changes in the stream collection and every time a new 
            // one is added we need to attach listeners.
            m_ws.StreamsCollectionChanged += new EventHandler(WorkspaceStreamsCollectionChanged);
        }

        #endregion

        private void WorkspaceStreamsCollectionChanged(object sender, EventArgs e)
        {
            foreach (AbstractStream stream in m_ws.Streams)
            {
                // Unsubscribe first for safety. A += WILL cause the event handler to fire twice if 
                // we were already subscribing and we want to avoid this. However, a -= when we're 
                // not subscribed does not cause an error.
                stream.PropertiesTable.RowPropertyChanged -= this.PropertiesTable_RowPropertyChanged;
                
                stream.PropertiesTable.RowPropertyChanged += this.PropertiesTable_RowPropertyChanged;
            }

            // Invoke the property change event because the streams that were added most likely 
            // have tables and those tables could have compounds selected.
            PropertiesTable_RowPropertyChanged(null, new PropertyChangedEventArgs("SelectedCompound"));
        }

        private void PropertiesTable_RowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We only care about the selected compounds
            if (!e.PropertyName.Equals("SelectedCompound"))
            {
                return;
            }

            // Backup the currently selected item
            string currentSelection = Compound_ComboBox.SelectedItem as string;

            // Clear the combo box items
            Compound_ComboBox.Items.Clear();

            // Go through all stream properties tables
            foreach (AbstractStream stream in m_ws.Streams)
            {
                StreamPropertiesTable table = stream.PropertiesTable;
                if (null == table)
                {
                    continue;
                }
                if (StreamType.Chemical != table.StreamType)
                {
                    continue;
                }

                // Go through the rows and look for at the selected compound. Add items to the 
                // combo box as needed.
                foreach (IStreamData data in table.Rows)
                {
                    string s = (data as ChemicalStreamData).SelectedCompound;
                    if (!string.IsNullOrEmpty(s) && !s.Equals("Overall") && 
                        !Compound_ComboBox.Items.Contains(s))
                    {
                        Compound_ComboBox.Items.Add(s);
                    }
                }
            }

            // Re-select what was previously selected
            if (!string.IsNullOrEmpty(currentSelection) &&
                Compound_ComboBox.Items.Contains(currentSelection))
            {
                Compound_ComboBox.SelectedItem = currentSelection;
            }

            // Invoke the selection-changed event to ensure that everything works
            Compound_ComboBox_SelectionChanged(this, EventArgs.Empty as SelectionChangedEventArgs);
        }
    }
}