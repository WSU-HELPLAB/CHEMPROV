/* 
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.EquationEditor.Views;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationEditor : UserControl, IXmlSerializable
    {
        #region Delegates

        public event EventHandler EquationTokensChanged = delegate { };

        #endregion Delegates

        #region Fields

        private object selectedTool;

        private EquationControl selectedEquation;
        private ObservableCollection<EquationData> equationData = new ObservableCollection<EquationData>();
        private IList<string> compounds;
        private List<string> elements = new List<string>();
        private ObservableCollection<EquationType> equationTypes = new ObservableCollection<EquationType>();

        private bool isReadOnly = false;

        private List<EquationViewModel> viewModels = new List<EquationViewModel>();

        #endregion Fields

        #region Properties

        public List<IPfdElement> PfdElements { get; set; }

        public ObservableCollection<EquationType> EquationTypes
        {
            get { return equationTypes; }
        }

        public object SelectedTool
        {
            get { return selectedTool; }
            set { selectedTool = value; }
        }

        public OptionDifficultySetting CurrentDifficultySetting
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a list of EquationData for everything equation it has and returns that.
        /// </summary>
        public ObservableCollection<EquationData> EquationsData
        {
            get
            {
                foreach (EquationData data in equationData)
                {
                    data.Dispose();
                }
                ObservableCollection<EquationData> newEquationDatas = new ObservableCollection<EquationData>();
                foreach (EquationControl eq in Equations)
                {
                    newEquationDatas.Add(new EquationData(eq));
                }
                equationData = newEquationDatas;
                return newEquationDatas;
            }
        }

        public IList<string> Compounds
        {
            get { return compounds; }
            set
            {
                compounds = value;
                updateCompounds();
            }
        }

        [Obsolete("EquationControl class is on its way out")]
        public List<EquationControl> Equations
        {
            get
            {
                return new List<EquationControl>();
            }
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
            }
        }

        #endregion Properties

        #region Constructor

        public EquationEditor()
        {
            InitializeComponent();

            PfdElements = new List<IPfdElement>();

            //create our first row
            AddNewEquationRow();
        }

        /// <summary>
        /// Adds a new equation row to the equations grid.
        /// </summary>
        private void AddNewEquationRow()
        {
            EquationViewModel newRowModel = new EquationViewModel();
            newRowModel.TypeOptions = EquationTypes;
            newRowModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(EquationViewModelPropertyChanged);
            viewModels.Add(newRowModel);

            int rowNumber = EquationsGrid.RowDefinitions.Count;
            EquationsGrid.RowDefinitions.Add(new RowDefinition());

            ErrorControl errorControl = new ErrorControl();
            errorControl.SetValue(Grid.RowProperty, rowNumber);
            errorControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(errorControl);

            ScopeControl scopeControl = new ScopeControl();
            scopeControl.SetValue(Grid.RowProperty, rowNumber);
            scopeControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(scopeControl);

            TypeControl typeControl = new TypeControl();
            typeControl.SetValue(Grid.RowProperty, rowNumber);
            typeControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(typeControl);

            Views.EquationControl equationControl = new Views.EquationControl();
            equationControl.SetValue(Grid.RowProperty, rowNumber);
            equationControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(equationControl);

        }

        /// <summary>
        /// Called whenever the user makes a change to one of the equations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EquationViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EquationViewModel model = sender as EquationViewModel;

            //is the data being modified the last row in our equations grid?  If so, 
            //add a new one
            int maxRowCount = EquationsGrid.RowDefinitions.Count - 1; //subtract 1 because rows start at 0
            UIElement element = (from child in EquationsGrid.Children
                                 where (int)child.GetValue(Grid.RowProperty) == maxRowCount
                                 select child).FirstOrDefault();
            EquationViewModel elementVm = (element.GetValue(Control.DataContextProperty) as EquationViewModel);
            if (elementVm.Id == model.Id && model.Equation.Length != 0)
            {
                AddNewEquationRow();
            }
        }

        #endregion Constructor

        #region Methods

        [Obsolete("Does nothing")]
        public void InsertConstant(string constant)
        {

        }

        public void ChangeSolveabiltyStatus(bool solvable)
        {
            //Empty since solvability is turned off.
            //We can't really say if something is solvable or not.
            return;
        }

        /// <summary>
        /// Will remove all existing equations currently listed in the equation editor.
        /// </summary>
        [Obsolete("Does nothing")]
        public void ClearEquations()
        {
            
        }

        [Obsolete("Needs to be reworked")]
        public void LoadXmlElements(XElement doc)
        {
            //clear the equation stack
            //EquationStackPanel.Children.Clear();

            //pull out the equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                //create the equation
                EquationControl eq = new EquationControl(isReadOnly);

                //attach event listeners
                //RegisterEquationListeners(eq);

                //add to the list of equations
                //EquationStackPanel.Children.Add(eq);

                //set the equation's value
                eq.EquationText = xmlEquation.Attribute("EquationText").Value;

                XElement xmlEquationType = xmlEquation.Element("EquationType");

                //AC: Default to an overall balance if the EquationType element has no children.
                //    This is kind of a quick hack, and as such, I'm not sure whether or not
                //    there are constants that I could be using instead of my strings.
                string content = "Overall";
                EquationTypeClassification classification = EquationTypeClassification.Total;
                if (xmlEquationType.HasAttributes)
                {
                    content = xmlEquationType.Attribute("SelectedItemContent").Value;
                    classification = (EquationTypeClassification)Int32.Parse(xmlEquationType.Attribute("SelectedItemClassification").Value);
                }
                EquationType item = new EquationType(classification, content);

                eq.EquationTypes.Add(item);
                eq.EquationType.SelectedItem = item;

                eq.EquationTextChanged();
            }

            //create the equation
            EquationControl lastEq = new EquationControl(isReadOnly);

            //attach event listeners
            //RegisterEquationListeners(lastEq);

            //add to the list of equations
            //EquationStackPanel.Children.Add(lastEq);

            //the last equation added needs to have a special event listener attached
            //lastEq.MyTextChanged += new EventHandler(eq_MyTextChanged);
        }

        #endregion Methods

        #region Private Helpers


        private void updateCompounds()
        {
            equationTypes = new ObservableCollection<EquationType>();
            ObservableCollection<EquationType> types = new ObservableCollection<EquationType>();
            elements.Clear();
            foreach (string compoundstr in compounds)
            {
                Compound compound = CompoundFactory.GetElementsOfCompound((compoundstr).ToLower());

                foreach (KeyValuePair<Element, int> element in compound.elements)
                {
                    if (!elements.Contains(element.Key.Name))
                    {
                        elements.Add(element.Key.Name);
                        
                    }
                }
            }

            //equationTypes.Add(new ComboBoxEquationTypeItem(EquationClassification.VariableDefinition));
            equationTypes.Add(new EquationType(EquationTypeClassification.Total, "Overall"));

            foreach (string compound in compounds)
            {
                equationTypes.Add(new EquationType(EquationTypeClassification.Compound, compound));
            }

            if (CurrentDifficultySetting != OptionDifficultySetting.MaterialBalance)
            {
                foreach (string element in elements)
                {
                    equationTypes.Add(new EquationType(EquationTypeClassification.Atom, element + "(e)"));
                }
            }

            if (CurrentDifficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
            {
                equationTypes.Add(new EquationType(EquationTypeClassification.Atom, ""));
            }

            foreach (EquationViewModel vm in viewModels)
            {
                vm.TypeOptions = EquationTypes;
            }
        }

        private void EquationTokens_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //AC: Since I'm gutting the equation validation process, we don't need this any more.
            //EquationTokensChanged(sender, new EventArgs());
        }

        private void EquationTextChanged(object sender, EventArgs e)
        {
            //Not needed because I turned off equation validation
            //EquationTokensChanged(sender, e);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(double.IsNaN(this.ActualWidth)))
            {
                /*
                foreach (EquationControl eq in EquationStackPanel.Children)
                {
                    eq.MaxWidth = this.ActualWidth;
                }
                 * */
            }
        }

        #endregion Private Helpers

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //not responsible for reading of XML data.  Handled in LoadXmlElements.
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EquationControl));

            writer.WriteStartElement("Equations");

            /*
            var equations = from c in this.EquationStackPanel.Children where c is EquationControl select c as EquationControl;
            //loop through our list of equations except the last one
            foreach (EquationControl equation in equations)
            {
                //if it is just empty or whitespace don't save it
                if (equation.EquationText.Trim() != "")
                {
                    serializer.Serialize(writer, equation);
                }
            }
             * */
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members
    }
}