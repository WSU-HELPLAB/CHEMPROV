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
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD.Streams.PropertiesWindow;
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

        private List<IPfdElement> pfdElements = new List<IPfdElement>();

        private ChemProV.Core.Workspace m_workspace = null;

        #endregion

        #region Properties

        private static Brush s_grayBrush = new SolidColorBrush(Colors.Gray);

        private static Brush s_lightGrayBrush = new SolidColorBrush(Colors.LightGray);

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
                UpdateCompounds();
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
            UpdateCompounds();
            updateScopes();

            // Set tooltips at runtime
            TextBlock tbTypeTip = new TextBlock();
            tbTypeTip.Text = "Indicates whether the equation expresses a fact given in the problem (problem\n" +
                "specification), indicates a total input or output, or refers to a specific compound.";
            ToolTipService.SetToolTip(TypeColumnHelpIcon, tbTypeTip);
            TextBlock tbScopeTip = new TextBlock();
            tbScopeTip.Text = "Indicates the process unit or subprocess to which the equation refers. Choose\n\"" +
                "Overall\" if the equation expresses a balance over the entire process as a whole.";
            ToolTipService.SetToolTip(ScopeColumnHelpIcon, tbScopeTip);
        }
        #endregion Constructor

        #region public methods

        /// <summary>
        /// Will remove all existing equations currently listed in the equation editor. Can 
        /// optionally add in the default blank row if desired.
        /// </summary>
        public void ClearEquations(bool addDefaultBlank)
        {
            EquationsStackPanel.Children.Clear();

            if (addDefaultBlank)
            {
                AddNewEquationRow();
            }
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
            // We can't do anything here without a workspace reference
            if (null == m_workspace)
            {
                return null;
            }
            
            // Create a new equation model
            EquationModel model;
            if (null == optionalXmlEquation)
            {
                model = new EquationModel();
            }
            else
            {
                model = EquationModel.FromXml(optionalXmlEquation);
            }

            // Add it to the workspace. Event listeners will update the UI appropriately.
            m_workspace.Equations.Add(model);

            return (EquationsStackPanel.Children[EquationsStackPanel.Children.Count - 1] as EquationControl).Model;
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            EquationControl row = null;
            
            // Start by finding the row index in the stack panel
            int indexOfThis = -1;
            for (int i = 0; i < EquationsStackPanel.Children.Count; i++)
            {
                // Will throw an exception if it the object is not an EquationControl, but that's 
                // what we want since the design contract is that all objects in the stack panel 
                // must be EquationControl objects.
                EquationControl ec = (EquationControl)EquationsStackPanel.Children[i];
                
                if (object.ReferenceEquals(sender, ec.MoveDownButton))
                {
                    indexOfThis = i;
                    row = ec;
                    break;
                }
            }


            // If it's the last row then disable the button to move down and return
            if (indexOfThis == EquationRowCount - 1)
            {
                row.MoveDownButton.IsEnabled = false;
                return;
            }

            // Move it down by removing it then inserting it
            EquationsStackPanel.Children.Remove(row);
            EquationsStackPanel.Children.Insert(indexOfThis + 1, row);

            FixNumsAndButtons();
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            EquationControl row = null;

            // Start by finding the row index in the stack panel
            int indexOfThis = -1;
            for (int i = 0; i < EquationsStackPanel.Children.Count; i++)
            {
                // Will throw an exception if it the object is not an EquationControl, but that's 
                // what we want since the design contract is that all objects in the stack panel 
                // must be EquationControl objects.
                EquationControl ec = (EquationControl)EquationsStackPanel.Children[i];

                if (object.ReferenceEquals(sender, ec.MoveUpButton))
                {
                    indexOfThis = i;
                    row = ec;
                    break;
                }
            }


            // If it's the first row then disable the button to move up and return
            if (0 == indexOfThis)
            {
                row.MoveUpButton.IsEnabled = false;
                return;
            }

            // Move it down by removing it then inserting it
            EquationsStackPanel.Children.Remove(row);
            EquationsStackPanel.Children.Insert(indexOfThis - 1, row);

            FixNumsAndButtons();
        }

        /// <summary>
        /// Sets the proper number for each row as well as the and IsEnabled state for the move up and move down buttons
        /// </summary>
        public void FixNumsAndButtons()
        {
            int count = EquationRowCount;
            for (int i = 0; i < count; i++)
            {
                EquationControl ec = GetRow(i);

                // Row number label
                Brush clrBrush = new SolidColorBrush(Color.FromArgb(255,42,176,240));
                ec.NumberLabel.Content = (i + 1).ToString() + ".";
                ec.NumberLabel.Foreground = clrBrush;

                // The "button" to show or hide comments for an equation can be in one of 4 states:
                // 1. There are comments for the equation and CommentsVisible is false
                // 2. There are comments for the equation and CommentsVisible is true
                // 3. There are no comments for the equation and CommentsVisible is false
                // 4. There are no comments for the equation and CommentsVisible is true

                // If the comments are visible, then we have a colored border and background
                if (ec.CommentsVisible)
                {
                    ec.CommentIconBorder.BorderBrush = ec.CommentIconBorder.Background = clrBrush;
                    ToolTipService.SetToolTip(ec.CommentIconBorder,
                        "Click to hide comments for this equation in the side pane");
                }
                else
                {
                    // Otherwise, if the comments are hidden, then we want some sort of visual cue to indicate whether 
                    // or not there are any comments for that equation. We will do this by setting the border brush to gray 
                    // if there are no comments and setting it to colored otherwise. The background will be gray in either 
                    // case.
                    ec.CommentIconBorder.BorderBrush = (ec.Model.Comments.Count > 0) ?
                        clrBrush : s_grayBrush;
                    ec.CommentIconBorder.Background = s_lightGrayBrush;

                    // Also set a tooltip
                    if (ec.Model.Comments.Count > 0)
                    {
                        ToolTipService.SetToolTip(ec.CommentIconBorder,
                            "There are comments for this equation, click to show them");
                    }
                    else
                    {
                        ToolTipService.SetToolTip(ec.CommentIconBorder,
                            "There are no comments for this equation, click to show the comment editor");
                    }
                }

                // Up/down buttons
                ec.MoveUpButton.IsEnabled = (i != 0);
                ec.MoveDownButton.IsEnabled = (i < count - 1);
            }
        }

        /// <summary>
        /// This is called from an equation control when it wants to delete itself. We must remove 
        /// it from the stack panel.
        /// </summary>
        private void DeleteEquationRow(EquationControl thisOne)
        {
            UIElement uie = thisOne as UIElement;

#if DEBUG
            if (!EquationsStackPanel.Children.Contains(uie))
            {
                throw new ArgumentException(
                    "Request was made to delete an equation row that was not in the stack");
            }
#endif

            // Remove it from the workspace. There are event listeners that will update the UI 
            // when doing this
            m_workspace.Equations.Remove(thisOne.Model);
            
            // Fix row numbers and buttons
            FixNumsAndButtons();
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
                foreach (UIElement uie in EquationsStackPanel.Children)
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
        /// <param name="index">Zero-based index of the row.</param>
        /// <returns>Null if index is invalid, reference to the appropriate EquationControl otherwise.</returns>
        private EquationControl GetRow(int index)
        {
            if (index < 0 || index >= EquationsStackPanel.Children.Count)
            {
                return null;
            }

            return (EquationControl)EquationsStackPanel.Children[index];
        }

        private int GetRowIndex(EquationControl row)
        {
            for (int i = 0; i < EquationsStackPanel.Children.Count; i++)
            {
                if (object.ReferenceEquals(row, EquationsStackPanel.Children[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Updates the list of available compounds that an equation can balance across
        /// </summary>
        public void UpdateCompounds()
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
        }

        public int EquationRowCount
        {
            get
            {                
                int rowsSeen = 0;
                foreach (UIElement uie in EquationsStackPanel.Children)
                {
                    EquationControl ec = uie as EquationControl;
                    if (null == ec)
                    {
                        throw new InvalidOperationException(
                            "Found a control that was not an EquationControl in the equation control stack. " +
                            "Control was: " + uie.ToString());
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
            model.Comments.Add(new Core.BasicComment(annotation, null));
        }

        private void AddNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewEquationRow();
        }

        internal EquationControl GetRowControl(int index)
        {
            return EquationsStackPanel.Children[index] as EquationControl;
        }

        public int CountRowsWithCommentsVisible()
        {
            int count = 0;
            foreach (UIElement uie in EquationsStackPanel.Children)
            {
                if ((uie as EquationControl).CommentsVisible)
                {
                    count++;
                }
            }
            return count;
        }

        public void SetWorkspace(ChemProV.Core.Workspace workspace)
        {
            if (object.ReferenceEquals(m_workspace, workspace))
            {
                // No change
                return;
            }

            // Detach listeners from old workspace
            if (null != m_workspace)
            {
                // This function should really only be called once, so we should never hit this 
                // code, but future versions might change this.
                throw new NotImplementedException();
            }

            // Store a reference to the workspace
            m_workspace = workspace;

            UpdateFromWorkspace();

            // Attach listeners
            m_workspace.Equations.CollectionChanged += new NotifyCollectionChangedEventHandler(Equations_CollectionChanged);
        }

        private void Equations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (NotifyCollectionChangedAction.Add == e.Action)
            {
                // An item was added, meaning we need to add a new row. In this case the number of rows 
                // that we currently have should be one less than the number in the workspace.
                if (EquationsStackPanel.Children.Count != m_workspace.Equations.Count - 1)
                {
                    // Do a full update for safety
                    UpdateFromWorkspace();
                    return;
                }

                EquationControl ec = new EquationControl(
                    this, m_workspace.Equations[m_workspace.Equations.Count - 1]);
                SetupEquationControlEvents(ec);
                EquationsStackPanel.Children.Add(ec);
                FixNumsAndButtons();
            }
            else
            {
                // Could probably make this more efficient
                UpdateFromWorkspace();
            }
        }

        /// <summary>
        /// Clears and re-creates the entire set of equation controls based on data from the workspace object
        /// </summary>
        private void UpdateFromWorkspace()
        {
            // Create appropriate UI elements for the workspace content
            ClearEquations(false);
            foreach (EquationModel em in m_workspace.Equations)
            {
                EquationControl ec = new EquationControl(this, em);
                SetupEquationControlEvents(ec);
                EquationsStackPanel.Children.Add(ec);
            }

            // Fix the move up/move down buttons on all rows
            FixNumsAndButtons();
        }

        private void SetupEquationControlEvents(EquationControl control)
        {
            // Set the deletion callback function
            control.SetDeleteRequestDelegate(this.DeleteEquationRow);

            control.Model.RelatedElements = PfdElements;
            control.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(EquationModelPropertyChanged);

            // Link up events for move up/move down buttons
            control.MoveDownButton.Click += new RoutedEventHandler(MoveDownButton_Click);
            control.MoveUpButton.Click += new RoutedEventHandler(MoveUpButton_Click);

            // Set comment button border
            control.CommentsVisible = false;
            control.CommentIconBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
            control.CommentIconBorder.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                // Toggle the comment visibility. All relevant UI elements that need to know about this 
                // will have attached event listeners and will update themselves appropriately
                control.CommentsVisible = !control.CommentsVisible;

                FixNumsAndButtons();

                // Tell the workspace to update visibility for the comment pane
                Core.App.Workspace.UpdateCommentsPaneVisibility();
            };
        }
    }
}