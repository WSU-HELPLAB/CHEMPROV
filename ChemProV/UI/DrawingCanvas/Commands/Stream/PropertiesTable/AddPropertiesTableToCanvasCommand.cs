/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.UI.DrawingCanvas.Commands.Stream.PropertiesWindow
{
    public class AddPropertiesWindowToCanvasCommand : ICommand
    {
        /// <summary>
        /// Private reference to our drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private Panel canvas;

        public Panel Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }

        /// <summary>
        /// Reference to the table to add the the drawing_canvas.
        /// </summary>
        private IPropertiesWindow newTable;

        public IPropertiesWindow NewTable
        {
            get { return newTable; }
            set { newTable = value; }
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
                instance = new AddPropertiesWindowToCanvasCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private AddPropertiesWindowToCanvasCommand()
        {
        }

        /// <summary>
        /// Adds the table to the layout root so its middle is at the point given.
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            UserControl puAsUiElement = newTable as UserControl;

            //width and height needed to calculate position, for some reaons it did not like puAsUiElemnt.Width had to
            //go with ActualWidth and ActualHeight but everything else had to b e Width and Height.
            double width = puAsUiElement.ActualWidth;
            double height = puAsUiElement.ActualHeight;

            //set the PU's position, if applicable
            if (location.X > 0 && location.Y > 0)
            {
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.LeftProperty, location.X - (width / 2));
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.TopProperty, location.Y - (height / 2));
            }

            //This sets the tables index to the greatest so it will be above everything
            puAsUiElement.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 3);

            newTable.TableDataChanged -= new TableDataEventHandler((canvas as DrawingCanvas).TableDataChanged);
            newTable.TableDataChanged += new TableDataEventHandler((canvas as DrawingCanvas).TableDataChanged);

            newTable.TableDataChanging += new EventHandler((canvas as DrawingCanvas).TableDataChanging);

            puAsUiElement.MouseLeftButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonDownHandler);
            puAsUiElement.MouseLeftButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonUpHandler);

            try
            {
                canvas.Children.Add(newTable as UIElement);
            }
            catch
            {
                MessageBox.Show("ChemProV has encountered an error, please delete the stream you just tried to make and the previous one and then re-create them, sorry for this inconvenience");
            }

            return true;
        }
    }
}