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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using ChemProV.Logic;
using ChemProV.Logic.Equations;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Undos;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationEditor : UserControl, IXmlSerializable
    {
        #region Delegates

        public event EventHandler EquationTokensChanged = delegate { };

        #endregion Delegates

        #region instance variables

        private List<string> elements = new List<string>();

        private ObservableCollection<EquationType> m_equationTypes = new ObservableCollection<EquationType>();

        /// <summary>
        /// List of process units that we've attached property-changed listeners to.
        /// </summary>
        private List<AbstractProcessUnit> m_monitoredProcessUnits = new List<AbstractProcessUnit>();

        /// <summary>
        /// List of streams that we've attached property-change listeners to.
        /// </summary>
        private List<AbstractStream> m_monitoredStreams = new List<AbstractStream>();

        /// <summary>
        /// List of stream property tables that we've attached row-change listeners to. Gets updated 
        /// when the collection of streams in the workspace changes.
        /// </summary>
        private List<StreamPropertiesTable> m_monitoredTables = new List<StreamPropertiesTable>();

        private Workspace m_workspace = null;

        #endregion

        #region Properties

        private static Brush s_grayBrush = new SolidColorBrush(Colors.Gray);

        private static Brush s_lightGrayBrush = new SolidColorBrush(Colors.LightGray);

        public ObservableCollection<EquationType> EquationTypes
        {
            get { return m_equationTypes; }
        }

        public ObservableCollection<EquationScope> EquationScopes { get; private set; }

        #endregion Properties

        #region Constructor

        public EquationEditor()
        {
            InitializeComponent();

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

        #endregion

        #region Private methods

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            EquationRowControl row = null;

            // Start by finding the row index in the stack panel
            int indexOfThis = -1;
            for (int i = 0; i < EquationsStackPanel.Children.Count; i++)
            {
                // Will throw an exception if it the object is not an EquationControl, but that's 
                // what we want since the design contract is that all objects in the stack panel 
                // must be EquationControl objects.
                EquationRowControl ec = (EquationRowControl)EquationsStackPanel.Children[i];

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

            // Move the row down by removing it and then inserting it at a higher index. Event 
            // handlers are subscribed to changes in the equation collection, so the UI will 
            // be updated automatically.
            EquationModel toMoveDown = m_workspace.Equations[indexOfThis];
            m_workspace.Equations.Remove(toMoveDown);
            m_workspace.Equations.Insert(indexOfThis + 1, toMoveDown);

            // Add an undo that will move it back up
            m_workspace.AddUndo(new UndoRedoCollection("Undo moving equation down",
                new Logic.Undos.MoveEquationUp(indexOfThis + 1)));
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            EquationRowControl row = null;

            // Start by finding the row index in the stack panel
            int indexOfThis = -1;
            for (int i = 0; i < EquationsStackPanel.Children.Count; i++)
            {
                // Will throw an exception if it the object is not an EquationControl, but that's 
                // what we want since the design contract is that all objects in the stack panel 
                // must be EquationControl objects.
                EquationRowControl ec = (EquationRowControl)EquationsStackPanel.Children[i];

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

            // Move the row up by removing it and then inserting it at a lower index. Event 
            // handlers are subscribed to changes in the equation collection, so the UI will 
            // be updated automatically.
            EquationModel toMoveUp = m_workspace.Equations[indexOfThis];
            m_workspace.Equations.Remove(toMoveUp);
            m_workspace.Equations.Insert(indexOfThis - 1, toMoveUp);

            // Add an undo that will move it back down
            m_workspace.AddUndo(new UndoRedoCollection("Undo moving equation up",
                new Logic.Undos.MoveEquationDown(indexOfThis - 1)));
        }

        public void UpdateRowProperties()
        {
            int count = EquationRowCount;
            for (int i = 0; i < count; i++)
            {
                EquationRowControl ec = GetRow(i);

                // Row number label
                Brush clrBrush = new SolidColorBrush(Color.FromArgb(255, 42, 176, 240));
                ec.NumberLabel.Content = (i + 1).ToString() + ".";
                ec.NumberLabel.Foreground = clrBrush;

                // The "button" to show or hide comments for an equation can be in one of 4 states:
                // 1. There are comments for the equation and CommentsVisible is false
                // 2. There are comments for the equation and CommentsVisible is true
                // 3. There are no comments for the equation and CommentsVisible is false
                // 4. There are no comments for the equation and CommentsVisible is true

                // If the comments are visible, then we have a colored border and background
                if (ec.Model.CommentsVisible)
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

                // Ensure that the font size is correct
                ec.EquationTextBox.FontSize = m_workspace.EquationEditorFontSize;
            }
        }

        /// <summary>
        /// This is called from an equation control when it wants to delete itself. We must remove 
        /// it from the stack panel.
        /// </summary>
        private void DeleteEquationRow(EquationRowControl thisOne)
        {
            int index = m_workspace.Equations.IndexOf(thisOne.Model);

#if DEBUG
            if (index < 0)
            {
                throw new ArgumentException(
                    "Request was made to delete an equation that was not found in the collection");
            }
#endif

            // Create an undo that will add it back
            m_workspace.AddUndo(new UndoRedoCollection(
                "Undo deleting equation row",
                new InsertEquation(m_workspace, thisOne.Model, index)));

            // Remove it from the workspace. There are event listeners that will update the UI 
            // when doing this
            m_workspace.Equations.Remove(thisOne.Model);

            // Fix row numbers and buttons
            UpdateRowProperties();
        }

        /// <summary>
        /// Gets the equation control at the specified row index.
        /// </summary>
        /// <param name="index">Zero-based index of the row.</param>
        /// <returns>Null if index is invalid, reference to the appropriate EquationControl otherwise.</returns>
        private EquationRowControl GetRow(int index)
        {
            if (index < 0 || index >= EquationsStackPanel.Children.Count)
            {
                return null;
            }

            return (EquationRowControl)EquationsStackPanel.Children[index];
        }

        /// <summary>
        /// Updates the list of available compounds that an equation can balance across
        /// </summary>
        public void UpdateCompounds()
        {
            if (null == m_workspace)
            {
                return;
            }

            m_equationTypes = BuildTypeOptions(m_workspace);

            // Update the type options for each row
            for (int i = 0; i < EquationRowCount; i++)
            {
                GetRow(i).SetTypeOptions(EquationTypes);
            }
        }

        public static ObservableCollection<EquationType> BuildTypeOptions(Workspace workspace)
        {
            IList<string> compounds = WorkspaceUtility.GetUniqueSelectedCompounds(workspace);

            List<string> elements = new List<string>();
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

            ObservableCollection<EquationType> equationTypes = new ObservableCollection<EquationType>();
            equationTypes.Add(new EquationType(EquationTypeClassification.Total, "Total"));
            equationTypes.Add(new EquationType(EquationTypeClassification.Specification, "Specification"));
            equationTypes.Add(new EquationType(EquationTypeClassification.Basis, "Basis"));

            foreach (string compound in compounds)
            {
                if (compound != "Overall")
                {
                    equationTypes.Add(new EquationType(EquationTypeClassification.Compound, compound));
                }
            }

            if (workspace.Difficulty != OptionDifficultySetting.MaterialBalance)
            {
                foreach (string element in elements)
                {
                    equationTypes.Add(new EquationType(EquationTypeClassification.Atom, element + "(e)"));
                }
            }

            return equationTypes;
        }

        private List<object> GetElementAndStreams(AbstractProcessUnit unit)
        {
            List<object> elements = new List<object>();

            // Add the process unit as well as its incoming and outgoing streams
            elements.Add(unit);
            foreach (AbstractStream element in unit.IncomingStreams)
            {
                elements.Add(element);
            }
            foreach (AbstractStream element in unit.OutgoingStreams)
            {
                elements.Add(element);
            }
            return elements;
        }

        private void UpdateEquationModelElements(EquationModel equation)
        {
            List<object> relevantUnits = new List<object>();

            //supply different PFD elements to the model depending on its scope
            switch (equation.Scope.Classification)
            {
                //With a single unit, all we care about is that unit and its related streams
                case EquationScopeClassification.SingleUnit:
                    AbstractProcessUnit selectedUnit = (from element in m_workspace.ProcessUnits
                                                        where (element as AbstractProcessUnit).Label == equation.Scope.Name
                                                        select element).FirstOrDefault() as AbstractProcessUnit;
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
                    // TODO: Fix or remove

                    //List<GenericProcessUnit> units = (from element in PfdElements
                    //                            where element is GenericProcessUnit
                    //                            &&
                    //                            (
                    //                             (element as GenericProcessUnit).Description == ProcessUnitDescriptions.Sink
                    //                             ||
                    //                             (element as GenericProcessUnit).Description == ProcessUnitDescriptions.Source
                    //                            )
                    //                            select element as GenericProcessUnit).ToList();
                    //foreach (GenericProcessUnit unit in units)
                    //{
                    //    relevantUnits = relevantUnits.Union(GetElementAndStreams(unit)).ToList();
                    //}
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
        private void GenericProcessUnitPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
                    EquationRowControl ec = uie as EquationRowControl;
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
            foreach (EquationModel model in m_workspace.Equations)
            {
                serializer.Serialize(writer, model);
            }
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members

        public EquationModel GetEquationModel(int rowIndex)
        {
            return GetRow(rowIndex).Model;
        }

        private void AddNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            // We can't do anything here without a workspace reference
            if (null == m_workspace)
            {
                return;
            }

            // Create a new equation mode and add it to the workspace. Event 
            // listeners will update the UI appropriately.
            m_workspace.Equations.Add(new EquationModel());

            // Add an undo that will delete that row
            m_workspace.AddUndo(new UndoRedoCollection(
                "Undo adding new equation row",
                new RemoveEquation(m_workspace, m_workspace.Equations.Count - 1)));
        }

        internal EquationRowControl GetRowControl(int index)
        {
            return EquationsStackPanel.Children[index] as EquationRowControl;
        }

        public int CountRowsWithCommentsVisible()
        {
            int count = 0;
            foreach (UIElement uie in EquationsStackPanel.Children)
            {
                if ((uie as EquationRowControl).Model.CommentsVisible)
                {
                    count++;
                }
            }
            return count;
        }

        public void SetWorkspace(Workspace workspace)
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

            UpdateCompounds();
            updateScopes();

            UpdateFromWorkspace();

            // Attach listeners
            m_workspace.Equations.CollectionChanged += new NotifyCollectionChangedEventHandler(Equations_CollectionChanged);
            m_workspace.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(WorkspacePropertyChanged);
            m_workspace.ProcessUnitsCollectionChanged += new EventHandler(Workspace_ProcessUnitsCollectionChanged);
            m_workspace.StreamsCollectionChanged += new EventHandler(WorkspaceStreamsCollectionChanged);
        }

        private void Equations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateFromWorkspace();
        }

        private void SetupEquationControlEvents(EquationRowControl control)
        {
            // Set the deletion callback function
            control.SetDeleteRequestDelegate(this.DeleteEquationRow);

            // TODO: Perhaps fix at a later date (the commented-out line)
            //control.Model.RelatedElements = PfdElements;
            control.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(EquationModelPropertyChanged);

            // Link up events for move up/move down buttons
            control.MoveDownButton.Click += new RoutedEventHandler(MoveDownButton_Click);
            control.MoveUpButton.Click += new RoutedEventHandler(MoveUpButton_Click);

            // Set comment button border
            control.CommentIconBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
            control.CommentIconBorder.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                // Toggle the comment visibility. All relevant UI elements that need to know about this 
                // will have attached event listeners and will update themselves appropriately
                control.Model.CommentsVisible = !control.Model.CommentsVisible;

                UpdateRowProperties();

                // If we have any rows with visible comments then ensure the comments pane is visible
                if (CountRowsWithCommentsVisible() > 0)
                {
                    Core.App.Workspace.CommentsPaneVisible = true;
                }
            };
        }

        /// <summary>
        /// Fired when a row in a chemical stream properties table changes. The reason we're interested 
        /// in this is because the "Type" options in the equations editor come from the collection of 
        /// selected compounds in the stream property tables.
        /// </summary>
        private void TableRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedCompound"))
            {
                UpdateCompounds();
            }
        }

        private void ProcessUnitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateScopes();
        }

        /// <summary>
        /// Updates the UI and the set of equation controls based on data from the workspace object
        /// </summary>
        private void UpdateFromWorkspace()
        {
            // Tests show that creating the equation row controls is somewhat slow, so here we want 
            // to minimize the number of controls we create. Thus instead of clearing out all 
            // controls and creating new ones, which would certainly work, we'll take a more efficient 
            // approach. We start by creating the exact number of row controls that are needed. This 
            // may require deleting or adding rows.

            // If there are too many equation row controls, then we have to delete
            while (EquationsStackPanel.Children.Count > m_workspace.Equations.Count)
            {
                // Remove the last one
                EquationRowControl ec =
                    EquationsStackPanel.Children[EquationsStackPanel.Children.Count - 1] as EquationRowControl;
                ec.SetModel(null);
                EquationsStackPanel.Children.Remove(ec);
            }

            // If there are too few equation row controls then add additional ones
            while (EquationsStackPanel.Children.Count < m_workspace.Equations.Count)
            {
                EquationRowControl ec = new EquationRowControl(m_workspace,
                    this, m_workspace.Equations[EquationsStackPanel.Children.Count]);
                SetupEquationControlEvents(ec);
                EquationsStackPanel.Children.Add(ec);
            }

            // Now we have the correct number of controls to match the number of equations in the workspace. Go 
            // through each one and set the data.
            for (int i = 0; i < m_workspace.Equations.Count; i++)
            {
                // If the control doesn't already have the correct model then update it
                if (!object.ReferenceEquals(m_workspace.Equations[i],
                    (EquationsStackPanel.Children[i] as EquationRowControl).Model))
                {
                    (EquationsStackPanel.Children[i] as EquationRowControl).SetModel(m_workspace.Equations[i]);
                }
            }

            // Fix the move up/move down buttons on all rows
            UpdateRowProperties();
        }

        /// <summary>
        /// Updates the list of scopes that an equation can reference
        /// </summary>
        private void updateScopes()
        {
            // Can't do anything without a workspace
            if (null == m_workspace)
            {
                return;
            }

            EquationScopes = new ObservableCollection<EquationScope>();

            //add "overall" scope
            EquationScopes.Add(new EquationScope(EquationScopeClassification.Overall, Name = "Overall"));

            //add "Unknown" scope
            EquationScopes.Add(new EquationScope(EquationScopeClassification.Unknown, Name = "Unknown"));

            // Add any process units and subprocesses to the list of possible scopes
            foreach (AbstractProcessUnit apu in m_workspace.ProcessUnits)
            {
                EquationScopes.Add(new EquationScope(EquationScopeClassification.SingleUnit, Name = apu.Label));

                // If there's a non-default subprocess for this process unit, then add it
                if (!apu.Subprocess.ToLower().Equals("#ffffffff"))
                {
                    // Find the name of this color
                    string name = null;
                    foreach (Core.NamedColor nc in Core.NamedColors.All)
                    {
                        if (nc.Color.ToString().Equals(apu.Subprocess))
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

            //add streams as well
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                EquationScopes.Add(new EquationScope(EquationScopeClassification.Unknown, string.Format("Stream #{0}", stream.Id)));
            }

            // Update all of the equation controls
            for (int i = 0; i < EquationRowCount; i++)
            {
                EquationRowControl ec = GetRow(i);
                ec.SetScopeOptions(EquationScopes);
                UpdateEquationModelElements(ec.Model);
            }
        }

        private void Workspace_ProcessUnitsCollectionChanged(object sender, EventArgs e)
        {
            // Dettach from old and attach to new
            foreach (AbstractProcessUnit apu in m_monitoredProcessUnits)
            {
                apu.PropertyChanged -= this.ProcessUnitPropertyChanged;
            }
            m_monitoredProcessUnits.Clear();

            foreach (AbstractProcessUnit apu in m_workspace.ProcessUnits)
            {
                m_monitoredProcessUnits.Add(apu);
                apu.PropertyChanged += this.ProcessUnitPropertyChanged;
            }
            updateScopes();
        }

        void StreamPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateScopes();
        }

        private void WorkspacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("EquationEditorFontSize"))
            {
                // Update the font size for all rows
                UpdateRowProperties();
            }
        }

        private void WorkspaceStreamsCollectionChanged(object sender, EventArgs e)
        {
            // Start by unsubscribing from the old list
            foreach (StreamPropertiesTable table in m_monitoredTables)
            {
                table.RowPropertyChanged -= this.TableRowPropertyChanged;
            }

            // Rebuild the list and subsribe to changes in chemical stream property tables
            m_monitoredTables.Clear();
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                ChemicalStream cs = stream as ChemicalStream;
                if (null == cs)
                {
                    // Ignore it if it's not a chemical stream
                    continue;
                }

                StreamPropertiesTable table = cs.PropertiesTable;
                if (null == table)
                {
                    throw new InvalidOperationException(
                        "Stream in workspace has a null table");
                }
                m_monitoredTables.Add(table);
                table.RowPropertyChanged += this.TableRowPropertyChanged;
            }

            //this code handles changes made to streams for the purposes of the scopes menu
            foreach (AbstractStream stream in m_monitoredStreams)
            {
                stream.PropertyChanged -= this.StreamPropertyChanged;
            }
            m_monitoredStreams.Clear();
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                m_monitoredStreams.Add(stream);
                stream.PropertyChanged += new PropertyChangedEventHandler(StreamPropertyChanged);
            }
            UpdateCompounds();
            updateScopes();
        }
    }
}