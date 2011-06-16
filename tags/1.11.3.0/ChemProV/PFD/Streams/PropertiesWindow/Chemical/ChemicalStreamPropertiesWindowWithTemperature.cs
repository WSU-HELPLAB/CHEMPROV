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

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public class ChemicalStreamPropertiesWindowWithTemperature : ChemicalStreamPropertiesWindow
    {
        private static string[] TempUnits = { "celsius", "fahrenheit" };


        /// <summary>
        /// This is the function that sets the grid to show the ExpandedView
        /// </summary>
        protected override void ExpandedView()
        {
            base.ExpandedView();

            if (ItemSource.Count > 0)
            {
                createTempatureCells(ItemSource[0]);
            }

        }

        //call the base constructor
        public ChemicalStreamPropertiesWindowWithTemperature(ChemicalStream stream)
            : base(stream)
        {
        }

        //call the base constructor
        public ChemicalStreamPropertiesWindowWithTemperature()
            : base()
        {
        }

        /// <summary>
        /// This function creates the cells dealing with Tempature at the bottem
        /// </summary>
        /// <param name="data"></param>
        private void createTempatureCells(ChemicalStreamData data)
        {
            TextBlock tb = new TextBlock();
            int row = ItemSource.Count + 2;
            tb.Text = "Temperature = ";
            tb.VerticalAlignment = VerticalAlignment.Center;
            base.PropertiesGrid.PlaceUIElement(tb, 1, row);

            TextBox txtBox = new TextBox();
            txtBox.Text = data.Temperature;
            txtBox.TextChanged += new TextChangedEventHandler(Temperature_TextChanged);
            txtBox.GotFocus += new RoutedEventHandler(base.tb_GotFocus);
            txtBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            base.PropertiesGrid.PlaceUIElement(txtBox, 2, row);

            tb = new TextBlock();
            tb.Margin = new Thickness(2, 0, 0, 2);
            tb.Text = "Temp. Units: ";
            tb.VerticalAlignment = VerticalAlignment.Center;
            base.PropertiesGrid.PlaceUIElement(tb, 3, row);

            ComboBox comboBox = new ComboBox();
            ComboBoxItem cbi;
            foreach (string s in TempUnits)
            {
                cbi = new ComboBoxItem();
                cbi.Content = s;
                comboBox.Items.Add(cbi);
            }
            comboBox.SelectedIndex = data.TempUnits;
            comboBox.Background = new SolidColorBrush(Colors.White);
            comboBox.BorderBrush = new SolidColorBrush(Colors.White);
            comboBox.SelectionChanged += new SelectionChangedEventHandler(TempUnits_SelectionChanged);

            base.PropertiesGrid.PlaceUIElement(comboBox, 4, row);
        }

        void TempUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemSource[0].TempUnits = (sender as ComboBox).SelectedIndex;
        }

        void Temperature_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.LostFocus += new RoutedEventHandler(TempertureTextBox_LostFocus);
        }

        void TempertureTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.LostFocus -= new RoutedEventHandler(TempertureTextBox_LostFocus);

            try
            {
                ItemSource[0].Temperature = double.Parse(tb.Text).ToString();
            }
            catch
            {
                tb.Text = "T" + ItemSource[0].Label;
                ItemSource[0].Temperature = "T" + ItemSource[0].Label;
            }
            UpdateGrid();
        }

    }
}
