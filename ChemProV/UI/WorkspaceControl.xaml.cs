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
    public partial class WorkspaceControl : UserControl
    {
        #region Delegates

        //public event EventHandler ToolPlaced = delegate { };
        public event EventHandler ValidationChecked = delegate { };

        #endregion Delegates

        #region Fields

        private bool isLoadingFile = false;

        private RuleManager ruleManager = RuleManager.GetInstance();

        private bool checkRules = true;

        private ObservableCollection<string> compounds = new ObservableCollection<string>();

        private ObservableCollection<string> elements = new ObservableCollection<string>();

        /// <summary>
        /// Dictionary used to map a user name to a sticky note color
        /// </summary>
        private Dictionary<string, PFD.StickyNote.StickyNoteColors> m_snUserColors =
            new Dictionary<string, PFD.StickyNote.StickyNoteColors>();

        private Core.Workspace m_workspace = null;

        #endregion Fields

        #region Constructor

        public WorkspaceControl()
        {
            InitializeComponent();

            DrawingCanvas.PfdChanging += new EventHandler(DrawingCanvas_PfdChanging);
            DrawingCanvas.PfdUpdated += new PfdUpdatedEventHandler(CheckRulesForPFD);
            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForPFD);

            SizeChanged += new SizeChangedEventHandler(WorkSpace_SizeChanged);

            CommentsPane.CloseButton.Click += new RoutedEventHandler(CloseCommentPaneButton_Click);
        }

        private void CloseCommentPaneButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < EquationEditor.EquationRowCount; i++)
            {
                EquationControl ec = EquationEditor.GetRowControl(i);
                ec.Model.CommentsVisible = false;
            }
            EquationEditor.UpdateRowProperties();

            m_workspace.DegreesOfFreedomAnalysis.CommentsVisible = false;

            // With all of the comments hidden, the update function will hide the pane
            UpdateCommentsPaneVisibility();
        }

        #endregion Constructor

        #region Properties

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
            if (e.Key == Key.Z && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                Undo();
                e.Handled = true;
            }
            else if (e.Key == Key.Y && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                Redo();
                e.Handled = true;
            }
            else
            {
                DrawingCanvas.GotKeyDown(sender, e);
            }
        }

        public void Undo()
        {
            // Go back to the null state for the drawing canvas
            DrawingCanvas.CurrentState = null;

            // Execute the workspace undo
            m_workspace.Undo();
        }

        public void Redo()
        {
            // Go back to the null state for the drawing canvas
            DrawingCanvas.CurrentState = null;

            // Execute the workspace redo
            m_workspace.Redo();
        }

        public void ClearWorkSpace()
        {
            // Clear the workspace
            m_workspace.Clear();

            // Add the default equation row
            m_workspace.Equations.Add(new PFD.EquationEditor.Models.EquationModel());
            
            //now, clear the drawing drawing_canvas
            DrawingCanvas.ClearDrawingCanvas();

            //clear any existing messages in the feedback window and rerun the error checker
            CheckRulesForPFD(this, EventArgs.Empty);

            UpdateCommentsPaneVisibility();
        }

        /// <summary>
        /// This fires when an equation is changed
        /// TODO: This is old code. See if it's even needed anymore
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

        //public void LoadXmlElements(XDocument doc)
        //{
        //    isLoadingFile = true;
        //    //clear out previous data
        //    DrawingCanvas.ClearDrawingCanvas();
        //    m_snUserColors.Clear();

        //    //some items don't have feedback so there might not be a feedbackwindow element.
        //    if (doc.Descendants("FeedbackWindow").Count() > 0)
        //    {
        //        FeedbackWindow.LoadXmlElements(doc.Descendants("FeedbackWindow").ElementAt(0));
        //    }

        //    //done loading the file so set isLoadingFile to false and call the CheckRulesForPFD to check the rules
        //    isLoadingFile = false;

        //    //AC: The function will update the equation editor's list of scope and type options.  This needs to be up to date
        //    //before we can load the equation editor.
        //    CheckRulesForPFD(this, EventArgs.Empty);

        //    // Update the equations
        //    EquationEditor.UpdateCompounds();

        //    UpdateCommentsPaneVisibility();
        //}

        public object GetobjectFromId(string id)
        {
            return null;
        }

        #endregion Public Methods

        #region Private Helper

        //private void UpdateCompounds(IEnumerable<IPropertiesWindow> iPropertiesWindows)
        //{
        //    ITableAdapter tableAdapter;

        //    compounds.Clear();

        //    foreach (IPfdElement ipfd in iPropertiesWindows)
        //    {
        //        if (ipfd is IPropertiesWindow)
        //        {
        //            tableAdapter = TableAdapterFactory.CreateTableAdapter(ipfd as IPropertiesWindow);
        //            int i = 0;
        //            while (i < tableAdapter.GetRowCount())
        //            {
        //                string compound = tableAdapter.GetCompoundAtRow(i);
        //                if (compound != "Select" && compound != "Overall" && compound.Length > 0)
        //                {
        //                    if (!compounds.Contains(compound))
        //                    {
        //                        compounds.Add(compound);
        //                    }
        //                }
        //                i++;
        //            }
        //        }
        //    }
            
        //    // Don't need this anymore because the equation editor handles it
        //    //EquationEditor.Compounds = compounds;
        //    CompoundsUpdated(this, EventArgs.Empty);
        //}

        private void DrawingCanvas_PfdChanging(object sender, EventArgs e)
        {
            FeedbackWindow.FeedbackStatusChanged(FeedbackStatus.ChangedButNotChecked);
        }

        private void WorkSpace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //FixSizeOfComponents();
        }

        #endregion Private Helper

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

            // Set it for the drawing canvas too
            DrawingCanvas.SetWorkspace(workspace);
        }

        /// <summary>
        /// Dictionary that maps a user name string to a sticky note color
        /// </summary>
        public Dictionary<string, PFD.StickyNote.StickyNoteColors> UserStickyNoteColors
        {
            get
            {
                return m_snUserColors;
            }
        }

        public Rect VisiblePFDArea
        {
            get
            {
                double vBarWidth =
                    (ScrollBarVisibility.Visible == DrawingCanvasScollViewer.VerticalScrollBarVisibility) ?
                    30.0 : 0.0;
                double hBarHeight = 
                    (ScrollBarVisibility.Visible == DrawingCanvasScollViewer.HorizontalScrollBarVisibility) ? 
                    30.0 : 0.0;
                return new Rect(
                    DrawingCanvasScollViewer.HorizontalOffset,
                    DrawingCanvasScollViewer.VerticalOffset,
                    DrawingCanvasScollViewer.Width - vBarWidth,
                    DrawingCanvasScollViewer.Height - hBarHeight);
            }
        }

        public void UpdateCommentsPaneVisibility()
        {
            // If there are no comments visible then hide the pane and return
            if (0 == EquationEditor.CountRowsWithCommentsVisible() &&
                !m_workspace.DegreesOfFreedomAnalysis.CommentsVisible)
            {
                CommentsPane.Visibility = System.Windows.Visibility.Collapsed;
                WorkspaceGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
                return;
            }

            // Otherwise make sure its visible and then update
            CommentsPane.Visibility = System.Windows.Visibility.Visible;
            WorkspaceGrid.ColumnDefinitions[1].Width = new GridLength(175.0);
        }
    }
}