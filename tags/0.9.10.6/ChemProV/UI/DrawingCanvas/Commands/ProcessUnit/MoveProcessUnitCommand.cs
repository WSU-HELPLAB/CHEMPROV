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

using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable;

namespace ChemProV.UI.DrawingCanvas.Commands.ProcessUnit
{
    public class MoveProcessUnitCommand : ICommand
    {
        private IProcessUnit processUnitToMove;

        public IProcessUnit ProcessUnitToMove
        {
            get { return processUnitToMove; }
            set { processUnitToMove = value; }
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
                instance = new MoveProcessUnitCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="newProcessUnit">The process unit to add to the canvas</param>
        /// <param name="location">The location where we want the process unit added</param>
        private MoveProcessUnitCommand()
        {
        }

        /// <summary>
        /// Moves the process unit to the given point being its middle.
        /// </summary>
        public bool Execute()
        {
            //first, we need to turn the IProcessUnit into something we can actually use, again
            //not a safe cast
            UserControl puAsUiElement = ProcessUnitToMove as UserControl;

            //width and height needed to calculate position
            double width = puAsUiElement.Width;
            double height = puAsUiElement.Height;

            //set the PU's position
            puAsUiElement.SetValue(Canvas.LeftProperty, location.X - (width / 2));
            puAsUiElement.SetValue(Canvas.TopProperty, location.Y - (height / 2));

            return true;
        }

    }
}
