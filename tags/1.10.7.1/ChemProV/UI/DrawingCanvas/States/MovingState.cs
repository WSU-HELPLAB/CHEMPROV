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

using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class MovingState : IState
    {

        private DrawingCanvas canvas;
        private bool validMove = true;
        public Point previousLocation = new Point();

        public MovingState(DrawingCanvas c)
        {
            canvas = c;
        }

        #region IState Members

        public void MouseLeave(object sender, MouseEventArgs e)
        {
            /*    commandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.SelectedElement, canvas, e.GetPosition(canvas)).Execute();
                canvas.SelectedPaletteItem = canvas.SelectedElement;
                canvas.CurrentState = canvas.PlacingState;*/
        }

        /// <summary>
        /// Moving the mouse and in movingstate so we are moving the selectedElement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if ((canvas as DrawingCanvas).SelectedElement is StreamSourceIcon)
            {
                validMove = CommandFactory.CreateCommand(CanvasCommands.MoveTail, ((canvas as DrawingCanvas).SelectedElement as StreamSourceIcon).Stream, canvas, e.GetPosition(canvas)).Execute();
            }
            else if ((canvas as DrawingCanvas).SelectedElement is StreamDestinationIcon)
            {
                validMove = CommandFactory.CreateCommand(CanvasCommands.MoveHead, ((canvas as DrawingCanvas).SelectedElement as StreamDestinationIcon).Stream, canvas, e.GetPosition(canvas)).Execute();
            }
            else
            {
                validMove = CommandFactory.CreateCommand(CanvasCommands.MoveHead, canvas.SelectedElement, canvas, e.GetPosition(canvas)).Execute();
            }

        }



        /// <summary>
        /// We have stopped our dragging need to check if it was a valid move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (validMove == false)
            {
                if (canvas.SelectedElement is StreamDestinationIcon)
                {
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                    CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, (canvas.SelectedElement as StreamDestinationIcon).Stream, canvas, new Point()).Execute();

                    //This must either be a new stream command or a move command.
                    StreamUndo streamUndo = canvas.undoStack.First.Value as StreamUndo;
                    canvas.undoStack.RemoveFirst();
                    if (streamUndo.CommandIssed == CanvasCommands.MoveHead)
                    {
                        canvas.saveState(CanvasCommands.RemoveFromCanvas, (canvas.SelectedElement as StreamDestinationIcon).Stream, canvas, streamUndo.Location);
                    }
                    else
                    {
                        //do nothing we removed the add command so now it is like this never happend.
                    }
                }
                else if (canvas.SelectedElement is StreamSourceIcon)
                {
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                    CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, (canvas.SelectedElement as StreamSourceIcon).Stream, canvas, new Point()).Execute();

                    //This must either be a new stream command or a move command.
                    StreamUndo streamUndo = canvas.undoStack.First.Value as StreamUndo;
                    canvas.undoStack.RemoveFirst();

                    if (streamUndo.CommandIssed == CanvasCommands.MoveTail)
                    {
                        canvas.saveState(CanvasCommands.RemoveFromCanvas, (canvas.SelectedElement as StreamSourceIcon).Stream, canvas, streamUndo.Location);
                    }
                    else
                    {
                        //do nothing we removed the add command so now it is like this never happend.
                    }

                }
                else
                {
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                    CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.SelectedElement, canvas, new Point()).Execute();
                }
                validMove = true;
                canvas.SelectedElement = null;
                canvas.CurrentState = canvas.NullState;
            }
            else
            {   
                if (canvas.SelectedElement is StreamDestinationIcon)
                {
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                }
                else if (canvas.SelectedElement is StreamSourceIcon)
                {
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                }
                //we have finished a move need to update the saved state for the move with new location
                canvas.CurrentState = canvas.SelectedState;
                canvas.UpdateCanvasSize();
            }
        }

        #region Unused Mouse Events
        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        #endregion
        #endregion
    }
}

