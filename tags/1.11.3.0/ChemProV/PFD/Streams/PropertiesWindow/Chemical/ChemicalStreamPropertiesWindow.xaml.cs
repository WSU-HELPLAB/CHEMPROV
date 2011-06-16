/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Silverlight.Controls;

using ChemProV.Validation;
using ChemProV.UI.DrawingCanvas;

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{

    public partial class ChemicalStreamPropertiesWindow : UserControl, IPropertiesWindow, IComparable
    {
        public event EventHandler SelectionChanged = delegate { };
        public event EventHandler LocationChanged = delegate { };
        public event TableDataEventHandler TableDataChanged = delegate { };

        /// <summary>
        /// Need to come up with a better way to impliment these.
        /// </summary>
        private static string[] Compunds = { "acetic acid", "ammonia", "benzene", "carbon dioxide", 
                                    "carbon monoxide", "cyclohexane", "ethane", "ethanol", 
                                    "ethylene", "hydrochloric acid", "hydrogen", "methane", 
                                    "methanol", "n-butane", "n-hexane", "n-octane", "nitrogen", 
                                    "oxygen", "phosphoric acid", "propane", "sodium hydroxide", 
                                    "sulfuric acid", "toluene", "water" };

        private static string[] Units = {   
                                    "%", "grams", "grams per second", 
                                    "kilograms", "kilograms per second", "moles", 
                                    "moles per second"
                                };

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
        private string tableName = "M";

        /// <summary>
        /// This a list of Data which is exactly what is in the table
        /// </summary>
        public ObservableCollection<ChemicalStreamData> ItemSource = new ObservableCollection<ChemicalStreamData>();

        //this is a reference to the grid we are using
        protected CustomDataGrid PropertiesGrid = new CustomDataGrid();

        private View view = View.Expanded;

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
            : base()
        {
            InitializeComponent();
            LocalInit();
        }

        public ChemicalStreamPropertiesWindow(IStream parent)
            : base()
        {
            InitializeComponent();
            ParentStream = parent;
            LocalInit();
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
        }

        /// <summary>
        /// This function is called whenver the view is changed and it handles the feedback label and tooltip
        /// </summary>
        private void setFeedBack()
        {
            if (feedbackLabel != null)
            {
                Label lb = new Label();
                lb.Content = feedbackLabel.Content;
                lb.Background = feedbackLabel.Background;
                feedbackLabel = lb;
                Silverlight.Controls.ToolTipService.SetToolTip(lb, feedbackToolTip);
                if (view == View.Expanded)
                {
                    PropertiesGrid.PlaceUIElement(feedbackLabel, 5, 1);
                }
                else
                {
                    PropertiesGrid.PlaceUIElement(feedbackLabel, 3, 1);
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
            if(feedbackLabel != null)
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

            //create buttons and place them on the grid
            createButtons(row, lastRow);


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
            TextBox tb = new TextBox();
            tb.BorderBrush = new SolidColorBrush(Colors.Transparent);
            tb.Text = ItemSource[row].Label;
            tb.TextChanged += new TextChangedEventHandler(LabelText_Changed);
            tb.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            return tb;
        }

        void LabelText_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Border br = tb.Parent as Border;

            //-1 one to get rid of the header row count
            int row = (int)br.GetValue(Grid.RowProperty) - 1;
            tb.LostFocus += new RoutedEventHandler(LabelTextBox_LostFocus);
        }

        private UIElement CreateQuantityCell(int row)
        {
            TextBox tb = new TextBox();
            tb.BorderBrush = new SolidColorBrush(Colors.Transparent);
            tb.Text = ItemSource[row].Quantity;
            tb.LostFocus += new RoutedEventHandler(QuantityTextBox_LostFocus);
            tb.GotFocus += new RoutedEventHandler(tb_GotFocus);
            tb.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            return tb;
        }

        protected void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            double value;
            TextBox tb = (sender as TextBox);
            if (double.TryParse(tb.Text, out value) == false)
            {
                tb.Text = "";
            }
        }

        protected void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateGrid();
            }
        }

        bool validLabel(string text)
        {
            if (text.Length < 2 || text[0] == 'T' || text[0].ToString() + text[1] == "Hf" || text[0].ToString() + text[1] == "Cp" || text.Contains(' '))
            {
                return false;
            }
            return true;
        }

        void LabelTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            Border br = tb.Parent as Border;
            tb.LostFocus -= new RoutedEventHandler(LabelTextBox_LostFocus);

            //-1 one to get rid of the header row count
            int row = (int)br.GetValue(Grid.RowProperty) - 1;
                  
            if (validLabel(tb.Text))
            {
                tb.Text = ItemSource[row].Label = tb.Text;
            }
            else
            {
                ItemSource[row].Label = tb.Text;
            }
            UpdateGrid();
        }

        void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.LostFocus -= new RoutedEventHandler(QuantityTextBox_LostFocus);
            try
            {
                double qty = double.Parse(tb.Text);
                ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = qty.ToString();
            }
            catch
            {
                try
                {
                    int i = tb.Text.IndexOf('/');
                    double numerator = int.Parse(tb.Text.Substring(0, i));
                    double denominator = int.Parse(tb.Text.Substring(i + 1));
                    ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = (numerator/denominator).ToString();
                }
                catch
                {
                    tb.Text = "?";
                    ItemSource[(int)(tb.Parent).GetValue(Grid.RowProperty) - 1].Quantity = "?";
                }
            }
            UpdateGrid();
        }

        private UIElement CreateUnitsCell(int row)
        {
            ComboBox cb = new ComboBox();
            cb.Background = new SolidColorBrush(Colors.White);
            cb.BorderBrush = new SolidColorBrush(Colors.White);
            foreach (string s in Units)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = s;
                cb.Items.Add(cbi);
            }
            if (row == 0)
            {
                //Overall Units cannot be % so if row 0 remove first element which is %
                cb.Items.RemoveAt(0);
            }
            cb.SelectedIndex = ItemSource[row].Units;
            cb.SelectionChanged += new SelectionChangedEventHandler(UnitsComboBox_SelectionChanged);

            return cb;
        }

        private void UnitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //This fucntion assumes this is true:
            //---Grid (baseGrid)
            //----Border which PlaceUIElement puts around it
            //-----ComboBox
            Border border = (sender as ComboBox).Parent as Border;

            //minus one to get rid of the header row in the count
            int row = (int)border.GetValue(Grid.RowProperty) - 1;

            ItemSource[row].Units = (sender as ComboBox).SelectedIndex;
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
                ComboBox cb = new ComboBox();
                cb.Background = new SolidColorBrush(Colors.White);
                cb.BorderBrush = new SolidColorBrush(Colors.White);
                foreach (string s in Compunds)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = s;
                    cb.Items.Add(cbi);
                }

                cb.SelectedIndex = ItemSource[row].Compound;
                
                //this is a bit of hack since when compound is not set we want
                //it to say Select but we dont want Select to in the options list
                if (cb.SelectedIndex == -1)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = "Select";
                    cb.Items.Add(cbi);
                    cb.SelectedIndex = cb.Items.Count - 1;
                    cb.DropDownOpened += new EventHandler(CompoundComboBox_DropDownOpened);
                }
                cb.SelectionChanged += new SelectionChangedEventHandler(CompoundComboBox_SelectionChanged);
                return cb;
            }
        }

        void CompoundComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = (sender as ComboBox);
            cb.DropDownOpened -= new EventHandler(CompoundComboBox_DropDownOpened);
            cb.Items.RemoveAt(cb.Items.Count -1);
            cb.SelectedIndex = 0;
        }

        void CompoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //This fucntion assumes this is true:
            //---Grid (baseGrid)
            //----Border which PlaceUIElement puts around it
            //-----ComboBox
            Border border = (sender as ComboBox).Parent as Border;

            //minus one to get rid of the header row in the count
            int row = (int)border.GetValue(Grid.RowProperty) - 1;

            ItemSource[row].Compound = (sender as ComboBox).SelectedIndex;
        }

        private void CollapsedView()
        {
            PropertiesGrid.ClearAll();

            PropertiesGrid.PlaceUIElement(new TextBlock(){Text = ItemSource[0].Label}, 0, 0);

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

            setFeedBack();
             */
            if (parentStream != null)
            {
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

        private void LocalInit()
        {
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

            ChemicalStreamPropertiesWindowUserControl.MouseRightButtonDown += new MouseButtonEventHandler(ChemicalStreamPropertiesWindowUserControl_MouseRightButtonDown);
            ChemicalStreamPropertiesWindowUserControl.MouseRightButtonUp += new MouseButtonEventHandler(ChemicalStreamPropertiesWindowUserControl_MouseRightButtonUp);
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

        void ChemicalStreamPropertiesWindow_LayoutUpdated(object sender, EventArgs e)
        {
            if (parentStream != null)
            {
                parentStream.UpdateStreamLocation();
            }
        }

        void ItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
            d.Units = 0;
            d.Compound = 25;
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
            d.Units = 0;
            d.Compound = -1;
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
        /// Uber-hack used to track changes in the process unit's position.
        /// Should not be called directly.  Instead, use Canvas.LeftProperty.
        /// </summary>
        public Double LeftProperty
        {
            get
            {
                return Convert.ToDouble(GetValue(Canvas.LeftProperty));
            }
            set
            {
                LocationChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Uber-hack used to track changes in the process unit's position.
        /// Should not be called directly.  Instead, use Canvas.LeftProperty.
        /// </summary>
        public Double TopProperty
        {
            get
            {
                return Convert.ToDouble(GetValue(Canvas.TopProperty));
            }
            set
            {
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

        public void WriteXml(System.Xml.XmlWriter writer)
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
            writer.WriteElementString("X", GetValue(Canvas.LeftProperty).ToString());
            writer.WriteElementString("Y", GetValue(Canvas.TopProperty).ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Temperature");
            writer.WriteElementString("Quantity", ItemSource[0].Temperature);
            writer.WriteElementString("Units", ItemSource[0].TempUnits.ToString());
            writer.WriteEndElement();
        }

        #endregion

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
                feedbackLabel.Background = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                feedbackLabel.Background = new SolidColorBrush(Colors.White);
            }
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            if (feedbackLabel == null)
            {
                feedbackLabel = new Label();
                feedbackToolTip = new Silverlight.Controls.ToolTip();
                Silverlight.Controls.ToolTipService.SetToolTip(feedbackLabel, feedbackToolTip);
            }
            if (ItemSource[0].Feedback == "")
            {
                ItemSource[0].Feedback = "[" + errorNumber + "]";
                ItemSource[0].ToolTipMessage = feedbackMessage;
            }
            else
            {
                ItemSource[0].Feedback = "[+]";
                ItemSource[0].ToolTipMessage += feedbackMessage;
            }
            feedbackLabel.Content = ItemSource[0].Feedback;
            feedbackToolTip.Content = ItemSource[0].ToolTipMessage;

            if (feedbackLabel.Parent == null)
            {
                int column;
                int row = 1;
                if (view == View.Collapsed)
                {
                    column = 3;
                }
                else
                {
                    column = 5;
                }
                Label feedbackheader = new Label();
                feedbackheader.Background = headerBrush;
                PropertiesGrid.PlaceUIElement(feedbackLabel, column, row);
                PropertiesGrid.PlaceUIElement(feedbackheader, column, 0);
            }
        }

        public void RemoveFeedback()
        {
            ItemSource[0].Feedback = "";
            ItemSource[0].ToolTipMessage = "";
            this.PropertiesGrid.RemoveUIElement(feedbackLabel);
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
    }
}
