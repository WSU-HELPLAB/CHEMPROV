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

using ChemProV.PFD.ProcessUnits;

namespace ChemProV.UI.DrawingCanvas.Commands.ProcessUnit
{
    public class AddProcessUnitToCanvasCommand : ICommand
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
        /// Reference to the process unit to add the the drawing_canvas.
        /// </summary>
        private IProcessUnit newProcessUnit;

        public IProcessUnit NewProcessUnit
        {
            get { return newProcessUnit; }
            set { newProcessUnit = value; }
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
                instance = new AddProcessUnitToCanvasCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private AddProcessUnitToCanvasCommand()
        {
        }

        /// <summary>
        /// Adds the process unit to the given drawing_canvas at the given point being its middle.
        /// </summary>
        public bool Execute()
        {
            //first, we need to turn the IProcessUnit into something we can actually use, again
            //not a safe cast

            UserControl puAsUiElement = newProcessUnit as UserControl;

            //width and height needed to calculate position
            double width = puAsUiElement.Width;
            double height = puAsUiElement.Height;

            //set the PU's position
            if (Location.X > 0 && Location.Y > 0)
            {
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.LeftProperty, location.X - (width / 2));
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.TopProperty, location.Y - (height / 2));
            }

            //add event listeners
            if (newProcessUnit is TemporaryProcessUnit)
            {
                puAsUiElement.MouseLeftButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).TempProcessUnitMouseLeftButtonDownHandler);
                puAsUiElement.MouseRightButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).TempProcessUnitMouseRightButtonDownHandler);
                //sets the temporary process unit to below that of more
                //specific process units but above the stream.  This allows the stream to be dragged onto
                //more descriptive elements at a later time
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 1);
            }
            else
            {
                //This sets a specific process unit above temporary and streams
                puAsUiElement.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 2);
                puAsUiElement.MouseLeftButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonDownHandler);
                puAsUiElement.MouseLeftButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonUpHandler);
                puAsUiElement.MouseRightButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseRightButtonDownHandler);
                puAsUiElement.MouseRightButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseRightButtonUpHandler);
                puAsUiElement.MouseEnter += new MouseEventHandler((canvas as DrawingCanvas).IProcessUnit_MouseEnter);
                puAsUiElement.MouseLeave += new MouseEventHandler((canvas as DrawingCanvas).IProcessUnit_MouseLeave);

                newProcessUnit.StreamsChanged += new EventHandler((canvas as DrawingCanvas).ProcessUnitStreamsChanged);
            }

            //add the PU to the drawing drawing_canvas
            canvas.Children.Add(puAsUiElement);
            return true;
        }
    }
}