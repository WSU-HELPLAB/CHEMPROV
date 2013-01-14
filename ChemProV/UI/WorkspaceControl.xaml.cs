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
using ChemProV.Validation.Feedback;
using ChemProV.Validation.Rules;
using ChemProV.Validation.Rules.Adapters.Table;
using ChemProV.Logic;

namespace ChemProV.UI
{
    public partial class WorkspaceControl : UserControl, INotifyPropertyChanged
    {
        #region Delegates

        public event EventHandler ValidationChecked = delegate { };

        #endregion Delegates

        #region Fields

        private bool isLoadingFile = false;

        private RuleManager ruleManager = RuleManager.GetInstance();

        private bool checkRules = true;

        private ObservableCollection<string> elements = new ObservableCollection<string>();

        private bool m_commentsPaneVisible = false;

        /// <summary>
        /// Dictionary used to map a user name to a sticky note color
        /// </summary>
        private Dictionary<string, StickyNoteColors> m_snUserColors =
            new Dictionary<string, StickyNoteColors>();

        private Workspace m_workspace = null;

        #endregion Fields

        #region Constructor

        public WorkspaceControl()
        {
            InitializeComponent();

            DrawingCanvas.PfdChanging += new EventHandler(DrawingCanvas_PfdChanging);
            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForPFD);

            SizeChanged += new SizeChangedEventHandler(WorkSpace_SizeChanged);

            CommentsPane.CloseButton.Click += new RoutedEventHandler(CloseCommentPaneButton_Click);
        }

        private void CloseCommentPaneButton_Click(object sender, RoutedEventArgs e)
        {
            // Commenting stuff out below until we discuss this in a meeting. I'm thinking that 
            // your selection of which equation comments are visible as well as whether or not 
            // DF analysis comments are visible should persist when you close the comment pane. 
            // That way when you show it again you are where you left off. The code below reset 
            // everything to hidden and I don't think we want that.
            
            //for (int i = 0; i < EquationEditor.EquationRowCount; i++)
            //{
            //    EquationControl ec = EquationEditor.GetRowControl(i);
            //    ec.Model.CommentsVisible = false;
            //}
            //EquationEditor.UpdateRowProperties();

            //m_workspace.DegreesOfFreedomAnalysis.CommentsVisible = false;

            CommentsPaneVisible = false;
        }

        #endregion Constructor

        #region Properties

        public ObservableCollection<string> Elements
        {
            get { return elements; }
            set { elements = value; }
        }

        [Obsolete("Significant changes have rendered this useless. Needs to be rewritten")]
        public bool CheckRules
        {
            get { return checkRules; }
            set { checkRules = value; }
        }

        /// <summary>
        /// gets a reference to the DrawingCanvas used by WorkSpace
        /// </summary>
        public DrawingCanvas DrawingCanvasReference
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
            m_workspace.Equations.Add(new Logic.Equations.EquationModel());
            
            //now, clear the drawing drawing_canvas
            DrawingCanvas.ClearDrawingCanvas();

            //clear any existing messages in the feedback window and rerun the error checker
            CheckRulesForPFD(this, EventArgs.Empty);

            CommentsPaneVisible = false;
        }

        /// <summary>
        /// This fires when an equation is changed
        /// TODO: This is old code. See if it's even needed anymore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [Obsolete("Significant changes have rendered this useless. Needs to be rewritten")]
        public void CheckRulesForPFD(object sender, EventArgs e)
        {
            //Stop listening for changed events since our ruleManager causes changes
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
            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForPFD);
        }

        [Obsolete("Significant changes have rendered this useless. Needs to be rewritten")]
        public object GetobjectFromId(string id)
        {
            return null;
        }

        #endregion Public Methods

        #region Private Helper

        private void DrawingCanvas_PfdChanging(object sender, EventArgs e)
        {
            FeedbackWindow.FeedbackStatusChanged(FeedbackStatus.ChangedButNotChecked);
        }

        private void WorkSpace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //FixSizeOfComponents();
        }

        #endregion Private Helper

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

            // Set it for the drawing canvas too
            DrawingCanvas.SetWorkspace(workspace);
        }

        /// <summary>
        /// Dictionary that maps a user name string to a sticky note color
        /// </summary>
        public Dictionary<string, StickyNoteColors> UserStickyNoteColors
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

        public bool CommentsPaneVisible
        {
            get
            {
                return m_commentsPaneVisible;
            }
            set
            {
                if (value == m_commentsPaneVisible)
                {
                    // No change
                    return;
                }
                
                m_commentsPaneVisible = value;
                if (value)
                {
                    CommentsPane.Visibility = System.Windows.Visibility.Visible;
                    WorkspaceGrid.ColumnDefinitions[1].Width = new GridLength(175.0);
                }
                else
                {
                    CommentsPane.Visibility = System.Windows.Visibility.Collapsed;
                    WorkspaceGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
                }

                PropertyChanged(this, new PropertyChangedEventArgs("CommentsPaneVisible"));
            }
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion Events
    }
}