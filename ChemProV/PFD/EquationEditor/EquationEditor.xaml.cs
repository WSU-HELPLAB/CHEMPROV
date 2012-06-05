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
using ChemProV.PFD.Undos;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationEditor : UserControl, IXmlSerializable
    {
        #region Delegates

        public event EventHandler EquationTokensChanged = delegate { };

        #endregion Delegates

        #region instance variables

        private IList<string> compounds = new List<string>();
        private List<string> elements = new List<string>();
        private ObservableCollection<EquationType> equationTypes = new ObservableCollection<EquationType>();
        private bool m_isReadOnly = false;

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
            get { return m_isReadOnly; }
            set
            {
                m_isReadOnly = value;

                // The button to add a new row should only be visible if we're not in read 
                // only mode
                AddNewRowButton.Visibility = m_isReadOnly ?
                    System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
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
            updateCompounds();
            updateScopes();
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
        /// Will remove all existing equations currently listed in the equation editor. Can 
        /// optionally add in the default blank row if desired.
        /// </summary>
        public void ClearEquations(bool addDefaultBlank)
        {
            EquationsGrid.Children.Clear();

            if (addDefaultBlank)
            {
                AddNewEquationRow();
            }
        }

        public void LoadXmlElements(XElement doc)
        {
            //pull out the equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                EquationModel rowModel = AddNewEquationRow(xmlEquation);
            }

            updateCompounds();
        }

        public List<IUndoRedoAction> MergeAnnotationsFrom(XDocument doc, string userNameIfNotInXml)
        {
            // Initialize the list of undo/redo actions
            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
            
            // The root should be ProcessFlowDiagram
            XElement root = doc.Element("ProcessFlowDiagram");
            if (null == root)
            {
                return undos;
            }

            // Find the <EquationEditor> child and then <Equations> within that
            XElement ee = root.Element("EquationEditor");
            if (null == ee) { return undos; }
            XElement eqs = ee.Element("Equations");
            if (null == eqs) { return undos; }

            // Iterate through <EquationModel> elements
            foreach (XElement em in eqs.Elements("EquationModel"))
            {
                // See if we have an annotation stored
                XElement annotationElement = em.Element("Annotation");
                if (null == annotationElement)
                {
                    continue;
                }

                // Look for a user name attribute
                string xmlUserName = null;
                XAttribute userAttr = annotationElement.Attribute("UserName");
                if (null != userAttr)
                {
                    xmlUserName = userAttr.Value;
                }

                // We need to make sure that we match up equation models with the same equation
                XElement emEq = em.Element("Equation");
                if (null == emEq)
                {
                    throw new Exception(
                        "Element \"EquationModel\" is missing child \"Equation\" element");
                }

                // Look through the equation models and try to find a matching equation string
                EquationModel match = null;
                foreach (EquationModel emThis in this.equationModels)
                {
                    if (emThis.Equation.Equals(emEq.Value))
                    {
                        match = emThis;
                        break;
                    }
                }

                // Go to next <EquationModel> element if we didn't find a match
                if (null == match)
                {
                    continue;
                }
                
                string anno = annotationElement.Value;
                if (!string.IsNullOrEmpty(anno))
                {
                    // Here's where we actually do the merge
                    undos.Add(new SetAnnotation(match, match.Annotation));
                    
                    // Prioritize user names from the Xml over the function's parameter
                    if (!string.IsNullOrEmpty(xmlUserName))
                    {
                        match.Annotation += "\r\n\r\n--- " + xmlUserName + " ---\r\n" + anno;
                    }
                    else
                    {
                        match.Annotation += "\r\n\r\n--- " + userNameIfNotInXml + " ---\r\n" + anno;
                    }
                }
            }

            return undos;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Adds a blank new equation row to the equations grid
        /// </summary>
        /// <returns></returns>
        private EquationModel AddNewEquationRow()
        {
            return AddNewEquationRow(null);
        }

        /// <summary>
        /// Adds a new equation row to the equations list. If the optional Xml equation element is non-null 
        /// then it will be used to fill the row's data appropriately. If it is null then the new row will 
        /// be blank.
        /// </summary>
        private EquationModel AddNewEquationRow(XElement optionalXmlEquation)
        {
            // E.O.
            // Create a new equation control and add it to the stack panel
            EquationControl newRow = new EquationControl(this);
            if (null != optionalXmlEquation)
            {
                newRow.LoadFrom(optionalXmlEquation);
            }

            // Set the deletion callback function
            newRow.SetDeleteRequestDelegate(this.DeleteEquationRow);

            // TODO: Make it readonly if necessary

            EquationsGrid.Children.Add(newRow);
            newRow.Model.RelatedElements = PfdElements;
            newRow.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(EquationModelPropertyChanged);

            newRow.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;

            return newRow.Model;
        }

        /// <summary>
        /// This is called from an equation control when it wants to delete itself. We must remove 
        /// it from the stack panel.
        /// </summary>
        private void DeleteEquationRow(EquationControl thisOne)
        {
            UIElement uie = thisOne as UIElement;

#if DEBUG
            if (!EquationsGrid.Children.Contains(uie))
            {
                throw new ArgumentException(
                    "Request was made to delete an equation row that was not in the stack");
            }
#endif
            
            EquationsGrid.Children.Remove(uie);
        }

        /// <summary>
        /// Updates the list of scopes that an equation can reference
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

            // Update all of the equation controls
            for (int i = 0; i < EquationRowCount; i++)
            {
                EquationControl ec = GetRow(i);
                ec.SetScopeOptions(EquationScopes);
                UpdateEquationModelElements(ec.Model);
            }
        }

        /// <summary>
        /// E.O.
        /// For compatibility since I significantly changed the equation control stuff
        /// Will remove at a later date after some refactoring
        /// </summary>
        private List<EquationModel> equationModels
        {
            get
            {
                List<EquationModel> models = new List<EquationModel>();
                foreach (UIElement uie in EquationsGrid.Children)
                {
                    EquationControl ec = uie as EquationControl;
                    if (null == ec)
                    {
                        continue;
                    }

                    models.Add(ec.Model);
                }

                return models;
            }
        }

        /// <summary>
        /// Gets the equation control at the specified row index.
        /// </summary>
        /// <param name="index">Zero-based index of the row</param>
        /// <returns>Null if not found, reference to the appropriate EquationControl otherwise</returns>
        private EquationControl GetRow(int index)
        {
            if (index < 0)
            {
                return null;
            }

            int rowsSeen = 0;

            // Iterate through the itms in the stack panel of equation controls. At the time of this writing, 
            // the only items in here will be EquationControl objects, so we could just do:
            //   return EquationsGrid.Children[index] as EquationControl
            // However, in the future I could see things such as equation feedback elements being added beneath 
            // equation controls in this panel for usability purposes. Therefore we do an iteration through the 
            // children, which is more robust for the future.
            foreach (UIElement uie in EquationsGrid.Children)
            {
                EquationControl ec = uie as EquationControl;
                if (null == ec)
                {
                    continue;
                }

                if (index == rowsSeen)
                {
                    return ec;
                }

                // We have passed up another row
                rowsSeen++;
            }

            // Coming here implies that we didn't find it
            return null;
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

            // Update the type options for each row
            for (int i = 0; i < EquationRowCount; i++)
            {
                GetRow(i).SetTypeOptions(EquationTypes);
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

            // Find the index of the last row that has a non-empty equation in it
            int lastNonEmpty = -1;
            int i = EquationRowCount - 1;
            while (i >= 0)
            {
                if (!string.IsNullOrEmpty(GetRow(i).Model.Equation))
                {
                    lastNonEmpty = i;
                    break;
                }

                i--;
            }
        }

        public int EquationRowCount
        {
            get
            {
                int rowsSeen = 0;

                // Iterate through the itms in the stack panel of equation controls. At the time of this writing, 
                // the only items in here will be EquationControl objects, so we could just do:
                //   return EquationsGrid.Children.Count
                // However, in the future I could see things such as equation feedback elements being added beneath 
                // equation controls in this panel for usability purposes. Therefore we do an iteration through the 
                // children, which is more robust for the future.
                foreach (UIElement uie in EquationsGrid.Children)
                {
                    EquationControl ec = uie as EquationControl;
                    if (null == ec)
                    {
                        continue;
                    }

                    // We have passed up another row
                    rowsSeen++;
                }

                // Return the number of rows we saw
                return rowsSeen;
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

        //public ReadOnlyEquationModel GetEquationModel(int rowIndex)
        //{
        //    EquationModel em = GetRow(rowIndex).Model;
        //    if (null == em)
        //    {
        //        return null;
        //    }

        //    return new ReadOnlyEquationModel(em);
        //}

        /// <summary>
        /// Use for testing purposes only. Ideally in the future the commented-out version above would 
        /// be used so that equation model modifications could only happen through the control, but I 
        /// just haven't done all the appropriate refactoring at this time.
        /// </summary>
        public EquationModel GetEquationModel(int rowIndex)
        {
            return GetRow(rowIndex).Model;
        }

        public void AddNewEquationRow(EquationType type, EquationScope scope, string equation, string annotation)
        {
            EquationModel model = AddNewEquationRow();
            model.Type = type;
            model.Scope = scope;
            model.Equation = equation;
            model.Annotation = annotation;
        }

        private void AddNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewEquationRow();
        }

        private void TypeColumnHelpBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Indicates whether the equation expresses a fact given in the problem (problem " +
                "specification), indicates a total input or output, or refers to a specific compound.",
                "Equation Type", MessageBoxButton.OK);
        }

        private void ScopeColumnHelpBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Indicates the process unit or subprocess to which the equation refers. Choose \"" + 
                "Overall\" if the equation expresses a balance over the entire process as a whole.",
                "Equation Scope", MessageBoxButton.OK);
        }
    }
}