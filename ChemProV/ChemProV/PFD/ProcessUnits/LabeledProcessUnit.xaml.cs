using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Data;

namespace ChemProV.PFD.ProcessUnits
{
    public partial class LabeledProcessUnit : GenericProcessUnit, INotifyPropertyChanged
    {
        private string processUnitLabel = "foo";
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Default constructor - used only for design view
        /// Keep the protection on this private!
        /// </summary>
        private LabeledProcessUnit()
            : base("/UI/Icons/pu_generic.png")
        {

        }
        
        public LabeledProcessUnit(string iconSource)
            : base(iconSource)
        {
            InitializeComponent();
            this.DataContext = this;
            ProcessUnitNameText.MouseLeftButtonDown += new MouseButtonEventHandler(ProcessUnitNameText_MouseLeftButtonDown);
            ProcessUnitNameBox.MouseLeftButtonDown += new MouseButtonEventHandler(ProcessUnitNameBox_MouseLeftButtonDown);
            ProcessUnitNameBox.LostFocus += new RoutedEventHandler(ProcessUnitNameBox_LostFocus);
            ProcessUnitNameBox.KeyDown += new KeyEventHandler(ProcessUnitNameBox_KeyDown);

            // E.O.
            // Create the icon image
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(iconSource, UriKind.Relative);
            ProcessUnitImage.SetValue(Image.SourceProperty, bmp);
        }

        /// <summary>
        /// Used to track the process unit's label (name)
        /// </summary>
        public String ProcessUnitLabel
        {
            get
            {
                return processUnitLabel;
            }
            set
            {
                //don't allow empty names
                if (value.Length > 0)
                {
                    processUnitLabel = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ProcessUnitLabel"));
                }
            }
        }

        void ProcessUnitNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUnitNameBox_LostFocus(this, new RoutedEventArgs());
            }
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        void ProcessUnitNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ProcessUnitNameText.Visibility = System.Windows.Visibility.Visible;
            ProcessUnitNameBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        void ProcessUnitNameBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void ProcessUnitNameText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessUnitNameText.Visibility = System.Windows.Visibility.Collapsed;
            ProcessUnitNameBox.Visibility = System.Windows.Visibility.Visible;
            ProcessUnitNameBox.Focus();
            e.Handled = true;
        }

        #region GenericProcessUnitOverrides

        /// <summary>
        /// Gets the icon dependency property
        /// </summary>
        public override Image Icon
        {
            get
            {
                return ProcessUnitImage;
            }
        }

        /// <summary>
        /// Use to reference the border around the process unit's icon.  
        /// </summary>
        public override Border IconBorder
        {
            get
            {
                return ProcessUnitBorder;
            }
            set
            {
                ProcessUnitBorder = value;
            }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Name", ProcessUnitLabel);
            base.WriteXml(writer);
        }

        public override IProcessUnit FromXml(System.Xml.Linq.XElement xpu, IProcessUnit targetUnit)
        {
            (targetUnit as LabeledProcessUnit).ProcessUnitLabel = xpu.Attribute("Name").Value;
            targetUnit = base.FromXml(xpu, targetUnit);
            return targetUnit;
        }

        #endregion

    }
}
