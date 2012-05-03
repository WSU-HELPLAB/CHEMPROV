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
using ChemProV.PFD.ProcessUnits;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationEditor : UserControl, IXmlSerializable
    {
        #region Delegates

        public event EventHandler EquationTokensChanged = delegate { };

        #endregion Delegates

        #region instance variables

        private object selectedTool;

        private ObservableCollection<EquationData> equationData = new ObservableCollection<EquationData>();
        private IList<string> compounds = new List<string>();
        private List<string> elements = new List<string>();
        private ObservableCollection<EquationType> equationTypes = new ObservableCollection<EquationType>();
        private bool isReadOnly = false;
        private List<EquationModel> equationModels = new List<EquationModel>();
        private List<IPfdElement> pfdElements = new List<IPfdElement>();

        #endregion

        #region Properties

        public List<IPfdElement> PfdElements
        {
            get
            {
                return pfdElements;
            }
            set
            {
                //attach event listeners to each LabeledProcessUnit so that we can update our
                //scopes as necessary
                foreach (IPfdElement element in value)
                {
                    if (element is LabeledProcessUnit)
                    {
                        (element as LabeledProcessUnit).PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(LabeledProcessUnitPropertyChanged);
                    }
                }

                //and detach event listeners from the old list of elements
                foreach (IPfdElement element in pfdElements)
                {
                    if (element is LabeledProcessUnit)
                    {
                        (element as LabeledProcessUnit).PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(LabeledProcessUnitPropertyChanged);
                    }
                }

                //finally, replace the old with the new
                pfdElements = value;
                updateScopes();
            }
        }

        public ObservableCollection<EquationType> EquationTypes
        {
            get { return equationTypes; }
        }

        public ObservableCollection<EquationScope> EquationScopes { get; private set; }

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

        public IList<string> Compounds
        {
            get { return compounds; }
            set
            {
                compounds = value;
                updateCompounds();
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
            updateCompounds();
            updateScopes();
            AddNewEquationRow();
        }
        #endregion Constructor

        #region public methods

        [Obsolete("Does nothing")]
        public void InsertConstant(string constant)
        {

        }

        [Obsolete("Does nothing")]
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
            int rowCount = EquationsGrid.RowDefinitions.Count;
            for (int i = 0; i < rowCount; i++)
            {
                RemoveEquationRow(i);
            }
        }

        public void LoadXmlElements(XElement doc)
        {
            //clear the equation stack
            //EquationStackPanel.Children.Clear();

            //pull out the equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                EquationModel modelFromXml = EquationModel.FromXml(xmlEquation);
                EquationModel rowModel = AddNewEquationRow();

                rowModel.Annotation = modelFromXml.Annotation;
                rowModel.Equation = modelFromXml.Equation;
                rowModel.Scope = modelFromXml.Scope;
                rowModel.Type = modelFromXml.Type;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Adds a new equation row to the equations grid.
        /// </summary>
        private EquationModel AddNewEquationRow()
        {
            EquationModel newRowModel = new EquationModel();
            newRowModel.TypeOptions = EquationTypes;
            newRowModel.ScopeOptions = EquationScopes;
            newRowModel.RelatedElements = PfdElements;
            newRowModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(EquationModelPropertyChanged);
            equationModels.Add(newRowModel);

            int rowNumber = EquationsGrid.RowDefinitions.Count;
            EquationsGrid.RowDefinitions.Add(new RowDefinition());

            AnnotateControl annotateControl = new AnnotateControl();
            annotateControl.SetValue(Grid.RowProperty, rowNumber);
            annotateControl.SetValue(Grid.ColumnProperty, 0);
            annotateControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(annotateControl);

            TypeControl typeControl = new TypeControl();
            typeControl.SetValue(Grid.RowProperty, rowNumber);
            typeControl.SetValue(Grid.ColumnProperty, 1);
            typeControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(typeControl);

            ScopeControl scopeControl = new ScopeControl();
            scopeControl.SetValue(Grid.RowProperty, rowNumber);
            scopeControl.SetValue(Grid.ColumnProperty, 2);
            scopeControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(scopeControl);

            Views.EquationControl equationControl = new Views.EquationControl();
            equationControl.SetValue(Grid.RowProperty, rowNumber);
            equationControl.SetValue(Grid.ColumnProperty, 3);
            equationControl.DataContext = newRowModel;
            EquationsGrid.Children.Add(equationControl);
            return newRowModel;
        }

        /// <summary>
        /// Removes an equation row from the list of equations
        /// </summary>
        private void RemoveEquationRow(int rowNumber)
        {
            UIElement[] elements = (from child in EquationsGrid.Children
                                    where (int)child.GetValue(Grid.RowProperty) == rowNumber
                                    select child).ToArray();
            foreach (UIElement element in elements)
            {
                //element.Visibility = System.Windows.Visibility.Collapsed;
                EquationsGrid.Children.Remove(element);
            }
        }

        /// <summary>
        /// Updates the list of scoes that an equation can reference
        /// </summary>
        private void updateScopes()
        {
            EquationScopes = new ObservableCollection<EquationScope>();

            //add "overall" scope
            EquationScopes.Add(new EquationScope(EquationScopeClassification.Overall, Name = "Overall"));

            //add "unspecified" scope
            EquationScopes.Add(new EquationScope(EquationScopeClassification.Unknown, Name = "Unknown"));

            //add any process units to the list of possible scopes
            foreach (IPfdElement element in PfdElements)
            {
                LabeledProcessUnit unit = element as LabeledProcessUnit;
                if (unit != null)
                {
                    EquationScopes.Add(new EquationScope(EquationScopeClassification.SingleUnit, Name = unit.ProcessUnitLabel));

                    // E.O.
                    // I think this is where this goes, I'm still getting familiar with all the equation-oriented stuff
                    // If there's a scope for this process unit, then add it
                    if (!unit.Subprocess.Equals(System.Windows.Media.Colors.White))
                    {
                        // Find the name of this color
                        string name = null;
                        foreach (Core.NamedColor nc in Core.NamedColors.All)
                        {
                            if (nc.Color.Equals(unit.Subprocess))
                            {
                                name = nc.Name + " subprocess";
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(name))
                        {
                            EquationScopes.Add(new EquationScope(EquationScopeClassification.SubProcess, name));
                        }
                    }
                }
            }

            //update all view models
            foreach (EquationModel vm in equationModels)
            {
                vm.ScopeOptions = EquationScopes;
                UpdateEquationModelElements(vm);
            }
        }

        /// <summary>
        /// Updates the list of available compounds that an equation can balance across
        /// </summary>
        private void updateCompounds()
        {
            equationTypes = new ObservableCollection<EquationType>();
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

            equationTypes.Add(new EquationType(EquationTypeClassification.Total, "Total"));
            equationTypes.Add(new EquationType(EquationTypeClassification.Specification, "Specification"));

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

            foreach (EquationModel vm in equationModels)
            {
                vm.TypeOptions = EquationTypes;
            }
        }

        private List<IPfdElement> GetElementAndStreams(IProcessUnit unit)
        {
            List<IPfdElement> elements = new List<IPfdElement>();
            
            //add the process unit as well as its incoming and outgoing streams
            elements.Add(unit);
            foreach (IPfdElement element in unit.IncomingStreams)
            {
                elements.Add(element);
            }
            foreach (IPfdElement element in unit.OutgoingStreams)
            {
                elements.Add(element);
            }
            return elements;
        }

        private void UpdateEquationModelElements(EquationModel equation)
        {
            List<IPfdElement> relevantUnits = new List<IPfdElement>();

            //supply different PFD elements to the model depending on its scope
            switch (equation.Scope.Classification)
            {
                //With a single unit, all we care about is that unit and its related streams
                case EquationScopeClassification.SingleUnit:
                    LabeledProcessUnit selectedUnit = (from element in PfdElements
                                                      where element is LabeledProcessUnit
                                                      &&
                                                      (element as LabeledProcessUnit).ProcessUnitLabel == equation.Scope.Name
                                                      select element).FirstOrDefault() as LabeledProcessUnit;
                    if (selectedUnit != null)
                    {
                        relevantUnits = GetElementAndStreams(selectedUnit);
                    }
                    break;

                //ChemProV doesn't currently support sub processes, but this is where
                //that logic would go
                case EquationScopeClassification.SubProcess:
                    break;

                //AC: Not sure what should happen here
                case EquationScopeClassification.Unknown:
                    break;

                //Pull all source and sink units as well as their streams
                case EquationScopeClassification.Overall:
                default:
                    List<IProcessUnit> units = (from element in PfdElements
                                                where element is IProcessUnit
                                                &&
                                                (
                                                 (element as IProcessUnit).Description == ProcessUnitDescriptions.Sink
                                                 ||
                                                 (element as IProcessUnit).Description == ProcessUnitDescriptions.Source
                                                )
                                                select element as IProcessUnit).ToList();
                    foreach (IProcessUnit unit in units)
                    {
                        relevantUnits = relevantUnits.Union(GetElementAndStreams(unit)).ToList();
                    }
                    break;
            }

            //assign the updated list to the equation
            equation.RelatedElements = relevantUnits;
        }

        #endregion Private Helpers

        #region event handlers

        /// <summary>
        /// Called whenever a labeled process unit's name changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabeledProcessUnitPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateScopes();
        }

        /// <summary>
        /// Called whenever the user makes a change to one of the equations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EquationModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EquationModel model = sender as EquationModel;
            
            //if the scope changed, then update the property units that are visible to the particular view model
            if (e.PropertyName == "Scope")
            {
                UpdateEquationModelElements(model);
            }
            //is the data being modified the last row in our equations grid?  If so, 
            //add a new one
            int maxRowCount = EquationsGrid.RowDefinitions.Count - 1; //subtract 1 because rows start at 0
            UIElement element = (from child in EquationsGrid.Children
                                 where (int)child.GetValue(Grid.RowProperty) == maxRowCount
                                 select child).FirstOrDefault();
            if (element != null)
            {
                EquationModel elementVm = (element.GetValue(Control.DataContextProperty) as EquationModel);
                if (elementVm.Id == model.Id && model.Equation.Length != 0)
                {
                    AddNewEquationRow();
                }
                else if (elementVm.Id != model.Id)
                {
                    //if not, perhaps its empty and we need to remove the row
                    if (maxRowCount > 2 && model.Equation.Length == 0)
                    {
                        FrameworkElement[] controls = (from child in EquationsGrid.Children
                                                       where (child as FrameworkElement).DataContext is EquationModel //make sure that the child has the correct view model (header row doesn't)
                                                       select child as FrameworkElement).ToArray();
                        element = (from control in controls
                                   where (control.DataContext as EquationModel).Id == model.Id
                                   select control).FirstOrDefault();
                        if (element != null)
                        {
                            RemoveEquationRow((int)element.GetValue(Grid.RowProperty));
                        }
                    }
                }
            }
        }

        #endregion

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
            XmlSerializer serializer = new XmlSerializer(typeof(EquationModel));

            writer.WriteStartElement("Equations");
            foreach (EquationModel model in equationModels)
            {
                serializer.Serialize(writer, model);
            }
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members
    }
}