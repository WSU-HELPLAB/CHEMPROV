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

using Silverlight.Controls;

using ChemProV.Validation;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.PFD.Streams.PropertiesWindow.Heat
{

    public partial class HeatStreamPropertiesWindow : UserControl, IPropertiesWindow, IComparable
    {
        public event EventHandler SelectionChanged = delegate { };
        public event EventHandler LocationChanged = delegate { };
        public event TableDataEventHandler TableDataChanged = delegate { };

        //reference to the parent stream
        private IStream parentStream = null;

        //Keeps track of the number of Tables that have been made
        protected static int NumberOfTables = 1;

        //this keeps the record of what table number the table is when it is created
        private string tableName = "Q";

        private LinearGradientBrush headerBrush;

        private static string[] EnergyUnits = { "btu", "btu per second", "joules", "watts"};

        public ObservableCollection<HeatStreamData> ItemSource = new ObservableCollection<HeatStreamData>();

        private PropertiesWindowGrid PropertiesGrid = new PropertiesWindowGrid();

        public HeatStreamPropertiesWindow()
            : base()
        {
            InitializeComponent();
            LocalInit();
        }

        public HeatStreamPropertiesWindow(IStream parent)
            : base()
        {
            InitializeComponent();
            ParentStream = parent;
            LocalInit();
        }

        private void LocalInit()
        {
            //Set this table's name
            TableName = getNextAvailableTableName();

            //Create bindings that listen for changes in the object's location
            SetBinding(Canvas.LeftProperty, new Binding("LeftProperty") { Source = this, Mode = BindingMode.TwoWay });
            SetBinding(Canvas.TopProperty, new Binding("TopProperty") { Source = this, Mode = BindingMode.TwoWay });

            //create the header row
            ItemSource.Add(CreateTableHeader());

            LayoutRoot.Children.Add(PropertiesGrid.My_Grid);

            SetPropertiesGrid();

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

            TextBox tb = new TextBox();
            tb.Text = ItemSource[0].Label;
            tb.TextChanged += new TextChangedEventHandler(HeatLabel_TextChanged);
            PropertiesGrid.PlaceUIElement(tb, 0, 1);

            tb = new TextBox();
            tb.Text = ItemSource[0].Quantity;
            tb.TextChanged += new TextChangedEventHandler(HeatQuantity_TextChanged);
            PropertiesGrid.PlaceUIElement(tb, 1, 1);

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

        void EnergyUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemSource[0].Units = (sender as ComboBox).SelectedIndex;
        }

        void HeatLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            TableName = (sender as TextBox).Text;
            ItemSource[0].Label = TableName;
        }

        void HeatQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            try
            {
                double.Parse(text);
                ItemSource[0].Quantity = text;
            }
            catch
            {
                (sender as TextBox).Text = "?";
                ItemSource[0].Quantity = "?";
            }

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

        private void PropertiesWindowDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.GetIndex() == 0 && e.Column.Header.ToString() == "Label")
            {
                string label = (e.Column.GetCellContent(e.Row) as TextBox).Text;
                if (label[0] == 'T' || label[0] == 'H' || label == "Cp")
                {
                    //TableNames cannot start with the capital letter T reserved for Temp column or H reserved for the constants and Cp is a constant.

                    //throw new ValidationException("Label cannot start with T or H");
                    (e.Column.GetCellContent(e.Row) as TextBox).Text = tableName;
                    return;
                }
            }
            else if (e.Column.Header.ToString() == "Quantity")
            {
                TextBox tb = (e.Column.GetCellContent(e.Row) as TextBox);
                try
                {
                    double.Parse(tb.Text);
                }
                catch
                {
                    if (tb.Text != "?")
                    {
                        if (e.Column.Header.ToString() == "Quantity")
                        {
                            tb.Text = ItemSource[e.Row.GetIndex()].Quantity;
                            //The validationException are not working dunno why (might have something to do with the fact that they are green? jk)
                            //throw new ValidationException("Quantity must be a ? or a valid number");
                        }
                    }
                    else if (e.Column.Header.ToString() == "Temp.")
                    {
                        tb.Text = "T" + tableName;
                    }

                }
            }
        }
    }
}
