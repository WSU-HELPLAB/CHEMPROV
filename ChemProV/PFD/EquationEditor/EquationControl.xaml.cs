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

        private bool m_commentsVisible = false;
        
        private EquationModel m_model;

        private DeleteRequestDelegate m_requestDelete = null;

        public EquationControl(EquationEditor parent)
        {
            InitializeComponent();
            
            m_model = new EquationModel();
            SetScopeOptions(parent.EquationScopes);
            SetTypeOptions(parent.EquationTypes);
            this.DataContext = m_model;

            // Setup the annotation button
            RefreshAnnotationButton();
            // Monitor when the annotation changes so that we can set the button's icon to a grayed-out sticky 
            // note when the annotation is empty and a yellow sticky note when it's not
            m_model.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("Annotation"))
                {
                    // Refresh the button's icon
                    RefreshAnnotationButton();
                }
            };
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
                EquationTextBox.Text = value;
                m_model.Equation = value;
            }
        }

        private void EquationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the model
            m_model.Equation = EquationTextBox.Text;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether or not a control within this control 
        /// is the current focused element.
        /// </summary>
        public bool HasFocus
        {
            get
            {
                object focus = FocusManager.GetFocusedElement();
                return object.ReferenceEquals(focus, AnnotationButton) ||
                    object.ReferenceEquals(focus, TypeComboBox) ||
                    object.ReferenceEquals(focus, ScopeComboBox) ||
                    object.ReferenceEquals(focus, EquationTextBox);
            }
        }

        /// <summary>
        /// Gets the equation model. This control exists to provide a user interface relevant to creating 
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

        public void LoadFrom(XElement equationModelElement)
        {
            m_model = EquationModel.FromXml(equationModelElement);
            EquationTextBox.Text = m_model.Equation;
            this.DataContext = m_model;

            // Setup the annotation button
            RefreshAnnotationButton();
            // Monitor when the annotation changes so that we can set the button's icon to a grayed-out sticky 
            // note when the annotation is empty and a yellow sticky note when it's not
            m_model.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("Annotation"))
                {
                    // Refresh the button's icon
                    RefreshAnnotationButton();
                }
            };

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

            // Last but not least, the text box
            EquationTextBox.DataContext = m_model;
        }

        private void RefreshAnnotationButton()
        {
            bool hasComment = !string.IsNullOrEmpty(m_model.Annotation);
            Image img2 = Core.App.CreateImageFromSource(hasComment ?
                "palette_stickyNote_16x16.png" :
                "palette_stickyNote_16x16_gray.png");
            img2.Width = img2.Height = 16;
            AnnotationButton.Content = img2;

            // Set a tool-tip based on whether or not there's a comment
            if (hasComment)
            {
                ToolTipService.SetToolTip(AnnotationButton,
                    "Edit comment for this equation");
            }
            else
            {
                ToolTipService.SetToolTip(AnnotationButton,
                    "There are no comments on this equation; click to add a comment");
            }
        }

        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_model.Scope = ScopeComboBox.SelectedItem as EquationScope;
        }

        public void SetDeleteRequestDelegate(DeleteRequestDelegate deleteFunc)
        {
            m_requestDelete = deleteFunc;
        }

        /// <summary>
        /// Refreshes the options in the "Scope" combo box. Preserves the selected value if possible.
        /// </summary>
        public void SetScopeOptions(ObservableCollection<EquationScope> options)
        {
            // We want to avoid firing the SelectionChanged event during this method
            ScopeComboBox.SelectionChanged -= ScopeComboBox_SelectionChanged;
            
            // Before we clear, store the selected element
            object selected = ScopeComboBox.SelectedItem;

            // Clear and rebuild
            this.ScopeComboBox.Items.Clear();
            foreach (EquationScope es in options)
            {
                ScopeComboBox.Items.Add(es);

                // If the item we just added is equal to the previously selected one, then select it
                if (es.Equals(selected))
                {
                    ScopeComboBox.SelectedItem = es;
                }
            }

            // Go back to watching for selection changes
            ScopeComboBox.SelectionChanged += ScopeComboBox_SelectionChanged;
        }

        /// <summary>
        /// Refreshes the options in the "Type" combo box. Preserves the selected value if possible.
        /// </summary>
        public void SetTypeOptions(ObservableCollection<EquationType> options)
        {
            // We want to avoid firing the SelectionChanged event during this method
            TypeComboBox.SelectionChanged -= TypeComboBox_SelectionChanged;
            
            // Before we clear, store the selected element
            object selected = TypeComboBox.SelectedItem;

            // Clear and rebuild
            this.TypeComboBox.Items.Clear();
            foreach (EquationType et in options)
            {
                TypeComboBox.Items.Add(et);

                // If the item we just added is equal to the previously selected one, then select it
                if (et.Equals(selected))
                {
                    TypeComboBox.SelectedItem = et;
                }
            }

            // Go back to watching for selection changes
            TypeComboBox.SelectionChanged += TypeComboBox_SelectionChanged;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_model.Type = TypeComboBox.SelectedItem as EquationType;
        }

        private void AnnotationButton_Click(object sender, RoutedEventArgs e)
        {
            PFD.EquationEditor.Views.AnnotateWindow window =
                    new PFD.EquationEditor.Views.AnnotateWindow();
            window.DataContext = m_model;
            window.Show();
        }

        public bool CommentsVisible
        {
            get
            {
                return m_commentsVisible;
            }
            set
            {
                m_commentsVisible = value;
            }
        }
    }
}