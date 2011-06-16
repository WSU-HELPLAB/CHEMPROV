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
            /*    commandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, e.GetPosition(drawing_canvas)).Execute();
                drawing_canvas.SelectedPaletteItem = drawing_canvas.SelectedElement;
                drawing_canvas.CurrentState = drawing_canvas.PlacingState;*/
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
                validMove = CommandFactory.CreateCommand(CanvasCommands.MoveTail, ((canvas as DrawingCanvas).SelectedElement as StreamSourceIcon).Stream, canvas, e.GetPosition(canvas), previousLocation).Execute();
            }
            else if ((canvas as DrawingCanvas).SelectedElement is StreamDestinationIcon)
            {
                validMove = CommandFactory.CreateCommand(CanvasCommands.MoveHead, ((canvas as DrawingCanvas).SelectedElement as StreamDestinationIcon).Stream, canvas, e.GetPosition(canvas), previousLocation).Execute();
            }
            else
            {
                validMove = CommandFactory.CreateCommand(CanvasCommands.MoveHead, canvas.SelectedElement, canvas, e.GetPosition(canvas), previousLocation).Execute();
            }
            previousLocation = e.GetPosition(canvas);
        }



        /// <summary>
        /// We have stopped our dragging need to check if it was a valid move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            previousLocation = new Point(-2, -2);
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
                    IStream stream = (canvas.SelectedElement as StreamDestinationIcon).Stream;
                    if (stream.Source == stream.Destination)
                    {
                        //source and dest are the same which is not allowed do not exit moving state
                        return;
                    }
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                }
                else if (canvas.SelectedElement is StreamSourceIcon)
                {
                    IStream stream = (canvas.SelectedElement as StreamSourceIcon).Stream;
                    if (stream.Source == stream.Destination)
                    {
                        //source and dest are the same which is not allowed do not exit moving state
                        return;
                    }
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
                    }
                }
                else if (canvas.SelectedElement is IProcessUnit)
                {
                    if (ProcessUnitFactory.GetProcessUnitType((canvas.SelectedElement as IProcessUnit)) == ProcessUnitType.HeatExchanger)
                    {
                        if ((canvas.SelectedElement as IProcessUnit).IncomingStreams.Count == 0)
                        {
                            HeatStream hs = new HeatStream();
                            hs.Destination = canvas.SelectedElement as IProcessUnit;
                            (canvas.SelectedElement as IProcessUnit).AttachIncomingStream(hs);
                            CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, hs, canvas, previousLocation).Execute();
                            canvas.SelectedElement = new StreamSourceIcon(hs, hs.rectangle);
                            
                            //Remove the event listener from the arrow so it is not dettachable
                            hs.Arrow_MouseButtonLeftDown -= new MouseButtonEventHandler((canvas as DrawingCanvas).HeadMouseLeftButtonDownHandler);

                            canvas.UpdateCanvasSize();
                            //need to stay in moving state so return so we dont go to selectedState
                            return;
                        }
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

