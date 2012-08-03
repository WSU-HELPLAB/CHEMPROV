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
using ChemProV.Logic;

namespace ChemProV.UI
{
    public partial class StreamTableControl : UserControl, Core.ICanvasElement
    {
        private ChemProV.UI.DrawingCanvas m_canvas;
        
        private bool m_ignoreTablePropertyChanges = false;
        
        /// <summary>
        /// Flag that can be set temporarily to ignore property change events from the 
        /// rows in m_table. This is set when elements in the UI are changed and we 
        /// update the table based on these changes.
        /// </summary>
        private bool m_ignoreRowPropertyChanges = false;

        private PFD.Streams.StreamControl m_parent;

        private bool m_programmaticallyChanging = false;

        private StreamPropertiesTable m_table;

        /// <summary>
        /// Reference to the temperature text box that only shows for chemical streams under a 
        /// certain difficulty setting. This is generated in the UpdateUI function.
        /// </summary>
        private TextBox m_tempTextBox = null;

        private Workspace m_ws = null;
        
        public StreamTableControl(StreamPropertiesTable tableData, 
            PFD.Streams.StreamControl parent, Workspace workspace, DrawingCanvas canvas)
        {
            InitializeComponent();

            m_table = tableData;
            m_parent = parent;
            m_ws = workspace;
            m_canvas = canvas;

            // Do the initial UI setup
            HeaderTextBlock.Text = "Stream #" + m_table.Stream.Id.ToString();
            Location = new Point(tableData.Location.X, tableData.Location.Y);
            UpdateUI();

            // Ensure that the minimize button eats mouse events
            MinimizeButton.MouseLeftButtonDown += this.MinimizeButton_MouseButtonEvent;
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

        private void AutoLabel(ChemicalStreamData row)
        {
            // Start by getting the character for the units
            char startChar;
            if ("massFrac" == row.SelectedUnits)
            {
                // We use 'X' and 'x' for mass fraction
                startChar = 'X';
            }
            else if ("molFrac" == row.SelectedUnits)
            {
                // Y,y for mole fraction
                startChar = 'Y';
            }
            else if ("mol %" == row.SelectedUnits || "mol" == row.SelectedUnits ||
                "mol/sec" == row.SelectedUnits)
            {
                // N,n for anything remaining that has moles
                startChar = 'N';
            }
            else
            {
                // M,m for everything else
                startChar = 'M';
            }

            // Determine the row number
            int rowNum = m_table.IndexOfRow(row);

            // Now set the actual label
            if ("Overall" == row.SelectedCompound)
            {
                // Label of the form: "[startChar.Upper][streamNum]"
                row.Label = startChar.ToString() + m_table.Stream.Id.ToString();
            }
            else if (string.IsNullOrEmpty(row.SelectedCompound))
            {
                // Label of the form: "[startChar.Lower][streamNum][rowNum]"
                row.Label = char.ToLower(startChar).ToString() + m_table.Stream.Id.ToString() + rowNum.ToString();
            }
            else
            {
                // Label of the form: "[startChar.Lower][streamNum][twoCharCompound]
                PFD.Streams.PropertiesWindow.Compound compound =
                    PFD.Streams.PropertiesWindow.CompoundFactory.GetElementsOfCompound(
                        row.SelectedCompound.ToLower());
                row.Label = char.ToLower(startChar).ToString() + m_table.Stream.Id.ToString() + compound.Abbr;
            }
        }

        private void ComboBoxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the column index for the control.Remember that there's an extra column in the 
            // grid (with respect to the data row) for the row removal buttons.
            int column = (int)(sender as ComboBox).GetValue(Grid.ColumnProperty) - 1;
            
            Tuple<IStreamData, string> info = (sender as ComboBox).Tag as
                Tuple<IStreamData, string>;
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

                // If we just changed the "SelectedUnits" field, then check if we have to auto-
                // rename the row
                ChemicalStreamData csd = info.Item1 as ChemicalStreamData;
                if ((info.Item2.Equals("SelectedUnits") || info.Item2.Equals("SelectedCompound")) && 
                    !info.Item1.UserHasRenamed && null != csd)
                {
                    m_programmaticallyChanging = true;

                    // Remove the event handler for the label text box while we change the label
                    TextBox tbLabel = GetControl(info.Item1, "Label") as TextBox;
                    tbLabel.TextChanged -= this.TextField_TextChanged;
                    
                    // Change the label in the data structure
                    AutoLabel(csd);

                    // Re-subscribe to the label text box changes
                    tbLabel.TextChanged += this.TextField_TextChanged;

                    m_programmaticallyChanging = false;
                    csd.UserHasRenamed = false;
                }
            }
        }

        /// <summary>
        /// Returns the data structure for this table. You can change properies in the returned value 
        /// and the UI will automatically update itself.
        /// </summary>
        public StreamPropertiesTable Data
        {
            get
            {
                return m_table;
            }
        }

        public Control GetControl(IStreamData row, string propertyName)
        {
            foreach (UIElement uie in MainGrid.Children)
            {
                Control c = uie as Control;
                if (null == c)
                {
                    continue;
                }
                
                // If it's a control that corresponds to an item in the data structure then it will 
                // have information in its tag
                Tuple<IStreamData, string> info = c.Tag as Tuple<IStreamData, string>;
                if (null == info)
                {
                    continue;
                }

                if (object.ReferenceEquals(info.Item1, row) && propertyName == info.Item2)
                {
                    return c;
                }
            }

            return null;
        }

        private void HeaderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas canvas = Core.App.Workspace.DrawingCanvas;
            canvas.SelectedElement = m_parent;
            canvas.CurrentState = UI.DrawingCanvasStates.MovingState.Create(
                this, canvas, m_ws);
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

        public PFD.Streams.StreamControl ParentStreamControl
        {
            get
            {
                return m_parent;
            }
        }

        private void RemoveRowBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get a reference to the row
            IStreamData row = (sender as Button).Tag as IStreamData;
            
            // Create an undo first
            m_ws.AddUndo(new UndoRedoCollection("Undo deletion of row",
                new Logic.Undos.InsertTableRow(m_table.IndexOfRow(row), row, m_table)));
            
            // Remove the row
            m_table.RemoveRow(row);
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
                Tuple<IStreamData, string> info = c.Tag as Tuple<IStreamData, string>;
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
                        (c as ComboBox).SelectedItem = val;
                    }
                }
            }
        }

        private void TextField_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the column index for the control.Remember that there's an extra column in the 
            // grid (with respect to the data row) for the row removal buttons.
            int column = (int)(sender as TextBox).GetValue(Grid.ColumnProperty) - 1;
            
            Tuple<IStreamData, string> info = (sender as TextBox).Tag as
                Tuple<IStreamData, string>;
            if (null != info)
            {
                // We handle the "Label" property in a specific way because we need to keep track of 
                // when the user changes this. If this was a label change by the user then we'll need 
                // to mark it as user-renamed. We will assume that it was a user change only if the 
                // current data row label is different from the text box text.
                if (info.Item2.Equals("Label"))
                {
                    if (!m_programmaticallyChanging && info.Item1.Label != (sender as TextBox).Text)
                    {
                        info.Item1.UserHasRenamed = true;
                    }
                }
                else
                {
                    // This is how we handle the general case for text field changes: just update the 
                    // corresponding data structure value.
                    m_ignoreRowPropertyChanges = true;
                    info.Item1[column] = (sender as TextBox).Text;
                    m_ignoreRowPropertyChanges = false;
                }
            }
        }

        private void Stream_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Id"))
            {
                // Update the header text at the top of the table
                HeaderTextBlock.Text = "Stream #" + m_table.Stream.Id.ToString();

                m_programmaticallyChanging = true;
                // The stream ID affects any auto-named labels, so update these too
                if (StreamType.Chemical == m_table.StreamType)
                {
                    foreach (IStreamData row in m_table.Rows)
                    {
                        if (row.UserHasRenamed)
                        {
                            // We don't touch it if the user has renamed it
                            continue;
                        }

                        AutoLabel(row as ChemicalStreamData);
                        row.UserHasRenamed = false;
                    }
                }

                // Auto label the temperature (if applicable)
                if (!m_table.UserHasChangedTemperature)
                {
                    // Change it only in the data structure (event handlers will update the UI)
                    m_table.Temperature = "TM" + m_table.Stream.Id.ToString();
                }

                m_programmaticallyChanging = false;
            }
        }

        private void TablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!m_ignoreTablePropertyChanges)
            {
                if (e.PropertyName.Equals("Location"))
                {
                    Location = new Point(m_table.Location.X, m_table.Location.Y);
                }
                else if (e.PropertyName.Equals("Temperature"))
                {
                    if (null != m_tempTextBox)
                    {
                        m_tempTextBox.Text = m_table.Temperature;
                    }
                }
            }
        }

        private void TableRowsChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void TemperatureTextChanged(object sender, TextChangedEventArgs e)
        {
            // There are two cases that could cause the text in the temperature text box to change:
            // 1. The user is typing a new value in the UI
            // 2. The text is being changed programmatically in response to a property change
            // In the first case we want to make sure we mark the temperature as having been changed 
            // by the user ("UserHasChangedTemperature" property of the table).
            // In the second case we must make sure that we do NOT change any properties in the table 
            // data structure.
            
            // Note that in case 2 the following sequence of events occurs:
            // 1. The property in the data structure will change
            // 2. this will fire an event that will set the text of the temperature text box
            // 3. this will invoke this text change event function.
            // In this case the text in the text box will match the text in the data structure and we 
            // don't have to take any action.
            if (m_table.Temperature == (sender as TextBox).Text)
            {
                return;
            }
            
            m_ignoreRowPropertyChanges = true;
            m_table.Temperature = (sender as TextBox).Text;
            m_table.UserHasChangedTemperature = true;
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
            foreach (IStreamData row in m_table.Rows)
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
                        tb.Tag = new Tuple<IStreamData, string>(row, propertyName);
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
                        cb.Tag = new Tuple<IStreamData, string>(row, propertyName);
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

                    // Add an undo to the workspace that will remove it
                    m_ws.AddUndo(new UndoRedoCollection("Undo addition of new row",
                        new Logic.Undos.RemoveTableRow(count - 1, m_table)));

                    m_programmaticallyChanging = true;
                    TextBox tbLabel = GetControl(m_table.Rows[count - 1], "Label") as TextBox;
                    tbLabel.TextChanged -= this.TextField_TextChanged;
                    m_table.Rows[count - 1].Label = "m" + m_table.Stream.Id.ToString() + count.ToString();
                    tbLabel.TextChanged += this.TextField_TextChanged;
                    m_programmaticallyChanging = false;
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

                // Keep a reference to this temperature text box
                m_tempTextBox = tb;

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
            else
            {
                m_tempTextBox = null;
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
