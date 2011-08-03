/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Controls;

using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.UI.DrawingCanvas.Commands.Stream.PropertiesWindow
{
    public class RemovePropertiesWindowFromCanvas : ICommand
    {
        /// <summary>
        /// Private reference to our drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private Panel canvas;
        private CommandFactory commandFactory = new CommandFactory();

        public Panel Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }

        /// <summary>
        /// Reference to the process unit to add the the drawing_canvas.
        /// </summary>
        private IPropertiesWindow removingTable;

        public IPropertiesWindow RemovingTable
        {
            get { return removingTable; }
            set { removingTable = value; }
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
                instance = new RemovePropertiesWindowFromCanvas();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private RemovePropertiesWindowFromCanvas()
        {
        }

        /// <summary>
        /// For deleteing a stream we must get rid of the table and any temporary process units it is connect too.
        /// We also need to let any non temporary process unit know we are dettaching from it.
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            removingTable.TableDataChanged += new TableDataEventHandler((canvas as DrawingCanvas).TableDataChanged);
            canvas.Children.Remove(removingTable as UIElement);
            return true;
        }
    }
}