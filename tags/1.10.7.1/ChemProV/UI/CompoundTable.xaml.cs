using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.PFD;
using ChemProV.Validation;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.UI
{
    public partial class CompoundTable : UserControl
    {

        public CompoundTable()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This is what updates the datagrid when the compound selection is changed
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void Compound_ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            List<KeyValuePair<Element, int>> elements = new List<KeyValuePair<Element, int>>();
            List<CompoundTableData> items = new List<CompoundTableData>();

            if (Compound_ComboBox.SelectedItem != null)
            {
                elements = CompoundFactory.GetElementsOfCompound((Compound_ComboBox.SelectedItem as string).ToLower());

                foreach (KeyValuePair<Element, int> element in elements)
                {
                    items.Add(new CompoundTableData(element.Key.Name, element.Value));
                }

                Compound_DataGrid.ItemsSource = items;
            }

        }

        /// <summary>
        /// This is called when the pfd is changed not just when a compound is changed.  This gets a lit of ipfdElements
        /// then it pulls out of those the tables and then makes a list of the compounds which it then sets as the items source
        /// to our combo_box
        /// </summary>
        /// <param name="ipfdElements"></param>
        public void UpdateCompounds(IEnumerable<IPfdElement> ipfdElements)
        {
            HashSet<string> usingCompounds = new HashSet<string>();
            int currentSelected = Compound_ComboBox.SelectedIndex;

            ITableAdapter tableAdapter;

            foreach (IPfdElement ipfd in ipfdElements)
            {
                if (ipfd is IPropertiesTable)
                {
                    tableAdapter = TableAdapterFactory.CreateTableAdapter(ipfd as IPropertiesTable);
                    int i = 0;
                    while (i < tableAdapter.GetRowCount())
                    {
                        string compound = tableAdapter.GetCompoundAtRow(i);
                        if (compound != "Select" && compound != "Overall")
                        {
                            if (!usingCompounds.Contains(compound))
                            {
                                usingCompounds.Add(compound);
                            }
                        }
                        i++;
                    }
                }
            }
            Compound_ComboBox.ItemsSource = new ObservableCollection<string>(usingCompounds);
            if (usingCompounds.Count > 0)
            {
                if (currentSelected > -1)
                {
                    Compound_ComboBox.SelectedIndex = currentSelected;
                }
                else
                {
                    Compound_ComboBox.SelectedIndex = 0;
                }
            }
            Compound_ComboBox_SelectionChanged(this, EventArgs.Empty as SelectionChangedEventArgs);


        }
    }
}
