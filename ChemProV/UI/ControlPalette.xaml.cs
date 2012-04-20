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
using ChemProV.PFD.ProcessUnits;

namespace ChemProV.UI
{
    public partial class ControlPalette : UserControl
    {
        public ControlPalette()
        {
            InitializeComponent();
        }

        private Border CreateButton(string imageSource, string text, Type tag)
        {
            // Create the icon for the button
            Image image = new Image();
            Uri uri = new Uri(imageSource, UriKind.Relative);
            ImageSource img = new System.Windows.Media.Imaging.BitmapImage(uri);
            image.SetValue(Image.SourceProperty, img);

            // Create the text block for the button's text
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Margin = new Thickness(6.0, 0.0, 0.0, 0.0);
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            // Create the stack panel to hold content within the button
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(image);
            sp.Children.Add(tb);

            // Create the "button". We're using Border objects as buttons because they have 
            // all the desired functionality and are easier to adjust in terms of visual styles
            Border btn = new Border();
            btn.CornerRadius = new CornerRadius(3.0);
            btn.BorderThickness = new Thickness(1.0);
            btn.Background = new SolidColorBrush(Colors.White);
            btn.BorderBrush = new SolidColorBrush(Colors.Gray);
            btn.Padding = new Thickness(2.0);
            //btn.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            btn.Child = sp;
            btn.Margin = new Thickness(2.0);

            // Give it a tag to represent the type it creates
            btn.Tag = tag;

            // Set up its click event
            btn.MouseLeftButtonDown += new MouseButtonEventHandler(PaletteButton_MouseLeftButtonDown);

            return btn;
        }

        private void PaletteButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SolidColorBrush fill = new SolidColorBrush(Colors.White);

            // Start by setting the background on all "buttons" to white. Reminder: we're 
            // using Border objects for our buttons.
            SelectButton.Background = StickyNoteButton.Background = fill;
            SelectButton.BorderThickness = StickyNoteButton.BorderThickness = new Thickness(1.0);
            foreach (UIElement ui in ProcessUnitsPanel.Children)
            {
                Border btn = ui as Border;
                if (null != ui)
                {
                    btn.Background = fill;
                    btn.BorderThickness = new Thickness(1.0);
                }
            }
            foreach (UIElement ui in StreamsPanel.Children)
            {
                Border btn = ui as Border;
                if (null != ui)
                {
                    btn.Background = fill;
                    btn.BorderThickness = new Thickness(1.0);
                }
            }

            // Now set the background on the one that was clicked to indicate that it's active
            ((Border)sender).Background = new SolidColorBrush(Colors.Yellow);
            ((Border)sender).BorderThickness = new Thickness(2.0);

            // Now we want to set the canvas state appropriately. If we've clicked a button for a process 
            // unit, then we want to make sure that the canvas is in process-unit-placing mode. If we've 
            // clicked a button for a stream, then we want to make sure that the canvas is in stream-placing 
            // mode. Similar thing with sticky notes. The select button is a special case wherein we want 
            // to cancel/finish and current state for the canvas and switch over to a null state.

            // Start with the select button
            if (object.ReferenceEquals(sender, SelectButton))
            {
                Core.App.Workspace.DrawingCanvas.CurrentState = Core.App.Workspace.DrawingCanvas.NullState;
                return;
            }

            // Set the canvas state to setting a new object
            Core.App.Workspace.DrawingCanvas.CurrentState = new UI.DrawingCanvas.States.PlacingNewObject(
                this, Core.App.Workspace.DrawingCanvas, ((Border)sender).Tag);
        }

        /// <summary>
        /// Refreshes the control palette with appropriate controls based on the difficulty setting. This 
        /// must be called each time the user changes the difficulty setting in the application.
        /// </summary>
        public void RefreshPalette(OptionDifficultySetting setting)
        {
            // First clear the content in the stack panels
            ProcessUnitsPanel.Children.Clear();
            StreamsPanel.Children.Clear();

            // Use reflection to find appropriate process units and streams for the palette
            Assembly a = Assembly.GetExecutingAssembly();
            foreach (Type t in a.GetTypes())
            {
                // Ignore abstract types
                if (t.IsAbstract)
                {
                    continue;
                }

                // We only are interested in types that inherit from GenericProcessUnit and 
                // AbstractStream
                if (t.IsSubclassOf(typeof(PFD.ProcessUnits.LabeledProcessUnit)) &&
                    typeof(PFD.ProcessUnits.LabeledProcessUnit) != t)
                {
                    // We've found a potential process unit, but we need to make sure that 
                    // it can be created under the specified difficulty setting
                    PFD.ProcessUnits.LabeledProcessUnit unit = (PFD.ProcessUnits.LabeledProcessUnit)
                        Activator.CreateInstance(t);
                    if (unit.IsAvailableWithDifficulty(setting))
                    {
                        ProcessUnitsPanel.Children.Add(CreateButton(unit.IconSource,
                            unit.Description, t));
                    }
                }
                else if (t.IsSubclassOf(typeof(PFD.Streams.AbstractStream)))
                {
                    // We've found a potential stream, but we need to make sure that 
                    // it can be created under the specified difficulty setting. Create 
                    // a dummy object to find out.
                    PFD.Streams.IStream stream = (PFD.Streams.IStream)Activator.CreateInstance(t);
                    if (stream.IsAvailableWithDifficulty(setting))
                    {
                        StreamsPanel.Children.Add(CreateButton((stream is PFD.Streams.HeatStream) ?
                            "/UI/Icons/pu_heat_stream.png" : "/UI/Icons/pu_stream.png", stream.Title, t));
                    }
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
            ReplaceMeWithStreams.Text = "Loading...";

            StickyNoteButton.Tag = typeof(PFD.StickyNote.StickyNote);
        }
    }
}
