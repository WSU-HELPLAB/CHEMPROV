/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds
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
using System.Reflection;
using ChemProV.UI;
using ChemProV.PFD.Streams;

namespace ChemProV.UI
{
    public partial class ControlPalette : UserControl
    {
        private int m_buttonsPerRow = 3;
        
        public ControlPalette()
        {
            InitializeComponent();
        }

        private Border CreateButton(string imageSource, string toolTip, Type tag)
        {
            // Create the icon for the button
            Image image = new Image();
            Uri uri = new Uri(imageSource, UriKind.Relative);
            ImageSource img = new System.Windows.Media.Imaging.BitmapImage(uri);
            image.SetValue(Image.SourceProperty, img);
            image.Stretch = Stretch.None;

            // Give the image a tool tip
            ToolTipService.SetToolTip(image, toolTip);

            // Create the "button". We're using Border objects as buttons because they have 
            // all the desired functionality and are easier to adjust in terms of visual styles
            Border btn = new Border();
            btn.IsHitTestVisible = true;
            btn.CornerRadius = new CornerRadius(3.0);
            btn.BorderThickness = new Thickness(1.0);
            btn.Background = new SolidColorBrush(Colors.White);
            btn.BorderBrush = new SolidColorBrush(Colors.Gray);
            btn.Padding = new Thickness(2.0);
            btn.Child = image;
            btn.Margin = new Thickness(2.0);

            // Give it a tag to represent the type it creates
            btn.Tag = tag;

            // Set up its click event
            btn.MouseLeftButtonDown += new MouseButtonEventHandler(PaletteButton_MouseLeftButtonDown);

            return btn;
        }

        private void HighlightButton(Border button)
        {
            SolidColorBrush fill = new SolidColorBrush(Colors.White);

            // Start by setting the background on all "buttons" to white. Reminder: we're 
            // using Border objects for our buttons.
            SelectButton.Background = StickyNoteButton.Background = fill;
            SelectButton.BorderThickness = StickyNoteButton.BorderThickness = new Thickness(1.0);
            SetBorderButtonsStyle(ProcessUnitsPanel, fill, 1.0, true);
            SetBorderButtonsStyle(StreamsPanel, fill, 1.0, true);

            // Now set the background on the one that was specified
            button.Background = new SolidColorBrush(Colors.Yellow);
            button.BorderThickness = new Thickness(2.0);
        }

        /// <summary>
        /// Highlights the select button (and unhighlights all others) without setting any canvas states. 
        /// The SwitchToSelect method will set the drawing canvas's current state to null but this method 
        /// will not.
        /// </summary>
        public void HighlightSelect()
        {
            HighlightButton(SelectButton);
        }

        private void PaletteButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas canvas = Core.App.Workspace.DrawingCanvasReference;

            // Make sure that if we have a popup menu that we close it at this point
            Core.App.ClosePopup();

            // Now we want to set the canvas state appropriately. If we've clicked a button for a process 
            // unit, then we want to make sure that the canvas is in process-unit-placing mode. If we've 
            // clicked a button for a stream, then we want to make sure that the canvas is in stream-placing 
            // mode. Similar thing with sticky notes. The select button is a special case wherein we want 
            // to cancel/finish and current state for the canvas and switch over to a null state.

            // First, set the canvas's selected element to null
            Core.App.Workspace.DrawingCanvas.SelectedElement = null;

            // Highlight the button that was clicked
            HighlightButton(sender as Border);

            // Now we need to see what button was clicked
            // Start with the select button
            if (object.ReferenceEquals(sender, SelectButton))
            {
                Core.App.Workspace.DrawingCanvas.CurrentState = null;
                return;
            }
            else if (object.ReferenceEquals(sender, StickyNoteButton))
            {
                // We have a custom state for placing sticky notes
                canvas.CurrentState = new UI.DrawingCanvasStates.PlacingNewCommentNote(
                    canvas, this);
                return;
            }
            else if (object.ReferenceEquals(sender, ChemicalStreamButton))
            {
                // Set a state to create a chemical stream
                canvas.CurrentState = new UI.DrawingCanvasStates.PlacingNewStream(
                    this, canvas, StreamType.Chemical);
            }
            else if (object.ReferenceEquals(sender, HeatStreamButton))
            {
                // Set a state to create a heat stream
                canvas.CurrentState = new UI.DrawingCanvasStates.PlacingNewStream(
                    this, canvas, StreamType.Heat);
            }
            else
            {
                // Get the type of object we are about to create
                Type newObjType = ((Border)sender).Tag as Type;

                // If it's a process unit, assign the state to create it
                if (newObjType.IsSubclassOf(typeof(Core.AbstractProcessUnit)))
                {
                    canvas.CurrentState = new UI.DrawingCanvasStates.PlacingNewProcessUnit(
                        this, canvas, newObjType);
                }
            }
        }

        /// <summary>
        /// Refreshes the control palette with appropriate controls based on the difficulty setting. This 
        /// must be called each time the user changes the difficulty setting in the application.
        /// </summary>
        public void RefreshPalette(OptionDifficultySetting setting)
        {
            // Show or hide the heat stream button based on the setting
            if ((new Core.HeatStream(-1)).IsAvailableWithDifficulty(setting))
            {
                HeatStreamButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                HeatStreamButton.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Now we use reflection to create the process unit buttons
            
            // First clear the content in the process unit stack panel
            ProcessUnitsPanel.Children.Clear();

            // We will create potentially multiple stack panels for rows of buttons
            StackPanel spPUs = null;

            // Keep track of how many buttons we create
            int puBtns = 0;

            // Use reflection to find appropriate process units and streams for the palette
            Assembly a = Assembly.GetExecutingAssembly();
            foreach (Type t in a.GetTypes())
            {
                // Ignore abstract types
                if (t.IsAbstract)
                {
                    continue;
                }

                // We only are interested in types that inherit from AbstractProcessUnit
                if (t.IsSubclassOf(typeof(Core.AbstractProcessUnit)) && !t.IsAbstract)
                {
                    // We've found a potential process unit, but we need to make sure that 
                    // it can be created under the specified difficulty setting
                    Core.AbstractProcessUnit unit = 
                        Activator.CreateInstance(t, (int)-1) as Core.AbstractProcessUnit;
                    if (unit.IsAvailableWithDifficulty(setting))
                    {
                        if (0 == (puBtns % m_buttonsPerRow))
                        {
                            // Create a new row
                            spPUs = new StackPanel();
                            spPUs.Orientation = Orientation.Horizontal;

                            // Add the first button to it
                            spPUs.Children.Add(CreateButton(
                                ProcessUnitControl.GetIconSource(t), unit.Description, t));

                            ProcessUnitsPanel.Children.Add(spPUs);
                        }
                        else
                        {
                            spPUs.Children.Add(CreateButton(
                                ProcessUnitControl.GetIconSource(t), unit.Description, t));
                        }

                        puBtns++;
                    }
                }
            }
        }

        private void SetBorderButtonsStyle(StackPanel parent, Brush fill, double borderThickness, bool recurse)
        {
            foreach (UIElement uie in parent.Children)
            {
                if (uie is Border)
                {
                    (uie as Border).Background = fill;
                    (uie as Border).BorderThickness = new Thickness(borderThickness);
                }
                else if (uie is StackPanel && recurse)
                {
                    SetBorderButtonsStyle(uie as StackPanel, fill, borderThickness, recurse);
                }
            }
        }

        public void SwitchToSelect()
        {
            PaletteButton_MouseLeftButtonDown(SelectButton, null);
        }

        /// <summary>
        /// Upon the intial load of this control, we just want to set the text labels to 
        /// "Loading...". Soon after the application intializes, it should call RefreshPalette 
        /// to populate the appropriate controls. The reason why we don't just set these in the 
        /// XAML is so that when developers see the design view they know what's going on.
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReplaceMeWithProcessUnits.Text = "Loading...";

            StickyNoteButton.Tag = typeof(StickyNoteControl);
        }
    }
}
