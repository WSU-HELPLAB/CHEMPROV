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

using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.Commands.StickyNoteCommand
{
    public class MoveStickyNoteCommand : ICommand
    {
        /// <summary>
        /// Reference to the process unit to add the the canvas.
        /// </summary>
        private StickyNote stickyNoteToMove;

        public StickyNote StickyNoteToMove
        {
            get { return stickyNoteToMove; }
            set { stickyNoteToMove = value; }
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
                instance = new MoveStickyNoteCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private MoveStickyNoteCommand()
        {
        }

        public bool Execute()
        {
            //first, we need to turn the IProcessUnit into something we can actually use, again
            //not a safe cast
            UserControl puAsUiElement = stickyNoteToMove as UserControl;

            //width and height needed to calculate position
            double width = puAsUiElement.ActualWidth;
            double height = puAsUiElement.ActualHeight;

            //set the PU's position
            puAsUiElement.SetValue(Canvas.LeftProperty, location.X - (width / 2));
                puAsUiElement.SetValue(Canvas.TopProperty, location.Y - stickyNoteToMove.Header.ActualHeight / 2);

         return true;   
        }
    }
}
