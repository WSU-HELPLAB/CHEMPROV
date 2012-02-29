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
        private ObservableCollection<ComboBoxEquationTypeItem> equationTypes = new ObservableCollection<ComboBoxEquationTypeItem>();

        private bool isReadOnly = false;

        private List<EquationViewModel> viewModels = new List<EquationViewModel>();

        #endregion Fields

        #region Properties

        public ObservableCollection<ComboBoxEquationTypeItem> EquationTypes
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

        public List<EquationControl> Equations
        {
            get
            {
                var eqs = from c in EquationStackPanel.Children where c is EquationControl select c as EquationControl;
                return new List<EquationControl>(eqs);
            }
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                foreach (EquationControl eq in EquationStackPanel.Children)
                {
                    eq.IsReadOnly = value;
                }
            }
        }

        #endregion Properties

        #region Constructor

        public EquationEditor()
        {
            InitializeComponent();
            EquationControl eq = new EquationControl(isReadOnly);

            //create our first row
            AddNewEquationRow();

            //this makes the first text box listen for when it has been changed and when it is it makes a new text box and sets that one to listen for the same thing
            RegisterEquationListeners(eq);
            selectedEquation = eq;
            eq.MyTextChanged += new EventHandler(eq_MyTextChanged);
            EquationStackPanel.Children.Add(eq);
            EquationScrollViewer.DataContext = this;
        }

        private void AddNewEquationRow()
        {
            EquationViewModel newRowModel = new EquationViewModel();
            newRowModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(EquationViewModelPropertyChanged);
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
            if (elementVm.Id == model.Id)
            {
                AddNewEquationRow();
            }
        }

        #endregion Constructor

        #region Methods

        public void InsertConstant(string constant)
        {
            selectedEquation.EquationText += " " + constant;
            EquationTextChanged(selectedEquation, EventArgs.Empty);

            //check to see if it is the last one if so need to call the eq_TextInputStart
            if (EquationStackPanel.Children[EquationStackPanel.Children.Count - 1] == selectedEquation)
            {
                eq_MyTextChanged(selectedEquation, EventArgs.Empty as TextCompositionEventArgs);
            }
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
        public void ClearEquations()
        {
            //remove event handlers
            foreach (UIElement element in EquationStackPanel.Children)
            {
                if (element is EquationControl)
                {
                    EquationControl eq = element as EquationControl;
                    UnregisterEquationListneres(eq);
                }
            }

            this.EquationStackPanel.Children.Clear();

            //add new child
            EquationControl newEq = new EquationControl(isReadOnly);

            //attach event listeners
            RegisterEquationListeners(newEq);
            newEq.MyTextChanged += new EventHandler(eq_MyTextChanged);

            //add to the list of equations
            EquationStackPanel.Children.Add(newEq);
        }

        public void LoadXmlElements(XElement doc)
        {
            //clear the equation stack
            EquationStackPanel.Children.Clear();

            //pull out the equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                //create the equation
                EquationControl eq = new EquationControl(isReadOnly);

                //attach event listeners
                RegisterEquationListeners(eq);

                //add to the list of equations
                EquationStackPanel.Children.Add(eq);

                //set the equation's value
                eq.EquationText = xmlEquation.Attribute("EquationText").Value;

                XElement xmlEquationType = xmlEquation.Element("EquationType");

                //AC: Default to an overall balance if the EquationType element has no children.
                //    This is kind of a quick hack, and as such, I'm not sure whether or not
                //    there are constants that I could be using instead of my strings.
                string content = "Overall";
                EquationClassification classification = EquationClassification.Overall;
                if (xmlEquationType.HasAttributes)
                {
                    content = xmlEquationType.Attribute("SelectedItemContent").Value;
                    classification = (EquationClassification)Int32.Parse(xmlEquationType.Attribute("SelectedItemClassification").Value);
                }
                ComboBoxEquationTypeItem item = new ComboBoxEquationTypeItem(classification, content);

                eq.EquationTypes.Add(item);
                eq.EquationType.SelectedItem = item;

                eq.EquationTextChanged();
            }

            //create the equation
            EquationControl lastEq = new EquationControl(isReadOnly);

            //attach event listeners
            RegisterEquationListeners(lastEq);

            //add to the list of equations
            EquationStackPanel.Children.Add(lastEq);

            //the last equation added needs to have a special event listener attached
            lastEq.MyTextChanged += new EventHandler(eq_MyTextChanged);
        }

        #endregion Methods

        #region Private Helpers

        private void eq_MyTextChanged(object sender, EventArgs e)
        {
            EquationControl eq = new EquationControl(isReadOnly);
            
            //this makes the text box that just fired stop listening since it is no longer the last one
            ((EquationControl)sender).MyTextChanged -= new EventHandler(eq_MyTextChanged);

            //this makes the new text box start listening whenever it has been fired
            eq.MyTextChanged += new EventHandler(eq_MyTextChanged);
            RegisterEquationListeners(eq);
            EquationStackPanel.Children.Add(eq);
            EquationTokensChanged(this, EventArgs.Empty);
        }

        private void eq_ReceivedFocus(object sender, EventArgs e)
        {
            selectedEquation = sender as EquationControl;
            selectedEquation.IsReadOnly = false;
        }

        private void updateCompounds()
        {
            equationTypes = new ObservableCollection<ComboBoxEquationTypeItem>();
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
            equationTypes.Add(new ComboBoxEquationTypeItem(EquationClassification.Overall));

            foreach (string compound in compounds)
            {
                equationTypes.Add(new ComboBoxEquationTypeItem(EquationClassification.Compound, compound));
            }

            if (CurrentDifficultySetting != OptionDifficultySetting.MaterialBalance)
            {
                foreach (string element in elements)
                {
                    equationTypes.Add(new ComboBoxEquationTypeItem(EquationClassification.Element, element + "(e)"));
                }
            }

            if (CurrentDifficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
            {
                equationTypes.Add(new ComboBoxEquationTypeItem(EquationClassification.Energy));
            }

            foreach (EquationControl eq in Equations)
            {
                eq.UpdateEquationTypeComboBox(equationTypes);
            }
        }

        private void EquationTokens_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //AC: Since I'm gutting the equation validation process, we don't need this any more.
            //EquationTokensChanged(sender, new EventArgs());
        }

        /// <summary>
        /// Called whenever a user navigates away from an equation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eq_LostFocus(object sender, RoutedEventArgs e)
        {
            EquationControl senderEq = sender as EquationControl;

            //if the sender is not the last equation in the list  and it empty, remove it
            if (senderEq.CompareTo(EquationStackPanel.Children.ElementAt(EquationStackPanel.Children.Count - 1)) != 0)
            {
                if (senderEq.EquationText.Trim().Length == 0)
                {
                    //remove from the stack panel
                    EquationStackPanel.Children.Remove(senderEq);

                    //remove event handlers
                    UnregisterEquationListneres(senderEq);
                }
            }
            senderEq.IsReadOnly = true;
        }

        private void EquationTextChanged(object sender, EventArgs e)
        {
            //Not needed because I turned off equation validation
            //EquationTokensChanged(sender, e);
        }

        /// <summary>
        /// Created to consolidate all event listener attachments into a single location
        /// rather than having it spread all over the file.
        /// </summary>
        /// <param name="eq">The equation that we'd like to attach events to.</param>
        private void RegisterEquationListeners(EquationControl eq)
        {
            eq.EquationTokensChagned += new EventHandler(EquationTextChanged);
            eq.LostFocus += new RoutedEventHandler(eq_LostFocus);
            eq.ReceivedFocus += new EventHandler(eq_ReceivedFocus);
            eq.LostFocus += new RoutedEventHandler(eq_LostFocus);
        }

        /// <summary>
        /// The inverse of RegisterEquationListeners: unregisters all event listeners from
        /// a given equation
        /// </summary>
        /// <param name="eq"></param>
        private void UnregisterEquationListneres(EquationControl eq)
        {
            eq.LostFocus -= new RoutedEventHandler(eq_LostFocus);
            eq.EquationTokensChagned -= new EventHandler(EquationTextChanged);
            eq.MyTextChanged -= new EventHandler(eq_MyTextChanged);
            eq.ReceivedFocus -= new EventHandler(eq_ReceivedFocus);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(double.IsNaN(this.ActualWidth)))
            {
                foreach (EquationControl eq in EquationStackPanel.Children)
                {
                    eq.MaxWidth = this.ActualWidth;
                }
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
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members
    }
}