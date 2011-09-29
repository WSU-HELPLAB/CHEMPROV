/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.UI.DrawingCanvas.Commands.DrawingCanvasCommands;

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

        private bool AttachStream(TemporaryProcessUnit tpu, Point Location)
        {
            bool deletedSomething = false;

            //call stateChanger to take care of everything, it is undo and is not recursion
            ChangeStateCommand changeState = ChangeStateCommand.GetInstance() as ChangeStateCommand;
            changeState.Undo = true;
            changeState.Drawing_Canvas = canvas;

            if (canvas.IsReadOnly)
            {
                //since we are in readOnly mode the user cannot attach a process unit to a stream by dragging the process unit onto
                //its soruce or destiation so automatically undo the action
                changeState.Execute();
                return deletedSomething;
            }

            if (tpu.IncomingStreams.Count != 0)
            {
                if ((canvas.SelectedElement as IProcessUnit).IsAcceptingIncomingStreams(tpu.IncomingStreams[0]))
                {
                    //make sure it is not an outgoing stream
                    foreach (IStream stream in (canvas.SelectedElement as IProcessUnit).OutgoingStreams)
                    {
                        if (tpu.IncomingStreams[0] == stream)
                        {
                            changeState.Execute();
                            return deletedSomething;
                        }
                    }
                    CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, tpu.IncomingStreams[0].Destination, canvas, Location).Execute();
                    //we removed an element so decrement i by one.
                    deletedSomething = true;
                    (tpu.IncomingStreams[0] as AbstractStream).DestinationArrorVisbility = true;
                    (canvas.SelectedElement as IProcessUnit).AttachIncomingStream(tpu.IncomingStreams[0]);
                    tpu.IncomingStreams[0].Destination = canvas.SelectedElement as IProcessUnit;
                }
                else
                {
                    changeState.Execute();
                }
            }
            else
            {
                if ((canvas.SelectedElement as IProcessUnit).IsAcceptingOutgoingStreams(tpu.OutgoingStreams[0]))
                {
                    //make sure it is not an incoming stream
                    foreach (IStream stream in (canvas.SelectedElement as IProcessUnit).IncomingStreams)
                    {
                        if (tpu.OutgoingStreams[0] == stream)
                        {
                            changeState.Execute();
                            return deletedSomething;
                        }
                    }
                    CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, tpu.OutgoingStreams[0].Source, canvas, Location).Execute();
                    //we removed an element so decrement i by one.
                    deletedSomething = true;
                    (tpu.OutgoingStreams[0] as AbstractStream).SourceRectangleVisbility = true;
                    (canvas.SelectedElement as IProcessUnit).AttachOutgoingStream(tpu.OutgoingStreams[0]);
                    tpu.OutgoingStreams[0].Source = canvas.SelectedElement as IProcessUnit;
                }
                else
                {
                    changeState.Execute();
                }
            }
            return deletedSomething;
        }

        /// <summary>
        /// We have stopped our dragging need to check if it was a valid move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            previousLocation = new Point(-2, -2);

            //this determines if the process unit was placed ontop of a temporaryProcessUnit and if so it deals with it
            if (canvas.SelectedElement is IProcessUnit)
            {
                UserControl processUnitAsUserControl = canvas.SelectedElement as UserControl;
                int i = 0;
                for (i = 0; i < canvas.Children.Count; i++)
                {
                    UIElement ui = canvas.Children[i];
                    if (ui is IProcessUnit && ui != canvas.SelectedElement)
                    {
                        UserControl tpu = (ui as UserControl);
                        Point tpuTopLeftPoint = new Point((double)ui.GetValue(Canvas.LeftProperty), (double)ui.GetValue(Canvas.TopProperty));
                        Point tpuBottemRightPoint = new Point(tpuTopLeftPoint.X + tpu.Width, tpuTopLeftPoint.Y + tpu.Height);
                        Point processUnitTopLeft = new Point((double)processUnitAsUserControl.GetValue(Canvas.LeftProperty), (double)processUnitAsUserControl.GetValue(Canvas.TopProperty));
                        Point ProcessUnitBottemRight = new Point(processUnitTopLeft.X + processUnitAsUserControl.Width, processUnitTopLeft.Y + processUnitAsUserControl.Height);

                        //Checks the top left conner of the tpu to see if it is in the PU
                        if (processUnitTopLeft.X < tpuTopLeftPoint.X && ProcessUnitBottemRight.X > tpuTopLeftPoint.X && processUnitTopLeft.Y < tpuTopLeftPoint.Y && ProcessUnitBottemRight.Y > tpuTopLeftPoint.Y)
                        {
                            if (tpu is TemporaryProcessUnit)
                            {
                                //attach stream
                                if (AttachStream(tpu as TemporaryProcessUnit, tpuTopLeftPoint))
                                {
                                    //AttachStream returns true if it deleted something if it did then we need to decrement i else i will miss an element
                                    i--;
                                }
                            }
                            else
                            {
                                ChangeStateCommand changeState = ChangeStateCommand.GetInstance() as ChangeStateCommand;
                                changeState.Undo = true;
                                changeState.Drawing_Canvas = canvas;
                                changeState.Execute();
                            }
                        }
                        //Checks the top right conner of the tpu to see if it is in the PU
                        else if (ProcessUnitBottemRight.X > tpuTopLeftPoint.X && processUnitTopLeft.X < tpuBottemRightPoint.X && processUnitTopLeft.Y < tpuTopLeftPoint.Y && ProcessUnitBottemRight.Y > tpuTopLeftPoint.Y)
                        {
                            if (tpu is TemporaryProcessUnit)
                            {
                                if (canvas.IsReadOnly)
                                {
                                    ChangeStateCommand changeState = ChangeStateCommand.GetInstance() as ChangeStateCommand;
                                    changeState.Undo = true;
                                    changeState.Drawing_Canvas = canvas;
                                    changeState.Execute();
                                }
                                else
                                {
                                    //attach stream
                                    if (AttachStream(tpu as TemporaryProcessUnit, tpuTopLeftPoint))
                                    {
                                        //AttachStream returns true if it deleted something if it did then we need to decrement i else i will miss an element
                                        i--;
                                    }
                                }
                            }
                            else
                            {
                                ChangeStateCommand changeState = ChangeStateCommand.GetInstance() as ChangeStateCommand;
                                changeState.Undo = true;
                                changeState.Drawing_Canvas = canvas;
                                changeState.Execute();
                            }
                        }
                        //Checks the bottem right conner of the tpu to see if it is in the PU
                        else if (processUnitTopLeft.X < tpuBottemRightPoint.X && ProcessUnitBottemRight.X > tpuBottemRightPoint.X && processUnitTopLeft.Y < tpuBottemRightPoint.Y && ProcessUnitBottemRight.Y > tpuBottemRightPoint.Y)
                        {
                            if (tpu is TemporaryProcessUnit)
                            {
                                //attach stream
                                if (AttachStream(tpu as TemporaryProcessUnit, tpuTopLeftPoint))
                                {
                                    //AttachStream returns true if it deleted something if it did then we need to decrement i else i will miss an element
                                    i--;
                                }
                            }
                            else
                            {
                                ChangeStateCommand changeState = ChangeStateCommand.GetInstance() as ChangeStateCommand;
                                changeState.Undo = true;
                                changeState.Drawing_Canvas = canvas;
                                changeState.Execute();
                            }
                        }
                        //Checks the top right conner of the tpu to see if it is in the PU
                        else if (ProcessUnitBottemRight.X > tpuTopLeftPoint.X && processUnitTopLeft.X < tpuTopLeftPoint.X && processUnitTopLeft.Y < tpuBottemRightPoint.Y && ProcessUnitBottemRight.Y > tpuBottemRightPoint.Y)
                        {
                            if (tpu is TemporaryProcessUnit)
                            {
                                //attach stream
                                if (AttachStream(tpu as TemporaryProcessUnit, tpuTopLeftPoint))
                                {
                                    //AttachStream returns true if it deleted something if it did then we need to decrement i else i will miss an element
                                    i--;
                                }
                            }
                            else
                            {
                                ChangeStateCommand changeState = ChangeStateCommand.GetInstance() as ChangeStateCommand;
                                changeState.Undo = true;
                                changeState.Drawing_Canvas = canvas;
                                changeState.Execute();
                            }
                        }
                    }
                }
            }

            if (validMove == false)
            {
                if (canvas.SelectedElement is StreamDestinationIcon)
                {
                    if ((canvas as DrawingCanvas).HoveringOver is IProcessUnit)
                    {
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NoBorder);
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
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NoBorder);
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
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NoBorder);
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
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NoBorder);
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
                        ((canvas as DrawingCanvas).HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NoBorder);
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

        #endregion Unused Mouse Events

        #endregion IState Members

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
            MouseLeftButtonUp(sender, e as MouseButtonEventArgs);
        }
    }
}