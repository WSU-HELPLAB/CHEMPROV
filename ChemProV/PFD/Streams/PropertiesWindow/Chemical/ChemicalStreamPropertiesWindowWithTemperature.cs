/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public class ChemicalStreamPropertiesWindowWithTemperature : ChemicalStreamPropertiesWindow
    {
        private string[] TempUnits = { "celsius", "fahrenheit" };

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
        public ChemicalStreamPropertiesWindowWithTemperature(ChemicalStream stream, bool isReadOnly)
            : base(stream, isReadOnly)
        {
        }

        //call the base constructor
        public ChemicalStreamPropertiesWindowWithTemperature(bool isReadOnly)
            : base(isReadOnly)
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

            if (base.isReadOnly)
            {
                TextBlock txtBox = new TextBlock();
                txtBox.Text = data.Temperature;
                base.PropertiesGrid.PlaceUIElement(txtBox, 2, row);
            }
            else
            {
                TextBox txtBox = new TextBox();
                txtBox.IsReadOnly = base.isReadOnly;
                txtBox.Text = data.Temperature;
                txtBox.TextChanged += new TextChangedEventHandler(Temperature_TextChanged);
                txtBox.GotFocus += new RoutedEventHandler(base.QuantityTextBox_GotFocus);
                txtBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
                base.PropertiesGrid.PlaceUIElement(txtBox, 2, row);
            }

            tb = new TextBlock();
            tb.Margin = new Thickness(2, 0, 0, 2);
            tb.Text = "Temp. Units: ";
            tb.VerticalAlignment = VerticalAlignment.Center;
            base.PropertiesGrid.PlaceUIElement(tb, 3, row);

            if (isReadOnly)
            {
                tb = new TextBlock();
                tb.Text = TempUnits[data.TempUnits];
                base.PropertiesGrid.PlaceUIElement(tb, 4, row);
            }
            else
            {
                ComboBox comboBox = new ComboBox();
                comboBox.IsEnabled = !base.isReadOnly;
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

                if (!isReadOnly)
                {
                    comboBox.SelectionChanged += new SelectionChangedEventHandler(TempUnits_SelectionChanged);
                }

                base.PropertiesGrid.PlaceUIElement(comboBox, 4, row);
            }
        }

        private void TempUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemSource[0].TempUnits = (sender as ComboBox).SelectedIndex;
        }

        private void Temperature_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.LostFocus += new RoutedEventHandler(TempertureTextBox_LostFocus);
            base.ChangeInProgress(this, EventArgs.Empty);
        }

        private void TempertureTextBox_LostFocus(object sender, RoutedEventArgs e)
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

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteStartElement("Temperature");
            writer.WriteElementString("Quantity", ItemSource[0].Temperature);
            writer.WriteElementString("Units", ItemSource[0].TempUnits.ToString());
            writer.WriteEndElement();
        }
    }
}