/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;

using ChemProV.PFD.StickyNote;
using ChemProV.UI.DrawingCanvas.States;

namespace ChemProV.UI.DrawingCanvas.Commands.StickyNoteCommand
{
    public class MoveStickyNoteCommand : ICommand
    {
        /// <summary>
        /// Reference to the process unit to add the the drawing_canvas.
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
        private Point currentMouseLocation;

        public Point CurrentMouseLocation
        {
            get { return currentMouseLocation; }
            set { currentMouseLocation = value; }
        }

        private Point previousMouseLocation;

        public Point PreviousMouseLocation
        {
            get { return previousMouseLocation; }
            set { previousMouseLocation = value; }
        }

        private DrawingCanvas drawingcanvas;

        public DrawingCanvas Drawingcanvas
        {
            get { return drawingcanvas; }
            set { drawingcanvas = value; }
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

            //basically if coming from placing state
            if (previousMouseLocation.X == -1 && previousMouseLocation.Y == -1)
            {
                puAsUiElement.SetValue(Canvas.LeftProperty, currentMouseLocation.X - (width / 2));
                puAsUiElement.SetValue(Canvas.TopProperty, currentMouseLocation.Y - (stickyNoteToMove.Header.ActualHeight / 2));
                (drawingcanvas.MovingState as MovingState).previousLocation = currentMouseLocation;
                return true;
            }

                //is this our first moving state?
            else if (previousMouseLocation.X == -2 && previousMouseLocation.Y == -2)
            {
                return true;
            }
            //basically if coming from placing state
            else if (previousMouseLocation.X == -3 && previousMouseLocation.Y == -3)
            {
                puAsUiElement.SetValue(Canvas.LeftProperty, currentMouseLocation.X - (width / 2));
                puAsUiElement.SetValue(Canvas.TopProperty, currentMouseLocation.Y);
                (drawingcanvas.MovingState as MovingState).previousLocation = currentMouseLocation;
                return true;
            }

            //otherwise we are in moving it around

            //this gets a percent of how far right we are.  Left-hand side is 0 percent right-hand side is 100%. This is
            //represented in decimal form so mouseOffSet must be between 0 and 1.
            double mouseOffSetX = (previousMouseLocation.X - (double)puAsUiElement.GetValue(Canvas.LeftProperty)) / width;

            //this can happen if width is zero or if the LeftProperty of the element has not been set yet.
            if (double.IsNaN(mouseOffSetX))
            {
                return true;
            }
            else if (mouseOffSetX < 0)
            {
                mouseOffSetX = 0;
            }
            else if (mouseOffSetX > 1)
            {
                mouseOffSetX = 1;
            }

            //this uses the percent to calcualate how far over we should push the drawing_canvas.leftProperty
            double mouseOffSetY = previousMouseLocation.Y - stickyNoteToMove.Header.ActualHeight / 2;

            Point Position = new Point();

            Position.X = (currentMouseLocation.X - (width * mouseOffSetX));
            Position.Y = currentMouseLocation.Y - stickyNoteToMove.Header.ActualHeight / 2;

            //NOTE: Doesn't check if bigger than drawing_canvas on purpose, drawing_canvas should  grow.
            if (Position.X < 0)
            {
                Position.X = 0;
            }

            if (Position.Y < 0)
            {
                Position.Y = 0;
            }

            puAsUiElement.SetValue(Canvas.LeftProperty, Position.X);
            puAsUiElement.SetValue(Canvas.TopProperty, Position.Y);

            return true;
        }
    }
}