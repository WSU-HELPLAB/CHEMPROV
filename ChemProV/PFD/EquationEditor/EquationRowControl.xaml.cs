/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ChemProV.Logic;
using ChemProV.Logic.Equations;

namespace ChemProV.PFD.EquationEditor
{
    /// <summary>
    /// Control to represent a single equation row in the equation editor. This control serves as UI for 
    /// an EquationModel object.
    /// </summary>
    public partial class EquationRowControl : UserControl
    {
        public delegate void DeleteRequestDelegate(EquationRowControl sender);

        /// <summary>
        /// Temporarily set to true in methods that change the model
        /// </summary>
        private bool m_changingModel = false;
        
        /// <summary>
        /// Reference to the model that this control represents. This control modifies this 
        /// object but does not "own it". In other words, it is instantiated elsewhere and 
        /// passed in to the constructor of this control.
        /// This reference can be changed at runtime, allowing the control to change from 
        /// one model to another. See the "SetModel" function.
        /// </summary>
        private EquationModel m_model;

        private DeleteRequestDelegate m_requestDelete = null;

        private Workspace m_workspace;

        public EquationRowControl(Workspace workspace, EquationEditor parent, EquationModel equationModel)
        {
            InitializeComponent();

            m_workspace = workspace;
            SetScopeOptions(parent.EquationScopes);
            SetTypeOptions(parent.EquationTypes);

            // Use the SetModel function to set the current model. This will update the UI and subscribe 
            // to relevant events.
            m_model = null;
            SetModel(equationModel);

            // We want to make sure we have a right-click menu for the equation text box
            Core.App.InitRightClickMenu(EquationTextBox);
        }

        private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != m_requestDelete)
            {
                m_requestDelete(this);
            }
        }

        private void EquationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Avoid recursion
            if (m_changingModel)
            {
                return;
            }

            // If the equation in the model is not equal to the equation in the text box, then this 
            // implies that the user is changing the text in the equation text box (i.e. it's not 
            // being changed programatically). In this case we need to update the model and add 
            // an undo.
            if (m_model.Equation != EquationTextBox.Text)
            {
                string oldText = m_model.Equation;
                
                // Update the model
                m_changingModel = true;
                m_model.Equation = EquationTextBox.Text;
                m_changingModel = false;

                // For now, create an undo for every text change
                m_workspace.AddUndo(new UndoRedoCollection("Undo changing equation text",
                    new Logic.Undos.SetEquationText(m_model, oldText)));
            }
        }

        /// <summary>
        /// Gets the equation model. This control exists to provide a user interface relevant to editing 
        /// this equation model.
        /// </summary>
        public EquationModel Model
        {
            get
            {
                return m_model;
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Equation"))
            {
                EquationTextBox.Text = m_model.Equation;
            }
            else if (e.PropertyName.Equals("Type"))
            {
                foreach (object item in TypeComboBox.Items)
                {
                    if (item.Equals(m_model.Type))
                    {
                        TypeComboBox.SelectedItem = item;
                        return;
                    }
                }
            }
            else if (e.PropertyName.Equals("Scope"))
            {
                foreach (object item in ScopeComboBox.Items)
                {
                    if (item.Equals(m_model.Scope))
                    {
                        ScopeComboBox.SelectedItem = item;
                        return;
                    }
                }
            }
        }

        #region Compatibility stuff until further refactoring

        public void HighlightFeedback(bool highlight)
        {
            throw new NotImplementedException();
        }

        public void RemoveFeedback()
        {
            throw new NotImplementedException();
        }

        public void SetFeedback(string message, int errorNumber)
        {
            throw new NotImplementedException();
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
        
        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Avoid recursion
            if (m_changingModel)
            {
                return;
            }

            if (null == ScopeComboBox.SelectedItem ||
                !m_model.Scope.Equals(ScopeComboBox.SelectedItem))
            {
                object oldVal = m_model.Scope;
                
                m_changingModel = true;
                m_model.Scope = ScopeComboBox.SelectedItem as EquationScope;
                m_changingModel = false;

                m_workspace.AddUndo(new UndoRedoCollection("Undo changing equation scope",
                    new Logic.Undos.SetProperty(m_model, "Scope", oldVal)));
            }
        }

        public void SetDeleteRequestDelegate(DeleteRequestDelegate deleteFunc)
        {
            m_requestDelete = deleteFunc;
        }

        /// <summary>
        /// Sets the model for this equation control. The UI elements will be updated will data from the 
        /// model and then the model will be updated as the user changes values by using the control. The 
        /// model reference can be null if desired, but that should only be used in cases where this 
        /// control is about to be disposed and is no longer needed.
        /// </summary>
        public void SetModel(EquationModel model)
        {
            // IMPORTANT: Unsubscribe from the old model
            if (null != m_model)
            {
                m_model.PropertyChanged -= this.Model_PropertyChanged;
            }
            
            m_model = model;

            // Calling this method with a null reference for the model is considered valid, so 
            // only update the UI if the model is non-null.
            if (null != model)
            {
                // Setup the type combo box
                foreach (object typeObj in TypeComboBox.Items)
                {
                    if (m_model.Type.Equals(typeObj))
                    {
                        TypeComboBox.SelectedItem = typeObj;
                    }
                }

                // Setup the scope combo box
                foreach (EquationScope item in ScopeComboBox.Items)
                {
                    if (item.Equals(m_model.Scope))
                    {
                        if (ScopeComboBox.SelectedItem != item)
                        {
                            ScopeComboBox.SelectedItem = item;
                        }
                    }
                }

                EquationTextBox.Text = model.Equation;

                // Watch for property changes
                m_model.PropertyChanged += this.Model_PropertyChanged;
            }
        }

        /// <summary>
        /// Refreshes the options in the "Scope" combo box. Sets the selected item to match the 
        /// equation model, if possible.
        /// </summary>
        public void SetScopeOptions(ObservableCollection<EquationScope> options)
        {
            // We want to avoid firing the SelectionChanged event during this method
            ScopeComboBox.SelectionChanged -= ScopeComboBox_SelectionChanged;

            // Clear and rebuild
            this.ScopeComboBox.Items.Clear();
            foreach (EquationScope es in options)
            {
                ScopeComboBox.Items.Add(es);

                // If the item we just added is equal to the value in the model, then select it
                if (null != m_model && es.Equals(m_model.Scope))
                {
                    ScopeComboBox.SelectedItem = es;
                }
            }

            // Go back to watching for selection changes
            ScopeComboBox.SelectionChanged += ScopeComboBox_SelectionChanged;
        }

        /// <summary>
        /// Refreshes the options in the "Type" combo box. Sets the selected item to match the 
        /// equation model, if possible.
        /// </summary>
        public void SetTypeOptions(ObservableCollection<EquationType> options)
        {
            // We want to avoid firing the SelectionChanged event during this method
            TypeComboBox.SelectionChanged -= TypeComboBox_SelectionChanged;

            // Clear and rebuild
            this.TypeComboBox.Items.Clear();
            foreach (EquationType et in options)
            {
                TypeComboBox.Items.Add(et);

                // If the item we just added is equal to the value in the model, then select it
                if (null != m_model && et.Equals(m_model.Type))
                {
                    TypeComboBox.SelectedItem = et;
                }
            }

            // Go back to watching for selection changes
            TypeComboBox.SelectionChanged += TypeComboBox_SelectionChanged;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Avoid recursion
            if (m_changingModel)
            {
                return;
            }

            if (null == TypeComboBox.SelectedItem || 
                !m_model.Type.Equals(TypeComboBox.SelectedItem as EquationType))
            {
                object oldVal = m_model.Type;
                
                m_changingModel = true;
                m_model.Type = TypeComboBox.SelectedItem as EquationType;
                m_changingModel = false;

                m_workspace.AddUndo(new UndoRedoCollection("Undo changing equation type",
                    new Logic.Undos.SetProperty(m_model, "Type", oldVal)));
            }
        }
    }
}