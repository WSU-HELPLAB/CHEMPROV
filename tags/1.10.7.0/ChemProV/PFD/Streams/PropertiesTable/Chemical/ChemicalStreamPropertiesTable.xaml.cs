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

namespace ChemProV.PFD.Streams.PropertiesTable.Chemical
{

    public partial class ChemicalStreamPropertiesTable : UserControl, IPropertiesTable, IComparable
    {
        public event EventHandler SelectionChanged = delegate { };
        public event EventHandler LocationChanged = delegate { };
        public event TableDataEventHandler TableDataChanged = delegate { };

        //reference to the parent stream
        private IStream parentStream = null;

        //Keeps track of the number of Tables that have been made
        protected static int NumberOfTables = 1;

        //this keeps the record of what table number the table is when it is created
        private string tableName = "M";

        private ObservableCollection<string> rowLabels = new ObservableCollection<string>();

        public ObservableCollection<ChemicalStreamData> ItemSource = new ObservableCollection<ChemicalStreamData>();

        public ObservableCollection<string> RowLabels
        {
            get { return rowLabels; }
            set { rowLabels = value; }
        }

        public ChemicalStreamPropertiesTable()
            : base()
        {
            InitializeComponent();
            LocalInit();
        }

        public ChemicalStreamPropertiesTable(IStream parent)
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
            
            PropertiesTable.SelectedIndex = 0;

            //Set the dataContext for the binding
            this.PropertiesTable.DataContext = this;

            PropertiesTable.SelectedIndex = 0;

            //Create bindings that listen for changes in the object's location
            SetBinding(Canvas.LeftProperty, new Binding("LeftProperty") { Source = this, Mode = BindingMode.TwoWay });
            SetBinding(Canvas.TopProperty, new Binding("TopProperty") { Source = this, Mode = BindingMode.TwoWay });

            PropertiesTable.ItemsSource = ItemSource;

            //attach event listeners
            PropertiesTable.PreparingCellForEdit += new EventHandler<DataGridPreparingCellForEditEventArgs>(PropertiesTable_PreparingCellForEdit);
            PropertiesTable.KeyDown += new KeyEventHandler(PropertiesTable_KeyDown);

            //create the header row
            ItemSource.Add(CreateTableHeader());
            ItemSource.Add(CreateNewDataRow());
            
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
        protected string getNextAvailableRowName()
        {
            string name = String.Format("{0}{1}", TableName.ToLower(), ItemSource.Count);
            return name;
        }
        /// <summary>
        /// Builds a header row for the properties table.  Should only be
        /// called once per instance of IPropertiesTable
        /// </summary>
        /// <returns></returns>
        private ChemicalStreamData CreateTableHeader()
        {
            ChemicalStreamData d = new ChemicalStreamData();
            d.Label = this.TableName;
            rowLabels.Add(d.Label);
            d.Quantity = "?";
            d.Units = 1;
            d.Compound = 25;
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
            rowLabels.Add(d.Label);
            d.Quantity = "?";
            d.Units = 0;
            d.Compound = 0;
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
            if(ItemSource[ItemSource.Count - 1].CompareTo(data) == 0)
            {
                ItemSource.Add(CreateNewDataRow());
            }

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
        /// Checks to see if the "delete" key was pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PropertiesTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (PropertiesTable.SelectedIndex != 0)
                {
                    ItemSource[PropertiesTable.SelectedIndex].PropertyChanged -= new PropertyChangedEventHandler(DataUpdated);
                    ItemSource.RemoveAt(PropertiesTable.SelectedIndex);
                }
            }

        }

        private void PropertiesTable_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if ((e.Column.Header.ToString() == "Compound" && PropertiesTable.SelectedIndex == 0) || e.Column.Header.ToString() == "")
            {
                //This stops the first row's compound from being changable
                PropertiesTable.CancelEdit();
            }
            else if (e.Column.Header.ToString() == "Compound" || e.Column.Header.ToString() == "Units")
            {
                (e.Column.GetCellContent(e.Row) as ComboBox).IsDropDownOpen = true;
            }
        }

        /// <summary>
        /// Gets the underlying DataGrid for the properties table
        /// </summary>
        public DataGrid PropertiesTable
        {
            get
            {
                return PropertiesTableDataGrid;
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
        }

        #endregion

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                //Sometimes this fails no idea why.. so try at catch incase it does
                PropertiesTable.CommitEdit();
            }
            catch
            {
            }
        }

        private void HeaderCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void PropertiesTableDataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            HeaderCanvas.Width = (sender as DataGrid).ActualWidth;
            LayoutRoot.Width = (sender as DataGrid).ActualWidth;
        }

        private void PropertiesTableDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //This is so if the table does get a mouse event it just ignores it otherwise it will
            //not drop when the mouse is released
            e.Handled = true;
        }

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
            tooltip.SetBinding(Silverlight.Controls.ToolTip.ContentProperty, new Binding("ToolTipMessage") { Source = (this.PropertiesTable.ItemsSource as ChemicalStreamData), Mode = BindingMode.TwoWay });

            //this attached it to the label
            Silverlight.Controls.ToolTipService.SetToolTip(lb, tooltip);

        }

        public int CompareTo(object obj)
        {
            //make sure that we're comparing two table elements
            if (!(obj is ChemicalStreamPropertiesTable))
            {
                return -1;
            }
            else
            {
                ChemicalStreamPropertiesTable other = obj as ChemicalStreamPropertiesTable;
                return TableName.CompareTo(other.TableName);
            }
        }
    }
}
