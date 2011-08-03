/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Controls;

using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas.States;

namespace ChemProV.UI.DrawingCanvas.Commands.RightClickMenu
{
    public class AddContextMenuToCanvas : ICommand
    {
        /// <summary>
        /// Private reference to our drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private DrawingCanvas drawing_canvas;

        public DrawingCanvas Drawing_Canvas
        {
            get { return drawing_canvas; }
            set { drawing_canvas = value; }
        }

        private ContextMenu newContextMenu;

        public ContextMenu NewContextMenu
        {
            get { return newContextMenu; }
            set { newContextMenu = value; }
        }

        /// <summary>
        /// Reference to the target location where we'd like to add the process unit
        /// </summary>
        private Point location;

        public Point Location
        {
            get { return location; }
            set { location = value; }
        }

        private static ICommand instance;

        /// <summary>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static ICommand GetInstance()
        {
            if (instance == null)
            {
                instance = new AddContextMenuToCanvas();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private AddContextMenuToCanvas()
        {
        }

        /// <summary>
        /// Adds the process unit to the given drawing_canvas at the given point being its middle.
        /// </summary>
        public bool Execute()
        {
            if (drawing_canvas.HoveringOver == null && drawing_canvas.HoveringOverStickyNote == null && drawing_canvas.SelectedElement == null)
            {
                MenuItem menuItem = new MenuItem();

                menuItem.Header = "Undo";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler((drawing_canvas.MenuState as MenuState).Undo);

                menuItem = new MenuItem();
                menuItem.Header = "Redo";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler((drawing_canvas.MenuState as MenuState).Redo);
                menuItem = new MenuItem();
                menuItem.Header = "Hide All Sticky Notes";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).HideStickyNotes));
                menuItem = new MenuItem();
                menuItem.Header = "Show All Sticky Notes";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ShowStickyNotes));
                // NewContextMenu.SetValue(System.Windows.Controls.Canvas.LeftProperty, e.GetPosition(drawing_canvas).X);
                // NewContextMenu.SetValue(System.Windows.Controls.Canvas.TopProperty, e.GetPosition(drawing_canvas).Y);
                //NewContextMenu.SizeChanged += new SizeChangedEventHandler(newContextMenu_SizeChanged);

                //It does not make sense that the mouse postion has to be relative to drawing_canvas's parent but it does
                //if not the menu will appear in the top left of the page
            }
            else
            {
                if (drawing_canvas.HoveringOver != null)
                {
                    drawing_canvas.SelectedElement = drawing_canvas.HoveringOver;
                }

                MenuItem menuItem = new MenuItem();
                newContextMenu = new ContextMenu();

                menuItem.Header = "Undo";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).Undo));

                menuItem = new MenuItem();
                menuItem.Header = "Redo";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).Redo));
                menuItem = new MenuItem();
                menuItem.Header = "Delete";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).Delete));
                menuItem = new MenuItem();
                menuItem.Header = "Hide All Sticky Notes";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).HideStickyNotes));
                menuItem = new MenuItem();
                menuItem.Header = "Show All Sticky Notes";
                newContextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ShowStickyNotes));

                if (drawing_canvas.HoveringOverStickyNote != null)
                {
                    drawing_canvas.SelectedElement = drawing_canvas.HoveringOverStickyNote;
                    menuItem = new MenuItem();
                    menuItem.Header = "Blue";
                    newContextMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Pink";
                    newContextMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Green";
                    newContextMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Orange";
                    newContextMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Yellow";
                    newContextMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((drawing_canvas.MenuState as MenuState).ChangeColor));
                }

                if (drawing_canvas.SelectedElement is StreamSourceIcon)
                {
                    drawing_canvas.SelectedElement = (drawing_canvas.SelectedElement as StreamSourceIcon).Stream;
                }
                else if (drawing_canvas.SelectedElement is StreamDestinationIcon)
                {
                    drawing_canvas.SelectedElement = (drawing_canvas.SelectedElement as StreamDestinationIcon).Stream;
                }
            }
            newContextMenu.SetValue(Canvas.LeftProperty, location.X);
            newContextMenu.SetValue(Canvas.TopProperty, location.Y);

            //This is above everything else
            newContextMenu.SetValue(Canvas.ZIndexProperty, 4);
            drawing_canvas.Children.Add(newContextMenu);

            drawing_canvas.NewContextMenu = newContextMenu;
            return true;
        }
    }
}