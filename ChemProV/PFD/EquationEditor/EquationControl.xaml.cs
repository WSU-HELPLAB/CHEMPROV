/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using ChemProV.PFD.EquationEditor.Tokens;
using ChemProV.Validation;
using Antlr.Runtime;
using System.IO;
using ChemProV.Grammars;
using Antlr.Runtime.Tree;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationControl : UserControl//, IXmlSerializable, INotifyPropertyChanged
    {
        public delegate void DeleteRequestDelegate(EquationControl sender);

        /// <summary>
        /// Temporarily set to true in methods that change the model
        /// </summary>
        private bool m_changingModel = false;
        
        /// <summary>
        /// Reference to the model that this control represents. This control modifies this 
        /// object but does not "own it". In other words, it is instantiated elsewhere and 
        /// passed in to the constructor of this control.
        /// </summary>
        private EquationModel m_model;

        private DeleteRequestDelegate m_requestDelete = null;

        public EquationControl(EquationEditor parent, EquationModel equationModel)
        {
            InitializeComponent();

            m_model = equationModel;
            SetScopeOptions(parent.EquationScopes);
            SetTypeOptions(parent.EquationTypes);
            
            // Set UI elements from the model

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

            EquationTextBox.Text = equationModel.Equation;

            // We want to make sure we have a right-click menu for the equation text box
            Core.App.InitRightClickMenu(EquationTextBox);
        }

        public bool CommentsVisible
        {
            get
            {
                return m_model.CommentsVisible;
            }
            set
            {
                // Avoid recursion
                if (m_changingModel)
                {
                    return;
                }
                
                if (value == m_model.CommentsVisible)
                {
                    // No change
                    return;
                }

                m_changingModel = true;
                m_model.CommentsVisible = value;
                m_changingModel = false;
            }
        }

        private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != m_requestDelete)
            {
                m_requestDelete(this);
            }
        }
        
        /// <summary>
        /// Gets or sets the equation text, which is just the text in the equation text box. However, it 
        /// also sets the equation in the model and does relevant updates.
        /// Use this property instead of directly accessing EquationTextBox.
        /// </summary>
        public string EquationText
        {
            get
            {
                return EquationTextBox.Text;
            }
            set
            {
                // Avoid recursion
                if (m_changingModel)
                {
                    return;
                }

                m_changingModel = true;
                EquationTextBox.Text = value;
                m_model.Equation = value;
                m_changingModel = false;
            }
        }

        private void EquationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Avoid recursion
            if (m_changingModel)
            {
                return;
            }
            
            // Update the model
            m_changingModel = true;
            m_model.Equation = EquationTextBox.Text;
            m_changingModel = false;
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

        #region Compatibility stuff until further refactoring

        /// <summary>
        /// Compatibility item
        /// Other code references this, but I'm fairly sure all that code is obsolete. But rather than trying 
        /// to do a major clean-out of all unused stuff right now, I'm just going to keep compatibility.
        /// </summary>
        public EquationType SelectedItem
        {
            get { return TypeComboBox.SelectedItem as EquationType; }
        }

        /// <summary>
        /// Compatibility item
        /// (read comment on SelectedItem above... same deal here)
        /// </summary>
        public ObservableCollection<IEquationToken> EquationTokens
        {
            get;
            set;
        }

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
            
            m_changingModel = true;            
            m_model.Scope = ScopeComboBox.SelectedItem as EquationScope;
            m_changingModel = false;
        }

        public void SetDeleteRequestDelegate(DeleteRequestDelegate deleteFunc)
        {
            m_requestDelete = deleteFunc;
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
                if (es.Equals(m_model.Scope))
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
                if (et.Equals(m_model.Type))
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

            m_changingModel = true;
            m_model.Type = TypeComboBox.SelectedItem as EquationType;
            m_changingModel = false;
        }
    }
}