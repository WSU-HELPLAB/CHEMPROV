/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{

    public partial class ChemicalStreamPropertiesWindow : UserControl, IPropertiesWindow, IComparable
    {
        public event EventHandler SelectionChanged = delegate { };
        public event EventHandler LocationChanged = delegate { };

        public event TableDataEventHandler TableDataChanged = delegate { };

        public event EventHandler TableDataChanging = delegate { };

        //this saves where the expanded view was before it was collapsed so it can be restored
        private Point expandedViewSavedLocation = new Point();

        /// <summary>
        /// gets the saved of where the expanded view was before it was collapsed so it can be restored
        /// </summary>
        public Point ExpandedViewSavedLocation
        {
            get
            {
                if (view == View.Collapsed)
                {
                    return expandedViewSavedLocation;
                }
                else
                {
                    return new Point((double)this.GetValue(Canvas.LeftProperty), (double)this.GetValue(Canvas.TopProperty));
                }
            }
        }

        //yellow
        private Brush highlightFeedbackBrush = new SolidColorBrush(Colors.Yellow);

        //reference to the parent stream
        private IStream parentStream = null;

        //this is the label that holds the feedback in the first row of the table
        private Label feedbackLabel = null;

        //similar to the feedback label this points to the ToolTop being used
        private Silverlight.Controls.ToolTip feedbackToolTip;

        private LinearGradientBrush headerBrush;

        //Keeps track of the number of Tables that have been made
        protected static int NumberOfTables = 1;

        //this keeps the record of what table number the table is when it is created
        private const string massTablePrefix = "M";
        private const string moleTablePrefix = "N";

        //start with assuming a mass table
        private string tableName = massTablePrefix;

        /// <summary>
        /// This a list of Data which is exactly what is in the table
        /// </summary>
        public ObservableCollection<ChemicalStreamData> ItemSource = new ObservableCollection<ChemicalStreamData>();

        //this is a reference to the grid we are using
        protected CustomDataGrid PropertiesGrid = new CustomDataGrid();

        private View view = View.Expanded;

        protected bool isReadOnly = false;

        /// <summary>
        /// This holds the tables current view state.  It is either collapsed or expanded.
        /// When this is set it automatically calls UpdateGrid to update it accordingly
        /// </summary>
        public View View
        {
            get
            {
                return view;
            }
            set
            {
                view = value;
                UpdateGrid();
            }
        }

        public ChemicalStreamPropertiesWindow()
        {
            InitializeComponent();
            LocalInit(false);
        }

        public ChemicalStreamPropertiesWindow(bool isReadOnly)
        {
            InitializeComponent();
            LocalInit(isReadOnly);
        }

        public ChemicalStreamPropertiesWindow(IStream parent, bool isReadOnly)
        {
            InitializeComponent();
            ParentStream = parent;
            LocalInit(isReadOnly);
        }

        /// <summary>
        /// This is the function that sets the grid to show the ExpandedView
        /// </summary>
        protected virtual void ExpandedView()
        {
            PropertiesGrid.ClearAll();

            PropertiesGrid.HideBordersForLastRow = true;

            createHeaderRow(false);

            int row = 0;
            foreach (ChemicalStreamData data in ItemSource)
            {
                createDataRow(false, data, row, row == ItemSource.Count - 1);
                row++;
            }

            setFeedBack();

            this.SetValue(Canvas.LeftProperty, expandedViewSavedLocation.X);
            this.SetValue(Canvas.TopProperty, expandedViewSavedLocation.Y);
        }

        /// <summary>
        /// This function is called whenver the view is changed and it handles the feedback label and tooltip
        /// </summary>
        private void setFeedBack()
        {
            if (feedbackLabel != null)
            {
                if (View == PropertiesWindow.View.Expanded)
                {
                    Label lb = new Label();
                    lb.Content = ItemSource[0].Feedback;

                    if (feedbackLabel.Background == highlightFeedbackBrush)
                    {
                        lb.Background = highlightFeedbackBrush;
                    }
                    feedbackLabel = lb;
                    Silverlight.Controls.ToolTipService.SetToolTip(lb, feedbackToolTip);
                    PropertiesGrid.PlaceUIElement(feedbackLabel, 5, 1);
                }
                else
                {
                    Label lb = PropertiesGrid.GetUIElementAt(0, 0) as Label;
                    if (feedbackLabel.Background == highlightFeedbackBrush)
                    {
                        lb.Background = highlightFeedbackBrush;
                    }
                    else if (ItemSource[0].Feedback != null || ItemSource[0].Feedback != "")
                    {
                        lb.Background = new SolidColorBrush(Colors.Red);
                    }
                    feedbackLabel = lb;
                    Silverlight.Controls.ToolTipService.SetToolTip(lb, feedbackToolTip);
                }
            }
        }

        private void createHeaderRow(bool collapsed)
        {
            Label tb = new Label();

            tb.Content = "";
            tb.Background = headerBrush;
            tb.BorderBrush = headerBrush;
            tb.BorderThickness = new Thickness(1);
            PropertiesGrid.PlaceUIElement(tb, 0, 0);

            tb = new Label();
            tb.Content = "Label";
            tb.Background = headerBrush;
            PropertiesGrid.PlaceUIElement(tb, 1, 0);

            int column;
            if (collapsed == false)
            {
                tb = new Label();
                tb.Content = "Qty";
                tb.Background = headerBrush;
                PropertiesGrid.PlaceUIElement(tb, 2, 0);

                tb = new Label();
                tb.Content = "Units";
                tb.Background = headerBrush;
                PropertiesGrid.PlaceUIElement(tb, 3, 0);
                column = 4;
            }
            else
            {
                column = 2;
            }
            tb = new Label();
            tb.Content = "Compounds";
            tb.Background = headerBrush;
            PropertiesGrid.PlaceUIElement(tb, column, 0);
            if (feedbackLabel != null)
            {
                tb = new Label();
                tb.Background = headerBrush;
                PropertiesGrid.PlaceUIElement(tb, column + 1, 0);
            }
            tb = new Label();
            tb.Background = headerBrush;
            PropertiesGrid.PlaceUIElement(tb, column + 2, 0);

            Button ToggleViewButton = new Button();
            ToggleViewButton.Style = this.Resources["SquareButton"] as Style;
            GradientStopCollection gsc = new GradientStopCollection();
            GradientStop gs = new GradientStop();
            gs.Color = Color.FromArgb(225, 200, 207, 230);
            gs.Offset = 1;
            gsc.Add(gs);
            gs = new GradientStop();
            gs.Color = Color.FromArgb(255, 225, 235, 250);
            gs.Offset = 0.0;
            gsc.Add(gs);

            ToggleViewButton.Background = new LinearGradientBrush(gsc, 90);
            if (collapsed == false)
            {
                ToggleViewButton.Content = "<";
            }
            else
            {
                ToggleViewButton.Content = ">";
            }
            ToggleViewButton.Click += new RoutedEventHandler(ToggleView);
            System.Windows.Controls.ToolTip tp = new System.Windows.Controls.ToolTip();
            tp.Content = "Click To Toggle Between The Collapsed View And The Expanded View";
            System.Windows.Controls.ToolTipService.SetToolTip(ToggleViewButton, tp);
            Border br = new Border();
            br.Child = ToggleViewButton;
            br.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210));
            br.BorderThickness = new Thickness(1);
            Grid.SetRowSpan((br as FrameworkElement), int.MaxValue);
            PropertiesGrid.PlaceUIElement(br, column + 2, 1);
        }

        private void createDataRow(bool collapsedRow, ChemicalStreamData data, int row, bool lastRow)
        {
            if (!isReadOnly)
            {
                //create buttons and place them on the grid
                createButtons(row, lastRow);
            }

            //row + 1 because row does not take into account the header row so we must
            PropertiesGrid.PlaceUIElement(CreateLabelCell(row), 1, row + 1);

            if (collapsedRow == false)
            { 
                PropertiesGrid.PlaceUIElement(CreateQuantityCell(row), 2, row + 1);

                PropertiesGrid.PlaceUIElement(CreateUnitsCell(row), 3, row + 1);

                PropertiesGrid.PlaceUIElement(CreateCompoundCell(row), 4, row + 1);
            }

            else
            {
                PropertiesGrid.PlaceUIElement(CreateCompoundCell(row), 2, row + 1);
            }
        }

        private UIElement CreateLabelCell(int row)
        {
            if (isReadOnly)
            {
                TextBlock tb = new TextBlock();
                tb.Text = ItemSource[row].Label;
                return tb;
            }
            else
            {
                TextBox tb = new TextBox();
                tb.BorderBrush = new SolidColorBrush(Colors.Transparent);
                tb.Style = this.Resources["TextBoxWithNoMouseOverBorder"] as Style;
                
                //use data binding to keep the labe in sync with the model
                Binding textBinding = new Binding("Label")
                {
                    Source = ItemSource[row],
                    Mode = BindingMode.TwoWay
                };
                tb.SetBinding(TextBox.TextProperty, textBinding);
                tb.TextChanged += new TextChangedEventHandler(LabelText_Changed);
                tb.KeyDown += new KeyEventHandler(TextBox_KeyDown);
                tb.GotFocus += new RoutedEventHandler(LabelText_GotFocus);
                return tb;
            }
        }

        /// <summary>
        /// This method highlights the currently selected row in the chemical stream 
        /// property window
        /// </summary>
        private void highlightRow(Brush brush, int index)
        {
            if (index >=0)
            {
                //highlight the label textbox in the selected row
                TextBox tb = PropertiesGrid.GetUIElementAt(1, index + 1) as TextBox;
                tb.Background = brush;
                //highlight the qty textbox in the selected row
                tb = PropertiesGrid.GetUIElementAt(2, index + 1) as TextBox;
                tb.Background = brush;
                //highlight the units combobox in the selected row
                ComboBox cb = PropertiesGrid.GetUIElementAt(3, index + 1) as ComboBox;
                cb.Background = brush;
                cb.BorderBrush = brush; //combobox border highlighted to make more visible

                //highlight the compound column in the selected row
                if (index == 0) //the first row column 4 is a textblock
                {
                    TextBlock tbk = PropertiesGrid.GetUIElementAt(4, index + 1) as TextBlock;
                    //tbk.Foreground = brush;
                    tbk.FontWeight = FontWeights.ExtraBold;
                }
                else // otherwise the 4th column is a combobox
                {
                    cb = PropertiesGrid.GetUIElementAt(4, index + 1) as ComboBox;
                    cb.Background = brush;
                    cb.BorderBrush = brush; //combobox border highlighted to make more visible
                }
            }
        }

        void LabelText_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;            

            int tbIndex = -1;

            //find the index value of the selected row
            for (int i = 0; i < ItemSource.Count; i++) 
            {
                if(tb.Text.Equals(ItemSource[i].Label))
                {
                    tbIndex = i;
                    break;
                }
            }
            
            highlightRow(highlightFeedbackBrush, tbIndex);
            
        }

        private void LabelText_Changed(object sender, TextChangedEventArgs e)
        {
            TableDataChanging(this, EventArgs.Empty);
        }

        private UIElement CreateQuantityCell(int row)
        {
            if (isReadOnly)
            {
                TextBlock tb = new TextBlock();
                tb.Text = ItemSource[row].Quantity;
                return tb;
            }
            else
            {
                TextBox tb = new TextBox();
                tb.BorderBrush = new SolidColorBrush(Colors.Transparent);
                tb.Style = this.Resources["TextBoxWithNoMouseOverBorder"] as Style;
                tb.Text = ItemSource[row].Quantity;
                if (!isReadOnly)
                {
                    tb.LostFocus += new RoutedEventHandler(QuantityTextBox_LostFocus);
                    tb.GotFocus += new RoutedEventHandler(QuantityTextBox_GotFocus);
                    tb.TextChanged += new TextChangedEventHandler(QuantityTextBox_TextChanged);
                    tb.KeyDown += new KeyEventHandler(TextBox_KeyDown);
                }
                return tb;
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TableDataChanging(this, EventArgs.Empty);
        }

        protected void QuantityTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            double value;
            TextBox tb = (sender as TextBox);
            if (double.TryParse(tb.Text, out value) == false)
            {
                tb.Text = "";
                TableDataChanging(this, EventArgs.Empty);
            }
        }

        protected void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.LostFocus -= new RoutedEventHandler(QuantityTextBox_LostFocus);
            try
            {
                double qty;
                //make sure the quantity is numeric to avoid format exception error
                bool isNum = double.TryParse(tb.Text, out qty);

                if (isNum)
                {
                    if(tb.Parent != null) //handles the case where the textbox parent is null
                        ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = qty.ToString();
                }
                else
                {
                    if (tb.Parent != null) //handles the case where the textbox parent is null
                        ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = "?";
                }
            }
            catch
            {
                try
                {
                    int i = tb.Text.IndexOf('/');
                    double numerator = int.Parse(tb.Text.Substring(0, i));
                    double denominator = int.Parse(tb.Text.Substring(i + 1));
                    ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = (Math.Round(numerator / denominator, 4) * 100).ToString();
                }
                catch
                {
                    tb.Text = "?";
                    try
                    {
                        ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = "?";
                    }
                    catch (System.Exception ex)
                    {
                    }
                }
            }
            UpdateGrid();
        }

        private UIElement CreateUnitsCell(int row)
        {
            if (isReadOnly)
            {
                TextBlock tb = new TextBlock();
                tb.Text = ItemSource[row].Unit.ToPrettyString();
                return tb;
            }
            else
            {
                ComboBox cb = new ComboBox();
                cb.Background = new SolidColorBrush(Colors.White);
                cb.BorderBrush = new SolidColorBrush(Colors.White);
                foreach (ChemicalUnits unit in Enum.GetValues(typeof(ChemicalUnits)))
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = unit.ToPrettyString();
                    cb.Items.Add(cbi);
                }
                if (row == 0)
                {
                    //Overall Units cannot be % so if row 0 remove first element which is %
                    cb.Items.RemoveAt(0);
                }

                //use data binding to keep the units in sync with the model
                Binding indexBinding = new Binding("Units")
                {
                    Source = ItemSource[row],
                    Mode = BindingMode.TwoWay
                };
                cb.SetBinding(ComboBox.SelectedIndexProperty, indexBinding);

                //we only care about changes between made on the "overall" row, which should be row 1
                if (row == 0)
                {
                    ItemSource[row].PropertyChanged += new PropertyChangedEventHandler(HeaderRowUnitsChanged);
                }
                return cb;
            }
        }

        private void HeaderRowUnitsChanged(object sender, PropertyChangedEventArgs e)
        {
            //Check to see if we're using moles.  Subtracting 1 because the header doesn't contain the % option
            if (ItemSource[0].Unit == ChemicalUnits.Moles - 1 || ItemSource[0].Unit == ChemicalUnits.MolesPerSecond - 1)
            {
                ConvertModelLabels("^([mM])(\\d+)$", moleTablePrefix);                
            }
            else
            {
                //switch to mass labels
                ConvertModelLabels("^([nN])(\\d+)$", massTablePrefix);                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">Assuming that the pattern will return three parts: 1 = overall match (m12), 2 = type match (m/n), 3 = numeric match (12)</param>
        /// <param name="newLabel"></param>
        private void ConvertModelLabels(string pattern, string newLabel)
        {
            //switching to mole lables.  Change table name if left to default
            Match match = Regex.Match(TableName, pattern);
            if (match.Success)
            {
                TableName = newLabel.ToUpper() + match.Groups[2].Value;
            }

            //Overwrite anything with a defalut mass label (ex: "M1", "m11", etc.)
            foreach (ChemicalStreamData data in ItemSource)
            {
                match = Regex.Match(data.Label, pattern);
                if (match.Success)
                {
                    char prefix = Convert.ToChar(match.Groups[1].Value);
                    if (char.IsLower(prefix))
                    {
                        data.Label = newLabel.ToLower() + match.Groups[2].Value;
                    }
                    else
                    {
                        data.Label = newLabel.ToUpper() + match.Groups[2].Value;
                    }
                }
            }
        }

        private UIElement CreateCompoundCell(int row)
        {
            if (row == 0)
            {
                TextBlock txtBlk = new TextBlock();
                txtBlk.Text = "Overall";
                txtBlk.HorizontalAlignment = HorizontalAlignment.Center;
                txtBlk.VerticalAlignment = VerticalAlignment.Center;
                return txtBlk;
            }
            else
            {
                if (isReadOnly)
                {
                    TextBlock tb = new TextBlock();
                    if (ItemSource[row].CompoundId == -1)
                    {
                        tb.Text = "";
                    }
                    else
                    {
                        tb.Text = ItemSource[row].Compound.ToPrettyString();
                    }
                    return tb;
                }
                else
                {
                    ComboBox cb = new ComboBox();
                    cb.Background = new SolidColorBrush(Colors.White);
                    cb.BorderBrush = new SolidColorBrush(Colors.White);
                    foreach (ChemicalCompounds compound in Enum.GetValues(typeof(ChemicalCompounds)))
                    {
                        ComboBoxItem cbi = new ComboBoxItem();
                        cbi.Content = compound.ToPrettyString();
                        cb.Items.Add(cbi);
                    }

                    cb.SelectedIndex = ItemSource[row].CompoundId;

                    //this is a bit of hack since when compound is not set we want
                    //it to say Select but we dont want Select to in the options list
                    if (cb.SelectedIndex == -1)
                    {
                        ComboBoxItem cbi = new ComboBoxItem();
                        cbi.Content = "Select";
                        cb.Items.Add(cbi);
                        cb.SelectedIndex = cb.Items.Count - 1;
                        if (!isReadOnly)
                        {
                            cb.DropDownOpened += new EventHandler(CompoundComboBox_DropDownOpened);
                        }
                    }
                    cb.SelectionChanged += new SelectionChangedEventHandler(CompoundComboBox_SelectionChanged);

                    return cb;
                }
            }
        }

        private void CompoundComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = (sender as ComboBox);
            cb.DropDownOpened -= new EventHandler(CompoundComboBox_DropDownOpened);
            cb.Items.RemoveAt(cb.Items.Count - 1);
            cb.SelectedIndex = 0;
        }

        private void CompoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //This fucntion assumes this is true:
            //---Grid (baseGrid)
            //----Border which PlaceUIElement puts around it
            //-----ComboBox
            Border border = (sender as ComboBox).Parent as Border;

            //minus one to get rid of the header row in the count
            int row = (int)border.GetValue(Grid.RowProperty) - 1;

            ItemSource[row].CompoundId = (sender as ComboBox).SelectedIndex;
        }

        private void CollapsedView()
        {
            PropertiesGrid.ClearAll();

            PropertiesGrid.PlaceUIElement(new Label() { Content = ItemSource[0].Label }, 0, 0);

            Button ToggleViewButton = new Button();
            ToggleViewButton.Style = this.Resources["SquareButton"] as Style;
            GradientStopCollection gsc = new GradientStopCollection();
            GradientStop gs = new GradientStop();
            gs.Color = Color.FromArgb(225, 200, 207, 230);
            gs.Offset = 1;
            gsc.Add(gs);
            gs = new GradientStop();
            gs.Color = Color.FromArgb(255, 225, 235, 250);
            gs.Offset = 0.0;
            gsc.Add(gs);

            ToggleViewButton.Background = new LinearGradientBrush(gsc, 90);
            ToggleViewButton.Content = ">";
            ToggleViewButton.Click += new RoutedEventHandler(ToggleView);
            System.Windows.Controls.ToolTip tp = new System.Windows.Controls.ToolTip();
            tp.Content = "Click To Toggle Between The Collapsed View And The Expanded View";
            System.Windows.Controls.ToolTipService.SetToolTip(ToggleViewButton, tp);

            PropertiesGrid.PlaceUIElement(ToggleViewButton, 1, 0);
            /*
            PropertiesGrid.HideBordersForLastRow = false;

            createHeaderRow(true);

            int row = 0;
            foreach (ChemicalStreamData data in ItemSource)
            {
                createDataRow(true, data, row, row == ItemSource.Count - 1);
                row++;
            }
            */
            setFeedBack();

            expandedViewSavedLocation.X = (double)this.GetValue(Canvas.LeftProperty);
            expandedViewSavedLocation.Y = (double)this.GetValue(Canvas.TopProperty);

            if (parentStream != null)
            {
                //we call update stream beacuse the stream will determine our location because we are attached to it now
                parentStream.UpdateStreamLocation();
            }
        }

        private Brush basicRadialGradientBrush(Color CenterColor, Color OutsideColor)
        {
            RadialGradientBrush brush = new RadialGradientBrush();
            GradientStopCollection gsc = new GradientStopCollection();
            GradientStop gs = new GradientStop();
            brush = new RadialGradientBrush();
            gsc = new GradientStopCollection();
            gs = new GradientStop();
            gs.Color = CenterColor;
            gs.Offset = .025;
            gsc.Add(gs);
            gs = new GradientStop();
            gs.Color = OutsideColor;
            gs.Offset = 1;
            gsc.Add(gs);
            brush.GradientStops = gsc;
            return brush;
        }

        private void createButtons(int row, bool lastRow)
        {
            System.Windows.Controls.ToolTip tp = new Silverlight.Controls.ToolTip();
            if (row != 0)
            {
                Button minusButton = new Button();
                //ToggleViewButton.Style = this.Resources["RoundButton"] as Style;
                //ToggleViewButton.Background = basicRadialGradientBrush(Colors.White, Colors.Red);
                TextBlock tb = new TextBlock();
                tb.Text = "-";
                tb.TextAlignment = TextAlignment.Center;
                minusButton.Content = "-";
                minusButton.Height = 15;
                minusButton.Width = 15;
                minusButton.FontSize = 6;
                minusButton.Click += new RoutedEventHandler(MinusRowButton_Click);
                tp.Content = "Click To Delete This Row";
                System.Windows.Controls.ToolTipService.SetToolTip(minusButton, tp);
                PropertiesGrid.PlaceUIElement(minusButton, 0, row + 1);

                if (lastRow)
                {
                    Button plusButton = new Button();
                    //ToggleViewButton.Style = this.Resources["RoundButton"] as Style;
                    tb = new TextBlock();
                    tb.Text = "+";
                    tb.TextAlignment = TextAlignment.Center;
                    plusButton.Content = "+";
                    plusButton.Height = 15;
                    plusButton.Width = 15;
                    plusButton.FontSize = 6;
                    plusButton.Click += new RoutedEventHandler(PlusRowButton_Click);
                    tp = new Silverlight.Controls.ToolTip();
                    tp.Content = "Click To Add A New Row";
                    System.Windows.Controls.ToolTipService.SetToolTip(plusButton, tp);
                    PropertiesGrid.PlaceUIElement(plusButton, 0, row + 2);
                }
            }
        }

        private void MinusRowButton_Click(object sender, RoutedEventArgs e)
        {
            //This fucntion assumes this is true:
            //---Grid (baseGrid)
            //----Border which PlaceUIElement puts around it
            //-----The ToggleViewButton itself
            Border border = (sender as Button).Parent as Border;

            //minus one to get rid of the header row in the count
            int row = (int)border.GetValue(Grid.RowProperty) - 1;

            if (ItemSource.Count > 2)
            {
                ItemSource.RemoveAt(row);
                TableDataChanged(this, EventArgs.Empty as TableDataChangedEventArgs);
            }
            UpdateGrid();
        }

        private void PlusRowButton_Click(object sender, RoutedEventArgs e)
        {
            ItemSource.Add(CreateNewDataRow());
            UpdateGrid();
        }

        public void UpdateGrid()
        {
            //This ensures that any changes are saved before we reset the table
            this.Focus();

            if (view == View.Collapsed)
            {
                CollapsedView();
            }
            else
            {
                ExpandedView();
            }
        }

        private void ToggleView(object sender, RoutedEventArgs e)
        {
            if (View == PropertiesWindow.View.Collapsed)
            {
                View = PropertiesWindow.View.Expanded;
            }
            else
            {
                View = PropertiesWindow.View.Collapsed;
            }
        }

        private void LocalInit(bool isReadOnly)
        {
            this.isReadOnly = isReadOnly;
            
            //set header bush
            headerBrush = new LinearGradientBrush();
            GradientStopCollection gsc = new GradientStopCollection();
            GradientStop gs = new GradientStop();
            gs.Color = Color.FromArgb(225, 210, 215, 222);
            gs.Offset = 0;
            gsc.Add(gs);
            gs = new GradientStop();
            gs.Offset = .5;
            gs.Color = Color.FromArgb(255, 230, 230, 235);
            gsc.Add(gs);
            gs = new GradientStop();
            gs.Color = Color.FromArgb(225, 210, 215, 222);
            gs.Offset = 1;
            gsc.Add(gs);
            headerBrush.StartPoint = new Point(0.5, 0);
            headerBrush.EndPoint = new Point(0.5, 1);
            headerBrush.GradientStops = gsc;

            SolidColorBrush lightGray = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));

            LayoutRoot.MouseRightButtonDown += new MouseButtonEventHandler(ChemicalStreamPropertiesWindowUserControl_MouseRightButtonDown);
            LayoutRoot.MouseRightButtonUp += new MouseButtonEventHandler(ChemicalStreamPropertiesWindowUserControl_MouseRightButtonUp);
            this.LayoutUpdated += new EventHandler(ChemicalStreamPropertiesWindow_LayoutUpdated);

            PropertiesGrid.BorderBrush = lightGray;

            this.LayoutRoot.Children.Add(PropertiesGrid.BaseGrid);

            //Set this table's name
            TableName = getNextAvailableTableName();

            //Create bindings that listen for changes in the object's location
            SetBinding(Canvas.LeftProperty, new Binding("LeftProperty") { Source = this, Mode = BindingMode.TwoWay });
            SetBinding(Canvas.TopProperty, new Binding("TopProperty") { Source = this, Mode = BindingMode.TwoWay });

            //create the header row
            ItemSource.Add(CreateTableHeader());
            ItemSource.Add(CreateNewDataRow());

            ItemSource.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemSource_CollectionChanged);

            View = PropertiesWindow.View.Expanded;
        }

        private void ChemicalStreamPropertiesWindow_LayoutUpdated(object sender, EventArgs e)
        {
            if (parentStream != null)
            {
                parentStream.UpdateStreamLocation();
            }
        }

        private void ItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateGrid();
        }

        /// <summary>
        /// Resets the table counter back to the initial state.  Used when creating a new file
        /// </summary>
        public static void ResetTableCounter()
        {
            ChemicalStreamPropertiesWindow.NumberOfTables = 1;
        }

        /// <summary>
        /// Generates a table name upon request
        /// </summary>
        /// <returns>A table name</returns>
        protected string getNextAvailableTableName()
        {
            String name = String.Format("{0}{1}", "M", NumberOfTables);
            NumberOfTables++;
            return name;
        }

        /// <summary>
        /// Generates a row name upon request
        /// </summary>
        /// <returns></returns>
        public string getNextAvailableRowName()
        {
            string name = String.Format("{0}{1}", TableName.ToLower(), ItemSource.Count);
            return name;
        }

        /// <summary>
        /// Builds a header row for the properties table.  Should only be
        /// called once per instance of IPropertiesWindow
        /// </summary>
        /// <returns></returns>
        private ChemicalStreamData CreateTableHeader()
        {
            ChemicalStreamData d = new ChemicalStreamData();
            d.Label = this.TableName;
            d.Quantity = "?";
            d.UnitId = 0;
            d.CompoundId = 25;
            d.Temperature = 'T' + this.TableName;
            d.PropertyChanged += new PropertyChangedEventHandler(DataUpdated);
            return d;
        }

        /// <summary>
        /// Creates a new data row for the properties table
        /// </summary>
        /// <returns></returns>
        private ChemicalStreamData CreateNewDataRow()
        {
            ChemicalStreamData d = new ChemicalStreamData();
            d.Label = this.getNextAvailableRowName();
            d.Quantity = "?";
            d.UnitId = 0;
            d.CompoundId = -1;
            d.TempUnits = -1;
            d.Temperature = "";
            d.PropertyChanged += new PropertyChangedEventHandler(DataUpdated);
            return d;
        }

        /// <summary>
        /// Called whenever the underlying data gets updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DataUpdated(object sender, PropertyChangedEventArgs e)
        {
            //check to see if we need a new row
            ChemicalStreamData data = sender as ChemicalStreamData;

            //only propigate if not a tooltip of feedback message
            if (e.PropertyName.CompareTo("Feedback") != 0 && e.PropertyName.CompareTo("ToolTipMessage") != 0)
            {
                //tell interested parties that our data has changed.
                TableDataChanged(this, new TableDataChangedEventArgs(sender, e.PropertyName));
            }
        }

        /// <summary>
        /// Uber-hack used to track changes in the process unit's position
        /// and the table only ever reports its expandedViewLocation never its collapsed ViewLocation.
        /// Should not be called directly.  Instead, use Canvas.LeftProperty.
        /// </summary>
        public Double LeftProperty
        {
            get
            {
                return ExpandedViewSavedLocation.X;
            }
            set
            {
                expandedViewSavedLocation.X = LeftProperty;
                LocationChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Uber-hack used to track changes in the process unit's position
        /// and the table only ever reports its expandedViewLocation never its collapsed ViewLocation.
        /// Should not be called directly.  Instead, use Canvas.LeftProperty.
        /// </summary>
        public Double TopProperty
        {
            get
            {
                return ExpandedViewSavedLocation.Y;
            }
            set
            {
                expandedViewSavedLocation.Y = TopProperty;
                LocationChanged(this, new EventArgs());
            }
        }

        public int NumberOfRows
        {
            get
            {
                return ItemSource.Count;
            }
        }

        private bool selected;

        /// <summary>
        ///
        /// </summary>
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        /// <summary>
        /// Gets or sets the table's name.
        /// </summary>
        public string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                tableName = value;
            }
        }

        /// <summary>
        /// Gets or sets the Table's unique ID number.  A wrapper for the already-existing
        /// TableName variable.  Implemented as a requirement of the IPfdElement interface
        /// </summary>
        public String Id
        {
            get
            {
                return TableName;
            }
            set
            {
                TableName = value;
            }
        }

        public IStream ParentStream
        {
            get
            {
                return parentStream;
            }
            set
            {
                parentStream = value;
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            //serializer for our data class
            XmlSerializer dataSerializer = new XmlSerializer(typeof(ChemicalStreamData));

            //reference to our parent stream
            writer.WriteElementString("ParentStream", ParentStream.Id);

            //property table data
            writer.WriteStartElement("DataRows");
            foreach (object dataRow in ItemSource)
            {
                dataSerializer.Serialize(writer, dataRow);
            }
            writer.WriteEndElement();

            //the property table's location
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", expandedViewSavedLocation.X.ToString());
            writer.WriteElementString("Y", expandedViewSavedLocation.Y.ToString());
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members

        /// <summary>
        /// This is called each time a label is loaded into the datagrid.  This is so we can get a reference to the label so that
        /// we can set its tooltip using the "advanced tooltip" found online.  It cannot be done in xmal which is why we are using
        /// this function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void feedbackTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Label lb = sender as Label;
            Silverlight.Controls.ToolTip tooltip = new Silverlight.Controls.ToolTip();

            //this sets the intial time to 1 second
            tooltip.InitialDelay = new Duration(new TimeSpan(0, 0, 1));

            //this sets the displayTime to 1 hour
            tooltip.DisplayTime = new Duration(new TimeSpan(1, 0, 0));

            //this attached it to the label
            Silverlight.Controls.ToolTipService.SetToolTip(lb, tooltip);
        }

        public int CompareTo(object obj)
        {
            //make sure that we're comparing two table elements
            if (!(obj is ChemicalStreamPropertiesWindow))
            {
                return -1;
            }
            else
            {
                ChemicalStreamPropertiesWindow other = obj as ChemicalStreamPropertiesWindow;
                return TableName.CompareTo(other.TableName);
            }
        }

        private void feedbackTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label lb = sender as Label;

            Silverlight.Controls.ToolTip oldTooptip = Silverlight.Controls.ToolTipService.GetToolTip(lb);

            oldTooptip.IsOpen = false;
        }

        public void HighlightFeedback(bool highlight)
        {
            if (highlight)
            {
                feedbackLabel.Background = highlightFeedbackBrush;
            }
            else
            {
                if (View == PropertiesWindow.View.Expanded)
                {
                    if (feedbackLabel != null)
                    {
                        feedbackLabel.Background = new SolidColorBrush(Colors.White);
                    }
                }
                else
                {
                    if (feedbackLabel != null)
                    {
                        feedbackLabel.Background = new SolidColorBrush(Colors.Red);
                    }
                }
            }
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            //first put into the propertiesWindowData
            if (ItemSource[0].Feedback == "")
            {
                ItemSource[0].Feedback = "[" + errorNumber + "]";
                ItemSource[0].ToolTipMessage = feedbackMessage;
            }
            else
            {
                ItemSource[0].Feedback = ItemSource[0].Feedback.Remove(ItemSource[0].Feedback.Length - 1) + "," + errorNumber + "]";
                ItemSource[0].ToolTipMessage += feedbackMessage;
            }

            //then set where it will be according to the view
            if (View == View.Expanded)
            {
                if (feedbackLabel == null)
                {
                    feedbackLabel = new Label();
                }

                if (feedbackLabel.Parent == null)
                {
                    int column = 5;
                    int row = 1;
                    Label feedbackheader = new Label();
                    feedbackheader.Background = headerBrush;
                    PropertiesGrid.PlaceUIElement(feedbackLabel, column, row);
                    PropertiesGrid.PlaceUIElement(feedbackheader, column, 0);
                }

                feedbackLabel.Content = ItemSource[0].Feedback;
            }
            else
            {
                feedbackLabel = PropertiesGrid.GetUIElementAt(0, 0) as Label;
                feedbackLabel.Background = new SolidColorBrush(Colors.Red);
            }

            //then create the tooltip we will use
            feedbackToolTip = new Silverlight.Controls.ToolTip();
            Silverlight.Controls.ToolTipService.SetToolTip(feedbackLabel, feedbackToolTip);

            feedbackToolTip.Content = ItemSource[0].ToolTipMessage;
        }

        public void RemoveFeedback()
        {
            ItemSource[0].Feedback = "";
            ItemSource[0].ToolTipMessage = "";
            if (View == PropertiesWindow.View.Expanded)
            {
                this.PropertiesGrid.RemoveUIElement(feedbackLabel);
            }
            else
            {
                if (feedbackLabel != null)
                {
                    feedbackLabel.Background = new SolidColorBrush(Colors.White);
                }
            }
            feedbackLabel = null;
            feedbackToolTip = null;
        }

        private void ChemicalStreamPropertiesWindowUserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TO DO: add right click menu for the Window
            e.Handled = true;
        }

        private void ChemicalStreamPropertiesWindowUserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// This fires the TableDataChanging Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void ChangeInProgress(IPropertiesWindow sender, EventArgs args)
        {
            TableDataChanging(sender, args);
        }
    }
}