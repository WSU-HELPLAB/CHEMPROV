/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Controls;

namespace ChemProV.UI
{
    public partial class OptionWindow : ChildWindow
    {
        private Core.Workspace m_workspace = null;
        
        public OptionDifficultySetting OptionSelection
        {
            get
            {
                if (Simplest.IsChecked == true)
                {
                    return OptionDifficultySetting.MaterialBalance;
                }
                else if (Medium.IsChecked == true)
                {
                    return OptionDifficultySetting.MaterialBalanceWithReactors;
                }
                else
                {
                    return OptionDifficultySetting.MaterialAndEnergyBalance;
                }
            }
        }

        public OptionWindow(Core.Workspace workspace)
        {
            InitializeComponent();

            m_workspace = workspace;

            // Try to select the appropriate item for the current font size
            foreach (object o in EEFontSizeCombo.Items)
            {
                ComboBoxItem cbi = o as ComboBoxItem;
                if (null != cbi)
                {
                    if (cbi.Content.ToString().Equals(m_workspace.EquationEditorFontSize.ToString()))
                    {
                        EEFontSizeCombo.SelectedItem = o;
                        break;
                    }
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem cbi = EEFontSizeCombo.SelectedItem as ComboBoxItem;
            if (null != cbi)
            {
                double d;
                if (double.TryParse(cbi.Content.ToString(), out d))
                {
                    m_workspace.EquationEditorFontSize = d;
                }
            }
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}