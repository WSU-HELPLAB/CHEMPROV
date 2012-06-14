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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ChemProV.PFD.Streams.PropertiesWindow.Heat
{
    public partial class HeatStreamPropertiesWindow : UserControl, IPropertiesWindow, IComparable
    {
        public event EventHandler SelectionChanged = delegate { };
        public event EventHandler LocationChanged = delegate { };
        public event TableDataEventHandler TableDataChanged = delegate { };
        public event EventHandler TableDataChanging = delegate { };

        //reference to the parent stream
        private IStream parentStream = null;

        //Keeps track of the number of Tables that have been made
        protected static int NumberOfTables = 1;

        //this keeps the record of what table number the table is when it is created
        private string tableName = "Q";

        private LinearGradientBrush headerBrush;

        private static string[] EnergyUnits = { "BTU", "BTU/sec", "J", "W" };

        public ObservableCollection<HeatStreamData> ItemSource = new ObservableCollection<HeatStreamData>();

        private CustomDataGrid PropertiesGrid = new CustomDataGrid();

        private bool isReadOnly = false;

        public HeatStreamPropertiesWindow()
        {
            InitializeComponent();
            LocalInit();
        }

        public HeatStreamPropertiesWindow(bool isReadOnly)
        {
            InitializeComponent();
            LocalInit(isReadOnly);
        }

        public HeatStreamPropertiesWindow(IStream parent, bool isReadOnly)
        {
            InitializeComponent();
            ParentStream = parent;
            LocalInit(isReadOnly);
        }

        private void LocalInit(bool isReadOnly = false)
        {
            this.isReadOnly = isReadOnly;
            //Set this table's name
            TableName = getNextAvailableTableName();

            //Create bindings that listen for changes in the object's location
            SetBinding(Canvas.LeftProperty, new Binding("LeftProperty") { Source = this, Mode = BindingMode.TwoWay });
            SetBinding(Canvas.TopProperty, new Binding("TopProperty") { Source = this, Mode = BindingMode.TwoWay });

            //create the header row
            ItemSource.Add(CreateTableHeader());

            LayoutRoot.Children.Add(PropertiesGrid.BaseGrid);

            SetPropertiesGrid();

            ItemSource.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemSource_CollectionChanged);

            LayoutRoot.MouseRightButtonDown += new MouseButtonEventHandler(LayoutRoot_MouseRightButtonDown);
            LayoutRoot.MouseRightButtonUp += new MouseButtonEventHandler(LayoutRoot_MouseRightButtonUp);
        }

        private void LayoutRoot_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void LayoutRoot_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SetPropertiesGrid();
            }
        }

        private void SetPropertiesGrid()
        {
            PropertiesGrid.HideBordersForLastRow = false;
            PropertiesGrid.HideBordersForLastTwoColumns = false;

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

            Label lb = new Label();
            lb.Background = headerBrush;
            lb.Content = "Label";
            PropertiesGrid.PlaceUIElement(lb, 0, 0);
            lb = new Label();
            lb.Background = headerBrush;
            lb.Content = "Quantity";
            PropertiesGrid.PlaceUIElement(lb, 1, 0);
            lb = new Label();
            lb.Background = headerBrush;
            lb.Content = "Units";
            PropertiesGrid.PlaceUIElement(lb, 2, 0);

            if (isReadOnly)
            {
                TextBlock tb = new TextBlock();
                tb.Text = ItemSource[0].Label;
                PropertiesGrid.PlaceUIElement(tb, 0, 1);

                tb = new TextBlock();
                tb.Text = ItemSource[0].Quantity;

                PropertiesGrid.PlaceUIElement(tb, 1, 1);

                tb = new TextBlock();

                if (ItemSource[0].Units == -1)
                {
                    tb.Text = "";
                }
                else
                {
                    tb.Text = EnergyUnits[ItemSource[0].Units];
                }
                PropertiesGrid.PlaceUIElement(tb, 2, 1);
            }
            else
            {
                TextBox tb = new TextBox();
                tb.Text = ItemSource[0].Label;
                tb.TextChanged += new TextChangedEventHandler(HeatLabel_TextChanged);
                PropertiesGrid.PlaceUIElement(tb, 0, 1);

                // E.O.
                // We want a custom right-click menu for text boxes
                Core.App.InitRightClickMenu(tb);

                tb = new TextBox();
                tb.Text = ItemSource[0].Quantity;
                tb.GotFocus += new RoutedEventHandler(HeatQuantity_GotFocus);
                tb.LostFocus += new RoutedEventHandler(HeatQuantity_LostFocus);
                tb.KeyDown += new KeyEventHandler(HeatQuantity_KeyDown);
                PropertiesGrid.PlaceUIElement(tb, 1, 1);
                
                // E.O.
                // Again, custom right-click menu, now for the quantity text box
                Core.App.InitRightClickMenu(tb);

                ComboBox cb = new ComboBox();
                ComboBoxItem cbi;

                foreach (string s in EnergyUnits)
                {
                    cbi = new ComboBoxItem();
                    cbi.Content = s;
                    cb.Items.Add(cbi);
                }

                cb.SelectedIndex = ItemSource[0].Units;
                cb.Background = new SolidColorBrush(Colors.White);
                cb.SelectionChanged += new SelectionChangedEventHandler(EnergyUnitComboBox_SelectionChanged);
                PropertiesGrid.PlaceUIElement(cb, 2, 1);
            }
        }

        private void HeatQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string text = (sender as TextBox).Text;
                double doub;
                if (double.TryParse(text, out doub))
                {
                    ItemSource[0].Quantity = (sender as TextBox).Text;
                }
                else
                {
                    ItemSource[0].Quantity = "?";
                    (sender as TextBox).Text = "?";
                }
            }
        }

        private void HeatQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            double doub;
            if (double.TryParse(text, out doub))
            {
                ItemSource[0].Quantity = (sender as TextBox).Text;
            }
            else
            {
                ItemSource[0].Quantity = "?";
                (sender as TextBox).Text = "?";
            }
        }

        private void HeatQuantity_GotFocus(object sender, RoutedEventArgs e)
        {
            TableDataChanging(this, EventArgs.Empty);
            if ((sender as TextBox).Text == "?")
            {
                (sender as TextBox).Text = "";
            }
        }

        private void EnergyUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemSource[0].Units = (sender as ComboBox).SelectedIndex;
        }

        private void HeatLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            TableName = (sender as TextBox).Text;
            ItemSource[0].Label = TableName;
            TableDataChanging(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resets the table counter back to the initial state.  Used when creating a new file
        /// </summary>
        public static void ResetTableCounter()
        {
            HeatStreamPropertiesWindow.NumberOfTables = 1;
        }

        /// <summary>
        /// Generates a table name upon request
        /// </summary>
        /// <returns>A table name</returns>
        protected string getNextAvailableTableName()
        {
            String name = String.Format("{0}{1}", tableName, NumberOfTables);
            NumberOfTables++;
            return name;
        }

        /// <summary>
        /// Builds a header row for the properties table.  Should only be
        /// called once per instance of IPropertiesWindow
        /// </summary>
        /// <returns></returns>
        private HeatStreamData CreateTableHeader()
        {
            HeatStreamData d = new HeatStreamData();
            d.Label = this.TableName;
            d.Quantity = "?";
            d.Units = 0;
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
            HeatStreamData data = sender as HeatStreamData;

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

        /// <summary>
        /// Not used for properties table, but must inherit because it's a requirement for
        /// all PFD elements.  Selected will *always* return false.
        /// </summary>
        public bool Selected
        {
            get
            {
                return false;
            }
            set
            {
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
            XmlSerializer dataSerializer = new XmlSerializer(typeof(HeatStreamData));

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

            //This sets the binding

            //do not know how this binding works but it does :/
            //tooltip.SetBinding(Silverlight.Controls.ToolTip.ContentProperty, new Binding("ToolTipMessage") { Source = (this.PropertiesWindow.ItemsSource as HeatStreamData), Mode = BindingMode.TwoWay });

            //this attached it to the label
            //Silverlight.Controls.ToolTipService.SetToolTip(lb, tooltip);
        }

        public int CompareTo(object obj)
        {
            //make sure that we're comparing two table elements
            if (!(obj is HeatStreamPropertiesWindow))
            {
                return -1;
            }
            else
            {
                HeatStreamPropertiesWindow other = obj as HeatStreamPropertiesWindow;
                return TableName.CompareTo(other.TableName);
            }
        }

        public void HighlightFeedback(bool highlight)
        {
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
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
        }

        public void RemoveFeedback()
        {
            ItemSource[0].Feedback = "";
            ItemSource[0].ToolTipMessage = "";
        }

        private void feedbackTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label lb = sender as Label;

            Silverlight.Controls.ToolTip oldTooptip = Silverlight.Controls.ToolTipService.GetToolTip(lb);

            oldTooptip.IsOpen = false;
        }

        #region ICanvasElement Members

        /// <summary>
        /// TODO: Changes for collapsed mode?
        /// </summary>
        public Point Location
        {
            get
            {
                return new Point(
                    (double)GetValue(Canvas.LeftProperty) + ActualWidth / 2.0,
                    (double)GetValue(Canvas.TopProperty) + 5.0);
            }
            set
            {
                SetValue(Canvas.LeftProperty, value.X - ActualWidth / 2.0);
                SetValue(Canvas.TopProperty, value.Y - 5.0);
            }
        }

        #endregion
    }
}