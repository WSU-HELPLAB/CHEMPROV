/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using ChemProV.PFD;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.UI.DrawingCanvas;
using ChemProV.Validation.Feedback;
using ChemProV.Validation.Rules;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.UI
{
    public partial class WorkSpace : UserControl
    {
        #region Delegates

        public event EventHandler ToolPlaced = delegate { };
        public event EventHandler CompoundsUpdated = delegate { };
        public event EventHandler ValidationChecked = delegate { };

        #endregion Delegates

        #region Fields

        private bool isLoadingFile = false;

        private bool isReadOnly = false;

        private List<Tuple<string, EquationControl>> userDefinedVaraibles = new List<Tuple<string, EquationControl>>();

        private RuleManager ruleManager = RuleManager.GetInstance();

        private bool checkRules = true;

        private OptionDifficultySetting currentDifficultySetting;

        private ObservableCollection<string> compounds = new ObservableCollection<string>();

        private ObservableCollection<string> elements = new ObservableCollection<string>();

        #endregion Fields

        #region Constructor

        public WorkSpace()
        {
            InitializeComponent();

            //this will make the workspace and everything in it read-only
            //IsReadOnly = true;
            EquationEditor.IsReadOnly = isReadOnly;
            localInitializer(isReadOnly);
        }

        public WorkSpace(bool isReadOnly)
        {
            localInitializer(isReadOnly);
        }

        public void localInitializer(bool isReadOnly)
        {
            IsReadOnly = isReadOnly;
            DrawingCanvas.PfdChanging += new EventHandler(DrawingCanvas_PfdChanging);
            DrawingCanvas.ToolPlaced += new EventHandler(DrawingCanvas_ToolPlaced);
            DrawingCanvas.PfdUpdated += new PfdUpdatedEventHandler(CheckRulesForPFD);
            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForPFD);

            GridSplitter.MouseMove += new MouseEventHandler(GridSplitter_MouseMove);
            SizeChanged += new SizeChangedEventHandler(WorkSpace_SizeChanged);

            // E.O.
            Core.App.Init(this);
        }

        #endregion Constructor

        #region Properties

        public OptionDifficultySetting CurrentDifficultySetting
        {
            get { return currentDifficultySetting; }
            set
            {
                if (value != currentDifficultySetting)
                {
                    DifficultySettingChanged(currentDifficultySetting, value);
                    currentDifficultySetting = value;
                }
            }
        }

        public ObservableCollection<string> Compounds
        {
            get { return compounds; }
            set { compounds = value; }
        }

        public ObservableCollection<string> Elements
        {
            get { return elements; }
            set { elements = value; }
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                DrawingCanvas.IsReadOnly = value;
            }
        }

        public bool CheckRules
        {
            get { return checkRules; }
            set { checkRules = value; }
        }

        /// <summary>
        /// gets a reference to the DrawingCanvas used by WorkSpace
        /// </summary>
        public DrawingCanvas.DrawingCanvas DrawingCanvasReference
        {
            get
            {
                return DrawingCanvas;
            }
        }

        /// <summary>
        /// gets a reference to the EquationEditor used by WorkSpace
        /// </summary>
        public EquationEditor EquationEditorReference
        {
            get
            {
                return EquationEditor;
            }
        }

        /// <summary>
        /// gets a reference to the FeedbackWindow used by WorkSpace
        /// </summary>
        public FeedbackWindow FeedbackWindowReference
        {
            get
            {
                return FeedbackWindow;
            }
        }

        #endregion Properties

        #region Public Methods

        public void GotKeyDown(object sender, KeyEventArgs e)
        {
            DrawingCanvas.GotKeyDown(sender, e);
        }

        public void Redo()
        {
            //pass it on down
            DrawingCanvas.Redo();
        }

        public void Undo()
        {
            //pass it on down
            DrawingCanvas.Undo();
        }

        public void ClearWorkSpace()
        {
            //now, clear the drawing drawing_canvas
            DrawingCanvas.ClearDrawingCanvas();
            EquationEditor.ClearEquations();

            //clear any existing messages in the feedback window and rerun the error checker
            CheckRulesForPFD(this, EventArgs.Empty);
        }

        public void RemoveScrollViewerFromDrawingCanvas()
        {
            this.WorkspaceGrid.Children.Remove(this.DrawingCanvasScollViewer);
            this.DrawingCanvasScollViewer.Content = null;
            this.WorkspaceGrid.Children.Add(this.DrawingCanvas);
        }

        public void DifficultySettingChanged(OptionDifficultySetting oldValue, OptionDifficultySetting newValue)
        {
            DrawingCanvas.CurrentDifficultySetting = newValue;
            EquationEditor.CurrentDifficultySetting = newValue;
        }

        public void UserDefinedVariablesUpdated(List<Tuple<string, EquationControl>> newVariables)
        {
            userDefinedVaraibles = newVariables;
            CheckRulesForPFD(null, EventArgs.Empty);
        }

        /// <summary>
        /// This fires when an equation is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckRulesForPFD(object sender, EventArgs e)
        {
            //Stop listening for changed events since our ruleManager causes changes
            DrawingCanvas.PfdUpdated -= new PfdUpdatedEventHandler(CheckRulesForPFD);
            EquationEditor.EquationTokensChanged -= new EventHandler(CheckRulesForPFD);

            if (!isLoadingFile)
            {
                var iPropertiesWindows = from c in DrawingCanvas.ChildIPfdElements
                                         where c is IPropertiesWindow
                                         select c as IPropertiesWindow;

                UpdateCompounds(iPropertiesWindows);

                var pfdElements = DrawingCanvas.ChildIPfdElements;

                foreach (IPfdElement pfdElement in pfdElements)
                {
                    pfdElement.RemoveFeedback();
                }

                //AC TODO: Update rules validation for equations
                //ruleManager.Validate(pfdElements, EquationEditor.EquationsData, userDefinedVaraibles);
                FeedbackWindow.updateFeedbackWindow(ruleManager.ErrorMessages);
                ValidationChecked(this, EventArgs.Empty);
            }

            //ok done changing stuff listen for changed events again
            DrawingCanvas.PfdUpdated += new PfdUpdatedEventHandler(CheckRulesForPFD);
            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForPFD);
        }

        public void LoadXmlElements(XDocument doc)
        {
            isLoadingFile = true;
            //clear out previous data
            DrawingCanvas.ClearDrawingCanvas();
            EquationEditor.ClearEquations();

            //tell the drawing drawing_canvas to load its new children
            DrawingCanvas.LoadXmlElements(doc.Descendants("DrawingCanvas").ElementAt(0));

            //some items don't have feedback so there might not be a feedbackwindow element.
            if (doc.Descendants("FeedbackWindow").Count() > 0)
            {
                FeedbackWindow.LoadXmlElements(doc.Descendants("FeedbackWindow").ElementAt(0));
            }

            //done loading the file so set isLoadingFile to false and call the CheckRulesForPFD to check the rules
            isLoadingFile = false;

            //AC: The function will update the equation editor's list of scope and type options.  This needs to be up to date
            //before we can load the equation editor.
            CheckRulesForPFD(this, EventArgs.Empty);

            //Now, update the list of PFD elements
            EquationEditor.PfdElements = DrawingCanvas.ChildIPfdElements;

            //load the equations
            EquationEditor.LoadXmlElements(doc.Descendants("EquationEditor").ElementAt(0));

        }

        public object GetobjectFromId(string id)
        {
            return null;
        }

        #endregion Public Methods

        #region Private Helper

        private void UpdateCompounds(IEnumerable<IPropertiesWindow> iPropertiesWindows)
        {
            ITableAdapter tableAdapter;

            compounds.Clear();

            foreach (IPfdElement ipfd in iPropertiesWindows)
            {
                if (ipfd is IPropertiesWindow)
                {
                    tableAdapter = TableAdapterFactory.CreateTableAdapter(ipfd as IPropertiesWindow);
                    int i = 0;
                    while (i < tableAdapter.GetRowCount())
                    {
                        string compound = tableAdapter.GetCompoundAtRow(i);
                        if (compound != "Select" && compound != "Overall" && compound.Length > 0)
                        {
                            if (!compounds.Contains(compound))
                            {
                                compounds.Add(compound);
                            }
                        }
                        i++;
                    }
                }
            }
            EquationEditor.Compounds = compounds;
            CompoundsUpdated(this, EventArgs.Empty);
        }

        private void DrawingCanvas_PfdChanging(object sender, EventArgs e)
        {
            FeedbackWindow.FeedbackStatusChanged(FeedbackStatus.ChangedButNotChecked);
        }

        private void WorkSpace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FixSizeOfComponents();
        }

        private void DrawingCanvas_ToolPlaced(object sender, EventArgs e)
        {
            //update equation scope options
            EquationEditor.PfdElements = DrawingCanvas.ChildIPfdElements;
            ToolPlaced(this, EventArgs.Empty);
        }

        /// <summary>
        /// Since Canvas object don't auto-resize, this method needs to be called
        /// whenever the main window gets resized so that we can resize our drawing
        /// drawing_canvas appropriately.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_MouseMove(object sender, MouseEventArgs e)
        {
            FixSizeOfComponents();
        }

        private void FixSizeOfComponents()
        {
            //set the drawing_canvas' scroll viewer's size
            double height = WorkspaceGrid.RowDefinitions[0].ActualHeight;
            double width = WorkspaceGrid.ColumnDefinitions[0].ActualWidth;
            DrawingCanvasScollViewer.Width = width;
            DrawingCanvasScollViewer.Height = height;

            DrawingCanvas.MinHeight = height;
            DrawingCanvas.MinWidth = width;

            //set the feedback and equation window height
            height = WorkspaceGrid.RowDefinitions[2].ActualHeight;

            if (height < 33)
            {
                height = 33;
            }
            FeedbackWindow.Height = height;
            EquationEditor.Height = height;
        }

        #endregion Private Helper
    }
}