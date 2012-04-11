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
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.PFD.EquationEditor.Views
{
    public partial class ScopeControl : UserControl
    {
        public ScopeControl()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(ScopeControl_DataContextChanged);
        }

        void ScopeControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //remove listener from old model
            if (e.OldValue is EquationModel)
            {
                EquationModel context = e.NewValue as EquationModel;
                context.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(DataContext_PropertyChanged);
            }

            //attach it to new model
            if (e.NewValue is EquationModel)
            {
                EquationModel context = e.NewValue as EquationModel;
                context.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DataContext_PropertyChanged);
            }
        }

        void DataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if the Scope changed, sync it up in our list
            if (e.PropertyName.CompareTo("Scope") == 0)
            {
                EquationModel model = DataContext as EquationModel;
                foreach (EquationScope item in ScopeComboBox.Items)
                {
                    if (item.Equals(model.Scope))
                    {
                        if (ScopeComboBox.SelectedItem != item)
                        {
                            ScopeComboBox.SelectedItem = item;
                        }
                    }
                }
            }
        }
    }
}
