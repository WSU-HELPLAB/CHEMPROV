/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Linq;

using ChemProV;
using ChemProV.UI.DrawingCanvas;
using ChemProV.Validation.Rules;
using ChemProV.PFD;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.ProcessUnits;

namespace ChemProV.UI
{
    public partial class WorkSpace : UserControl
    {
        public event EventHandler ToolPlaced = delegate { };
        public event EventHandler UpdateCompounds = delegate { };
        public event EventHandler ValidationChecked = delegate { };

        private List<Tuple<string, Equation>> userDefinedVaraibles = new List<Tuple<string,Equation>>();

        private RuleManager ruleManager = RuleManager.GetInstance();

        private bool checkRules = true;

        private OptionDifficultySetting currentDifficultySetting;

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

        public bool CheckRules
        {
            get { return checkRules; }
            set { checkRules = value; }
        }

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
            CheckRulesForEquations(this, EventArgs.Empty);

        }

        public WorkSpace()
        {
            InitializeComponent();

            DrawingCanvas.ToolPlaced += new EventHandler(DrawingCanvas_ToolPlaced);
            DrawingCanvas.PfdUpdated += new PfdUpdatedEventHandler(CheckRulesForIPfdElements);
            GridSplitter.MouseMove += new MouseEventHandler(GridSplitter_MouseMove);
            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForEquations);
            SizeChanged += new SizeChangedEventHandler(WorkSpace_SizeChanged);
        }

        void WorkSpace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this sets the minium for the drawing canvas so it cant be smaller than the space it is supposed to fit in
            double height = WorkspaceGrid.RowDefinitions[0].ActualHeight;
            double width = WorkspaceGrid.ColumnDefinitions[0].ActualWidth;
            DrawingCanvasScollViewer.Width = width;
            DrawingCanvasScollViewer.Height = height;

            DrawingCanvas.MinHeight = height;
            DrawingCanvas.MinWidth = width;
        }

        public void DifficultySettingChanged(OptionDifficultySetting oldValue, OptionDifficultySetting newValue)
        {
            DrawingCanvas.CurrentDifficultySetting = newValue;
        }

        void DrawingCanvas_ToolPlaced(object sender, EventArgs e)
        {
            ToolPlaced(this, EventArgs.Empty);
        }

        /// <summary>
        /// Since Canvas object don't auto-resize, this method needs to be called
        /// whenever the main window gets resized so that we can resize our drawing
        /// drawing_canvas appropriately.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GridSplitter_MouseMove(object sender, MouseEventArgs e)
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

        public void UserDefinedVariablesUpdated(List<Tuple<string, Equation>> newVariables)
        {
            userDefinedVaraibles = newVariables;
            CheckRulesForEquations(null, EventArgs.Empty);
        }

        /// <summary>
        /// This fires when an equation is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckRulesForEquations(object sender, EventArgs e)
        {
                var uiElements = from c in DrawingCanvas.Children
                                 where c is IPfdElement
                                 select c;
                List<IPfdElement> pfdElements = new List<IPfdElement>();
                foreach (UIElement element in uiElements)
                {
                    //this works because all the elements in uiElements are IPfdElements they just need cast as such
                    pfdElements.Add(element as IPfdElement);
                }

                ruleManager.Validate(pfdElements, EquationEditor.EquationTokens, userDefinedVaraibles);
                FeedbackWindow.updateFeedbackWindow(ruleManager.ErrorMessages);
                ValidationChecked(this, EventArgs.Empty);
        }

        /// <summary>
        /// This fires whenever a stream is moved since we possible moved onto or off of a processUnit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckRulesForIPfdElements(object sender, PfdUpdatedEventArgs e)
        {
            UpdateCompounds(e.pfdElements, EventArgs.Empty);
            ruleManager.Validate(e.pfdElements, EquationEditor.EquationTokens, userDefinedVaraibles);
            FeedbackWindow.updateFeedbackWindow(ruleManager.ErrorMessages);
            ValidationChecked(this, EventArgs.Empty);
        }

        public void LoadXmlElements(XDocument doc)
        {
            //clear out previous data
            DrawingCanvas.ClearDrawingCanvas();
            EquationEditor.ClearEquations();

            //tell the drawing drawing_canvas to load its new children
            DrawingCanvas.LoadXmlElements(doc.Descendants("DrawingCanvas").ElementAt(0));

            //load the equations
            EquationEditor.LoadXmlElements(doc.Descendants("EquationEditor").ElementAt(0));

            //clear any existing messages in the feedback window and rerun the error checker
            CheckRulesForEquations(this, EventArgs.Empty);
        }

    }
}
