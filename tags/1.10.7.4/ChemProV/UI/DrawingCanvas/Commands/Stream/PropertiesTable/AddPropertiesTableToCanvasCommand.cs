/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;

namespace ChemProV.UI.DrawingCanvas.Commands.Stream.PropertiesTable
{
    public class AddPropertiesTableToCanvasCommand : ICommand
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
        private IPropertiesTable newTable;

        public IPropertiesTable NewTable
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
                instance = new AddPropertiesTableToCanvasCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private AddPropertiesTableToCanvasCommand()
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

            canvas.Children.Add(newTable as UIElement);

            return true;
        }

        
    }
}
