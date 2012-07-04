/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using ChemProV.Core;
using ChemProV.UI.DrawingCanvas;

namespace ChemProV.UI
{
    public partial class StreamTableControl : UserControl, Core.ICanvasElement
    {
        private ChemProV.UI.DrawingCanvas.DrawingCanvas m_canvas;
        
        private bool m_ignoreTablePropertyChanges = false;
        
        /// <summary>
        /// Flag that can be set temporarily to ignore property change events from the 
        /// rows in m_table. This is set when elements in the UI are changed and we 
        /// update the table based on these changes.
        /// </summary>
        private bool m_ignoreRowPropertyChanges = false;

        private Core.StreamPropertiesTable m_table;

        private Workspace m_ws = null;
        
        public StreamTableControl(Core.StreamPropertiesTable table, Workspace workspace,
            DrawingCanvas.DrawingCanvas canvas)
        {
            InitializeComponent();

            m_table = table;
            m_ws = workspace;
            m_canvas = canvas;

            // Do the initial UI setup
            HeaderTextBlock.Text = "Stream #" + m_table.Stream.Id.ToString();
            Location = new Point(table.Location.X, table.Location.Y);
            UpdateUI();

            // Ensure that the minimize button eats mouse events
            MinimizeButton.MouseLeftButtonDown += new MouseButtonEventHandler(MinimizeButton_MouseButtonEvent);
            MinimizeButton.MouseLeftButtonUp += this.MinimizeButton_MouseButtonEvent;

            // Monitor changes in the parent stream (all we care about is the Id)
            m_table.Stream.PropertyChanged += this.Stream_PropertyChanged;

            // Monitor changes in the row data
            m_table.RowPropertyChanged += new PropertyChangedEventHandler(RowPropertyChanged);

            m_table.RowsChanged += new EventHandler(TableRowsChanged);

            // Monitor changes to the difficulty level so we can show/hide the temperature row 
            // for chemical stream property tables
            m_ws.PropertyChanged += new PropertyChangedEventHandler(WorkspacePropertyChanged);

            // Monitor changes to m_table
            m_table.PropertyChanged += new PropertyChangedEventHandler(TablePropertyChanged);
        }

        private void ComboBoxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the column index for the control.Remember that there's an extra column in the 
            // grid (with respect to the data row) for the row removal buttons.
            int column = (int)(sender as ComboBox).GetValue(Grid.ColumnProperty) - 1;
            
            Tuple<Core.IStreamData, string> info = (sender as ComboBox).Tag as
                Tuple<Core.IStreamData, string>;
            if (null != info)
            {
                string newValue = (sender as ComboBox).SelectedItem as string;
                if (null == newValue)
                {
                    newValue = (sender as ComboBox).SelectedItem.ToString();
                }

                // Set the new value
                m_ignoreRowPropertyChanges = true;
                info.Item1[column] = newValue;
                m_ignoreRowPropertyChanges = false;
            }
        }

        /// <summary>
        /// Returns the data structure for this table. You can change properies in the returned value 
        /// and the UI will automatically update itself.
        /// </summary>
        public Core.StreamPropertiesTable Data
        {
            get
            {
                return m_table;
            }
        }

        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas.DrawingCanvas canvas = Core.App.Workspace.DrawingCanvas;
            canvas.SelectedElement = this;
            canvas.CurrentState = UI.DrawingCanvas.States.MovingState.Create(
                canvas, m_ws);
        }

        public Point Location
        {
            get
            {
                return new Point(m_table.Location.X, m_table.Location.Y);
            }
            set
            {
                // We need to measure the control in order to get an accurate width
                this.Measure(new Size(1000.0, 1000.0));
                double w = this.DesiredSize.Width;
                
                SetValue(Canvas.LeftProperty, value.X - w / 2.0);
                SetValue(Canvas.TopProperty, value.Y - 5.0);
                
                m_ignoreTablePropertyChanges = true;
                m_table.Location = new MathCore.Vector(value.X, value.Y);
                m_ignoreTablePropertyChanges = false;
            }
        }

        /// <summary>
        /// Captures left mouse button down and up events to prevent them from bubbling up to the 
        /// drawing canvas, which could try to reposition the table when this button is clicked 
        /// and that's not what we want.
        /// </summary>
        private void MinimizeButton_MouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void RemoveRowBtn_Click(object sender, RoutedEventArgs e)
        {
            Core.IStreamData row = (sender as Button).Tag as Core.IStreamData;
            m_table.RemoveRow(row);

            // TODO: Setup event handlers and take this out:
            UpdateUI();
        }

        /// <summary>
        /// Fired when any row in the table has a property changed. Note that this is for a 
        /// property change in a single existing row. We handle addition/removal of rows 
        /// elsewhere.
        /// </summary>
        /// <param name="sender">The row object that had a property changed</param>
        /// <param name="e">Info about the change</param>
        private void RowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (m_ignoreRowPropertyChanges)
            {
                return;
            }
            
            // Find the child control that corresponds to the property
            foreach (UIElement uie in MainGrid.Children)
            {
                Control c = uie as Control;
                if (null == c)
                {
                    continue;
                }

                // Look for information about the row and property in the tag
                Tuple<Core.IStreamData, string> info = c.Tag as Tuple<Core.IStreamData, string>;
                if (null == info)
                {
                    continue;
                }

                if (object.ReferenceEquals(info.Item1, sender) && info.Item2.Equals(e.PropertyName))
                {
                    // Use reflection to get the actual property value
                    System.Reflection.PropertyInfo pi = sender.GetType().GetProperty(e.PropertyName);
                    string val = pi.GetValue(sender, null).ToString();
                    
                    // The control will either be a combo box or a text box
                    TextBox tb = c as TextBox;
                    if (null != tb)
                    {
                        tb.Text = val;
                    }
                    else
                    {
                        ((ComboBox)sender).SelectedItem = val;
                    }
                }
            }
        }

        private void TextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the column index for the control.Remember that there's an extra column in the 
            // grid (with respect to the data row) for the row removal buttons.
            int column = (int)(sender as TextBox).GetValue(Grid.ColumnProperty) - 1;
            
            Tuple<Core.IStreamData, string> info = (sender as TextBox).Tag as
                Tuple<Core.IStreamData, string>;
            if (null != info)
            {
                m_ignoreRowPropertyChanges = true;
                info.Item1[column] = (sender as TextBox).Text;
                m_ignoreRowPropertyChanges = false;
            }
        }

        private void Stream_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Id"))
            {
                HeaderTextBlock.Text = "Stream #" + m_table.Stream.Id.ToString();
            }
        }

        private void TablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!m_ignoreTablePropertyChanges)
            {
                // All we care about is the location
                if (e.PropertyName.Equals("Location"))
                {
                    Location = new Point(m_table.Location.X, m_table.Location.Y);
                }
            }
        }

        private void TableRowsChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void TemperatureTextChanged(object sender, TextChangedEventArgs e)
        {
            m_ignoreRowPropertyChanges = true;
            m_table.Temperature = (sender as TextBox).Text;
            m_ignoreRowPropertyChanges = false;
        }

        private void TempUnitsCBSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_ignoreRowPropertyChanges = true;
            m_table.TemperatureUnits = (0 == (sender as ComboBox).SelectedIndex) ?
                "celsius" : "fahrenheit";
            m_ignoreRowPropertyChanges = false;
        }

        /// <summary>
        /// Updates the entire control based on data from m_table
        /// </summary>
        private void UpdateUI()
        {
            int i;
            
            // Start by checking to see if we need the column for adding/removing rows
            if (!m_table.CanAddRemoveRows)
            {
                // "Collapse" the first column
                MainGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
            }

            // If we're making a heat stream table then there's no "Compounds" column
            if (StreamType.Heat == m_table.StreamType)
            {
                // "Collapse" the second column
                MainGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
            }

            // We want to clear all rows beneath the headers and rebuild. This means we have to remove 
            // child controls in these rows, remove row definitions, then re-add.
            for (i = 0; i<MainGrid.Children.Count; i++)
            {
                UIElement child = MainGrid.Children[i];
                try
                {
                    int childRowIndex = (int)child.GetValue(Grid.RowProperty);
                    if (childRowIndex >= 2)
                    {
                        MainGrid.Children.RemoveAt(i);
                        i--;
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
            while (MainGrid.RowDefinitions.Count > 2)
            {
                MainGrid.RowDefinitions.RemoveAt(2);
            }

            i = 2;
            foreach (Core.IStreamData row in m_table.Rows)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = GridLength.Auto
                    });

                // Create the "-" button if we can add/remove rows
                if (m_table.CanAddRemoveRows)
                {
                    Button removeRowBtn = new Button();
                    removeRowBtn.Tag = row;
                    removeRowBtn.Content = "-";
                    MainGrid.Children.Add(removeRowBtn);
                    removeRowBtn.SetValue(Grid.ColumnProperty, 0);
                    removeRowBtn.SetValue(Grid.RowProperty, i);
                    removeRowBtn.Click += new RoutedEventHandler(RemoveRowBtn_Click);
                }

                // IStream data objects implement GetColumnUIObject(index) which gives an object 
                // that should be either a collection of strings or some other object (most likely 
                // just a string). If it's a collection of strings then we create a combo box, 
                // otherwise we create a text box. We do this iteratively to make controls across 
                // a row.
                int column, dataColumn = 1;
                if (StreamType.Heat == m_table.StreamType)
                {
                    column = 2;
                    dataColumn = 0;
                }
                else
                {
                    column = 1;
                    dataColumn = 0;
                }
                string propertyName;
                object o = row.GetColumnUIObject(dataColumn, out propertyName);
                while (null != o && column < 5)
                {
                    dataColumn = (m_table.StreamType == StreamType.Heat) ?
                        (column - 2) : (column - 1);
                    
                    IEnumerable<string> options = o as IEnumerable<string>;
                    if (null == options)
                    {
                        // Assume string type and make a text box
                        TextBox tb = new TextBox();
                        MainGrid.Children.Add(tb);
                        tb.SetValue(Grid.ColumnProperty, column);
                        tb.SetValue(Grid.RowProperty, i);
                        tb.Style = this.Resources["TextBoxStyle"] as Style;
                        tb.Text = row[dataColumn] as string;

                        // Make sure it has a right-click menu
                        Core.App.InitRightClickMenu(tb);

                        // When it changes we need to update the data structure
                        tb.Tag = new Tuple<Core.IStreamData, string>(row, propertyName);
                        tb.TextChanged += this.TextField_TextChanged;

                        tb.GotFocus += delegate(object sender, RoutedEventArgs e)
                        {
                            m_canvas.SelectedElement = null;
                        };
                        tb.KeyDown += delegate(object sender, KeyEventArgs e)
                        {
                            if (Key.Back == e.Key)
                            {
                                // Make sure this doesn't bubble up
                                e.Handled = true;
                            }
                            else if (Key.Enter == e.Key)
                            {
                                e.Handled = true;
                            }
                        };
                    }
                    else
                    {
                        // This means we have a collection of strings an want a combo box
                        ComboBox cb = new ComboBox();
                        MainGrid.Children.Add(cb);
                        cb.SetValue(Grid.ColumnProperty, column);
                        cb.SetValue(Grid.RowProperty, i);
                        cb.ItemsSource = options;
                        cb.Style = this.Resources["ComboBoxStyle"] as Style;
                        cb.SelectedItem = row[dataColumn] as string;

                        // Handle selected item change events
                        cb.Tag = new Tuple<Core.IStreamData, string>(row, propertyName);
                        cb.SelectionChanged += this.ComboBoxField_SelectionChanged;
                    }

                    // Get the next UI object
                    column++;
                    dataColumn++;
                    o = row.GetColumnUIObject(dataColumn, out propertyName);
                }

                // Go to the next row
                i++;
            }

            if (m_table.CanAddRemoveRows)
            {
                // Add one final row for the "+" button
                MainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(18.0)
                });
                Button plusBtn = new Button();
                MainGrid.Children.Add(plusBtn);
                plusBtn.SetValue(Grid.RowProperty, i);
                plusBtn.SetValue(Grid.ColumnProperty, 0);
                plusBtn.Content = "+";

                // When it's clicked have it add a row to m_table
                plusBtn.Click += delegate(object sender, RoutedEventArgs e)
                {
                    m_table.AddNewRow();
                    int count = m_table.RowCount;
                    m_table.Rows[count - 1].Label = "m" + m_table.Stream.Id.ToString() + count.ToString();
                };

                i++;
            }

            // If we're a chemical stream properties table and we're on the hardest difficulty 
            // level then show the temperature row.
            if (StreamType.Chemical == m_table.StreamType &&
                OptionDifficultySetting.MaterialAndEnergyBalance == m_ws.Difficulty)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });

                TextBlock tblock = new TextBlock()
                {
                    Text = "Temperature ="
                };
                MainGrid.Children.Add(tblock);
                tblock.SetValue(Grid.RowProperty, i);
                tblock.SetValue(Grid.ColumnProperty, 0);
                tblock.SetValue(Grid.ColumnSpanProperty, 2);

                TextBox tb = new TextBox()
                {
                    Text = m_table.Temperature
                };
                MainGrid.Children.Add(tb);
                tb.SetValue(Grid.RowProperty, i);
                tb.SetValue(Grid.ColumnProperty, 2);
                tb.TextChanged += new TextChangedEventHandler(TemperatureTextChanged);
                Core.App.InitRightClickMenu(tb);

                tblock = new TextBlock()
                {
                    Text = "Temp. Units:"
                };
                MainGrid.Children.Add(tblock);
                tblock.SetValue(Grid.RowProperty, i);
                tblock.SetValue(Grid.ColumnProperty, 3);

                ComboBox cb = new ComboBox();
                cb.Items.Add("celsius");
                cb.Items.Add("fahrenheit");
                cb.SelectedItem = m_table.TemperatureUnits;
                MainGrid.Children.Add(cb);
                cb.SetValue(Grid.RowProperty, i);
                cb.SetValue(Grid.ColumnProperty, 4);
                cb.SelectionChanged += new SelectionChangedEventHandler(TempUnitsCBSelectionChanged);
            }
        }

        private void WorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Difficulty"))
            {
                UpdateUI();
            }
        }
    }
}
