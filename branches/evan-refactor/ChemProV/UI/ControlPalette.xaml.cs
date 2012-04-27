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
using ChemProV.PFD.Streams;

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

        /// <summary>
        /// Highlights the select button (and unhighlights all others) without setting any canvas states. 
        /// The SwitchToSelect method will set the drawing canvas's current state to null but this method 
        /// will not.
        /// </summary>
        public void HighlightSelect()
        {
            DrawingCanvas.DrawingCanvas canvas = Core.App.Workspace.DrawingCanvasReference;
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
            SelectButton.Background = new SolidColorBrush(Colors.Yellow);
            SelectButton.BorderThickness = new Thickness(2.0);
        }

        private void PaletteButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas.DrawingCanvas canvas = Core.App.Workspace.DrawingCanvasReference;
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

            // First, set the canvas's selected element to null
            Core.App.Workspace.DrawingCanvas.SelectedElement = null;

            // Start with the select button
            if (object.ReferenceEquals(sender, SelectButton))
            {
                Core.App.Workspace.DrawingCanvas.CurrentState = null;
                return;
            }
            else if (object.ReferenceEquals(sender, StickyNoteButton))
            {
                // We have a custom state for placing sticky notes
                canvas.CurrentState = new UI.DrawingCanvas.States.PlacingStickyNote(
                    canvas, this);
                return;
            }

            // Get the type of object we are about to create
            Type newObjType = ((Border)sender).Tag as Type;

            // If it's a process unit, assign the state to create it
            if (newObjType.IsSubclassOf(typeof(GenericProcessUnit)))
            {
                canvas.CurrentState = new UI.DrawingCanvas.States.PlacingNewProcessUnit(
                    this, canvas, newObjType);
            }
            else
            {
                // Set the canvas state to setting a new object of the appropriate type
                // This state handles the placing of streams
                // TODO: Rename appropriately since it only handles streams
                Core.App.Workspace.DrawingCanvas.CurrentState = new UI.DrawingCanvas.States.PlacingNewStream(
                    this, Core.App.Workspace.DrawingCanvas, newObjType);
            }
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
                    // We've found a potential stream, but we need to make sure that it can be created under the 
                    // specified difficulty setting. The stream must have a static method for this and we'll use 
                    // reflection to find it.
                    MethodInfo mi = t.GetMethod("IsAvailableWithDifficulty",
                        new Type[]{typeof(OptionDifficultySetting)});

                    // Tell the developer that they need to add this in
                    if (null == mi)
                    {
                        throw new Exception("Note to developer: You have a class named " + t.Name +
                            " that inherits from AbstractStream but does not have the required static method " +
                            "\"IsAvailableWithDifficulty(OptionDifficultySetting)\". Please implement this " +
                            "method so that the control palette knows how to deal with the stream type.");
                    }

                    // There also needs to be a static string property for the title
                    PropertyInfo pi = t.GetProperty("Title", typeof(string));
                    if (null == pi)
                    {
                        throw new Exception("Note to developer: You have a class named " + t.Name +
                            " that inherits from AbstractStream but does not have the required static property " +
                            "\"string Title { get; }\". Please implement this property so that the control " + 
                            "palette knows how to label the stream creation option.");
                    }

                    // Execute it
                    bool available = (bool)mi.Invoke(null, new object[] { setting });
                    if (available)
                    {
                        StreamsPanel.Children.Add(CreateButton(t.Equals(typeof(PFD.Streams.HeatStream)) ?
                            "/UI/Icons/pu_heat_stream.png" : "/UI/Icons/pu_stream.png",
                            pi.GetGetMethod().Invoke(null, null) as string, t));
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
