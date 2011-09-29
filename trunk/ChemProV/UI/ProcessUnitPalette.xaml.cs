/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
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

namespace ChemProV.UI
{
    public partial class ProcessUnitPalette : UserControl
    {
        /// <summary>
        /// This is fired whenever the selection has been changed.
        /// </summary>
        public event EventHandler SelectionChanged;
        private IPaletteItem selectedItem = null;

        /// <summary>
        /// This is the currently selectedItem in the Palette
        /// </summary>
        public IPaletteItem SelectedItem
        {
            get 
            { 
                return selectedItem; 
            }
            set 
            { 
                selectedItem = value; 

                //fire a selection changed event
                SelectionChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// This is the constructor for Palette item, it initializes itself and then attaches necessary mouse listeners
        /// </summary>
        public ProcessUnitPalette()
        {
            InitializeComponent();

            //attach mouse listeners to the child objects (palette elements)
            AttachMouseListeners();

        }

        /// <summary>
        /// Attaches mouse listeners for each of the stack's children
        /// </summary>
        private void AttachMouseListeners()
        {
            foreach (UIElement uc in LayoutRoot.Children)
            {
                //only attach listeners for elements that extend IPaletteItem (gets us around selecting
                //UI chrome
                if (uc is IPaletteItem)
                {
                    uc.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                }
            }
        }

        /// <summary>
        /// Removes selection highlighting from all children
        /// </summary>
        private void ClearSelection()
        {
            foreach (UIElement uc in LayoutRoot.Children)
            {
                //only palette elements can be selected
                if (uc is IPaletteItem)
                {
                    uc.SetValue(GenericPaletteItem.SelectedProperty, false);
                }
            }
        }

        /// <summary>
        /// Resets the current selection back to the default choice
        /// </summary>
        public void ResetSelection()
        {
            HighlightItem(DefaultSelection);

        }

        private void HighlightItem(IPaletteItem item)
        {
            if (item.CompareTo(selectedItem) != 0)
            {
                //set the new selected item
                SelectedItem = item;

                //clear any current selection
                ClearSelection();

                //highlight current selection
                item.Selected = true;

            }
        }

        /// <summary>
        /// Handles palette item clicks
        /// </summary>
        /// <param name="sender">The object that was clicked</param>
        /// <param name="e">Some mouse event args?</param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HighlightItem((IPaletteItem)sender);
        }
    }
}
