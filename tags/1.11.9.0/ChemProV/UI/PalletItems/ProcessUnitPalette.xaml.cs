/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ChemProV.UI.PalletItems
{
    /// <summary>
    /// This is the base class for the ProcessUnitPalette
    /// Currently there are three different levels and each one inherients from the previous level
    /// </summary>
    public partial class ProcessUnitPalette : UserControl
    {
        /// <summary>
        /// This is fired whenever the selection has been changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        //we default to the simplest case
        private OptionDifficultySetting currentDifficultySetting = OptionDifficultySetting.MaterialBalance;

        public OptionDifficultySetting CurrentDifficultySetting
        {
            get { return currentDifficultySetting; }
            set
            {
                currentDifficultySetting = value;
            }
        }

        private IPaletteItem selectedItem = null;
        struct HoveringOver
        {
            public bool OverCategorySelector;
            public bool OverPopUp;
        }
        private HoveringOver SelectingProcessUnit;
        private HoveringOver SelectingStreams;
        DispatcherTimer timer = new DispatcherTimer();

        public void StartPopUpTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(PopUpTimerClick);
            timer.Start();
        }

        private void PopUpTimerClick(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Tick -= new EventHandler(PopUpTimerClick);
            if (SelectingProcessUnit.OverCategorySelector == false && SelectingProcessUnit.OverPopUp == false && ProcessUnitFlyOut.IsOpen == true)
            {
                FlyOut_Close(ProcessUnitFlyOut);
            }
            if (SelectingStreams.OverCategorySelector == false && SelectingStreams.OverPopUp == false && StreamsFlyOut.IsOpen == true)
            {
                FlyOut_Close(StreamsFlyOut);
            }
        }

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

            ProcessUnitCategory.MouseEnter += new MouseEventHandler(CategorySelector_MouseEnter);
            ProcessUnitCategory.MouseLeave += new MouseEventHandler(CategorySelector_MouseLeave);
            StreamsCategory.MouseEnter += new MouseEventHandler(CategorySelector_MouseEnter);
            StreamsCategory.MouseLeave += new MouseEventHandler(CategorySelector_MouseLeave);

            ProcessUnitFlyOutBorder.MouseEnter += new MouseEventHandler(FlyOut_MouseEnter);
            ProcessUnitFlyOutBorder.MouseLeave += new MouseEventHandler(FlyOut_MouseLeave);
            ProcessUnitFlyOut.Opened += new EventHandler(FlyOut_Opened);

            StreamsUnitFlyOutBorder.MouseEnter += new MouseEventHandler(FlyOut_MouseEnter);
            StreamsUnitFlyOutBorder.MouseLeave += new MouseEventHandler(FlyOut_MouseLeave);
            StreamsFlyOut.Opened += new EventHandler(FlyOut_Opened);

            //attach mouse listeners to the child objects (palette elements)
            AttachMouseListenersToAllPaletteItems();
        }

        /// <summary>
        /// Attaches mouse listeners for each of the stack's children
        /// </summary>
        protected void AttachMouseListenersToAllPaletteItems()
        {
            foreach (UIElement ui in ((ProcessUnitFlyOut.Child as Border).Child as StackPanel).Children)
            {
                ui.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                ui.MouseEnter += new MouseEventHandler(OnMouseEnter);
                ui.MouseLeave += new MouseEventHandler(OnMouseLeave);
            }
            foreach (UIElement ui in ((StreamsFlyOut.Child as Border).Child as StackPanel).Children)
            {
                ui.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                ui.MouseEnter += new MouseEventHandler(OnMouseEnter);
                ui.MouseLeave += new MouseEventHandler(OnMouseLeave);
            }
            foreach (UIElement ui in LayoutRoot.Children)
            {
                if (ui is GenericPaletteItem)
                {
                    ui.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                }
            }
        }

        /// <summary>
        /// Attaches mouse listeners for each of the stack's children
        /// </summary>
        protected void DetachMouseListenersToAllPaletteItems()
        {
            foreach (UIElement ui in ((ProcessUnitFlyOut.Child as Border).Child as StackPanel).Children)
            {
                ui.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
                ui.MouseEnter -= new MouseEventHandler(OnMouseEnter);
                ui.MouseLeave -= new MouseEventHandler(OnMouseLeave);
            }
            foreach (UIElement ui in ((StreamsFlyOut.Child as Border).Child as StackPanel).Children)
            {
                ui.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
                ui.MouseEnter -= new MouseEventHandler(OnMouseEnter);
                ui.MouseLeave -= new MouseEventHandler(OnMouseLeave);
            }
            foreach (UIElement ui in LayoutRoot.Children)
            {
                if (ui is GenericPaletteItem)
                {
                    ui.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
                }
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            (sender as GenericPaletteItem).LayoutRoot.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            (sender as GenericPaletteItem).LayoutRoot.Background = new SolidColorBrush(Colors.White);
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
                if (selectedItem != null)
                {
                    selectedItem.Selected = false;
                }
                else
                {
                    DefaultSelection.Selected = false;
                }

                //set the new selected item
                SelectedItem = item;

                //highlight current selection
                item.Selected = true;

                if (item != DefaultSelection && item.Description != "Sticky Note")
                {
                    GenericPaletteItem gItem = (item as GenericPaletteItem);

                    gItem.blink_Storyboard.Completed += new EventHandler(blink_Storyboard_Completed);
                    gItem.blink_Storyboard.Begin();
                }
            }
        }

        private void blink_Storyboard_Completed(object sender, EventArgs e)
        {
            if (StreamsFlyOut.IsOpen == true)
            {
                FlyOut_Close(StreamsFlyOut);
            }
            else if (ProcessUnitFlyOut.IsOpen == true)
            {
                FlyOut_Close(ProcessUnitFlyOut);
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
            e.Handled = true;
        }

        private void FlyOut_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender == this.ProcessUnitFlyOut.Child)
            {
                SelectingProcessUnit.OverPopUp = true;
            }
            else if (sender == this.StreamsFlyOut.Child)
            {
                SelectingStreams.OverPopUp = true;
            }
        }

        private void FlyOut_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectingProcessUnit.OverPopUp = false;
            SelectingStreams.OverPopUp = false;
            /*   if (StreamsFlyOut.IsOpen == true)
               {
                   FlyOut_Close(StreamsFlyOut);
               }
               else if (ProcessUnitFlyOut.IsOpen == true)
               {
                   FlyOut_Close(ProcessUnitFlyOut);
               }*/
        }

        private void CategorySelector_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender == this.ProcessUnitCategory)
            {
                ProcessUnitFlyOut.IsOpen = true;
                StreamsFlyOut.IsOpen = false;
                SelectingProcessUnit.OverCategorySelector = true;
            }
            else if (sender == this.StreamsCategory)
            {
                StreamsFlyOut.IsOpen = true;
                ProcessUnitFlyOut.IsOpen = false;
                SelectingStreams.OverCategorySelector = true;
            }
        }

        private void CategorySelector_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender == this.ProcessUnitCategory)
            {
                SelectingProcessUnit.OverCategorySelector = false;
                StartPopUpTimer();
            }
            else if (sender == this.StreamsCategory)
            {
                SelectingStreams.OverCategorySelector = false;
                StartPopUpTimer();
            }
        }

        private void FlyOut_Opened(object sender, EventArgs e)
        {
            Storyboard OpenPopUp_Storyboard = new Storyboard();
            DoubleAnimation da = new DoubleAnimation();
            Duration duration = new Duration(TimeSpan.FromSeconds(.25));
            da.Duration = duration;
            OpenPopUp_Storyboard.Duration = duration;
            OpenPopUp_Storyboard.Children.Add(da);

            Storyboard.SetTarget(da, (sender as Popup).Child);

            Storyboard.SetTargetProperty(da, new PropertyPath(Border.OpacityProperty));

            da.From = 0;
            da.To = 1;

            OpenPopUp_Storyboard.Begin();
        }

        private bool closingFlyOutProcessUnit = false;
        private bool closingFlyOutStreams = false;

        private void FlyOut_Close(Popup sender)
        {
            Storyboard ClosePopUp_Storyboard = new Storyboard();
            DoubleAnimation da = new DoubleAnimation();
            Duration duration = new Duration(TimeSpan.FromSeconds(.25));
            da.Duration = duration;
            ClosePopUp_Storyboard.Duration = duration;
            ClosePopUp_Storyboard.Children.Add(da);

            Storyboard.SetTarget(da, sender.Child);

            Storyboard.SetTargetProperty(da, new PropertyPath(Border.OpacityProperty));

            da.From = 1;
            da.To = 0;
            ClosePopUp_Storyboard.Completed += new EventHandler(ClosePopUp_Storyboard_Completed);
            if (sender == ProcessUnitFlyOut)
            {
                if (closingFlyOutProcessUnit == false)
                {
                    closingFlyOutProcessUnit = true;
                    closing = sender;
                    ClosePopUp_Storyboard.Begin();
                }
            }
            else if (sender == StreamsFlyOut)
            {
                if (closingFlyOutStreams == false)
                {
                    closingFlyOutStreams = true;
                    Closing = sender;
                    ClosePopUp_Storyboard.Begin();
                }
            }
        }

        private Popup closing;

        public Popup Closing
        {
            get { return closing; }
            set
            {
                if (closing != null)
                    closing.IsOpen = false;
                closing = value;
            }
        }

        private void ClosePopUp_Storyboard_Completed(object sender, EventArgs e)
        {
            if (closing != null)
            {
                Closing.IsOpen = false;

                if (closing == ProcessUnitFlyOut)
                {
                    closingFlyOutProcessUnit = false;
                }
                else if (closing == StreamsFlyOut)
                {
                    closingFlyOutStreams = false;
                }
                closing = null;
            }
        }
    }
}