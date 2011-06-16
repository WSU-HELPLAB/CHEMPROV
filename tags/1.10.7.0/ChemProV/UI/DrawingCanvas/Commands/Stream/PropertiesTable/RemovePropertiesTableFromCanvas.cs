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

namespace ChemProV.UI.DrawingCanvas.Commands.Stream.PropertiesTable
{
    public class RemovePropertiesTableFromCanvas : ICommand
    {
          /// <summary>
        /// Private reference to our canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private Panel canvas;
        private CommandFactory commandFactory = new CommandFactory();
        public Panel Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }

        /// <summary>
        /// Reference to the process unit to add the the canvas.
        /// </summary>
        private IPropertiesTable removingTable;

        public IPropertiesTable RemovingTable
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
                instance = new RemovePropertiesTableFromCanvas();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private RemovePropertiesTableFromCanvas()
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
