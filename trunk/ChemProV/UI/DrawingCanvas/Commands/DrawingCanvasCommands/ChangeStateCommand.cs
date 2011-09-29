/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System.Collections.Generic;

/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.UI.DrawingCanvas.Commands.DrawingCanvasCommands
{
    public class ChangeStateCommand : ICommand
    {
        /// <summary>
        /// Private reference to our drawing_drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private DrawingCanvas drawing_canvas;

        public DrawingCanvas Drawing_Canvas
        {
            get { return drawing_canvas; }
            set { drawing_canvas = value; }
        }

        private bool undo;

        public bool Undo
        {
            get { return undo; }
            set { undo = value; }
        }

        private bool recursion = false;

        private static ICommand instance;

        /// <summary>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static ICommand GetInstance()
        {
            if (instance == null)
            {
                instance = new ChangeStateCommand(true, false);
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private ChangeStateCommand(bool undo, bool recursion)
        {
            this.undo = undo;
            this.recursion = recursion;
        }

        /// <summary>
        /// This function is called from undo or redo
        /// </summary>
        /// <param name="undo">If we want to undo this is true but if we want to redo this is false</param>
        /// <param name="recursion">if statechanger is calling itself set to true else false, this is so we dont save intermedier steps</param>
        public bool Execute()
        {
            /*
             * The logic of this funcition
             * first find if we are dealing with a stream or a processUnit.
             * Then figure out what command we are doing.  This effects the command we do and how we save the state
             * Then figure out if we are doing undo or redo. If undo and command is delete then do the opposite which is add but if redo and command is delete then do delete
             * Then figure out if we are doing recursion, if not doing recursion save state if we are doing recurions do not save state.
             */
            SavedStateObject desiredState;
            if (undo == true)
            {
                if (drawing_canvas.undoStack.Count > 0)
                {
                    desiredState = drawing_canvas.undoStack.First.Value;
                    drawing_canvas.undoStack.RemoveFirst();
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (drawing_canvas.redoStack.Count > 0)
                {
                    desiredState = drawing_canvas.redoStack.First.Value;
                    drawing_canvas.redoStack.RemoveFirst();
                }
                else
                {
                    return true;
                }
            }

            //This is to see if we are changing a stream or a IprocessUnit
            if (desiredState is StreamUndo)
            {
                StreamUndo changeStream = desiredState as StreamUndo;
                if (changeStream.CommandIssed == CanvasCommands.AddToCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.redoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.Source as IProcessUnit, changeStream.Destination as IProcessUnit, new Point()));
                        }
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated, changeStream.TheCanvasUsed, new Point()).Execute();
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.undoStack.AddFirst(changeStream);
                        }
                        //We need to add back the stream and if it had temporary processUnit we need to add those back too
                        if ((changeStream.Source is TemporaryProcessUnit))
                        {
                            //Need to get back the source tempProcessUnit
                            drawing_canvas.redoStack.AddFirst(changeStream.SourceUndo);

                            //We are redoing so undo is false, and we are doing recursion so that is true
                            (new ChangeStateCommand(false, true) { Drawing_Canvas = drawing_canvas }).Execute();
                        }
                        else
                        {
                            changeStream.StreamManipulated.Source = changeStream.Source;
                            changeStream.Source.AttachOutgoingStream(changeStream.StreamManipulated);
                        }

                        if ((changeStream.Destination is TemporaryProcessUnit))
                        {
                            drawing_canvas.redoStack.AddFirst(changeStream.DestinationUndo);
                            (new ChangeStateCommand(false, true) { Drawing_Canvas = drawing_canvas }).Execute();
                        }
                        else
                        {
                            changeStream.StreamManipulated.Destination = changeStream.Destination;
                            changeStream.Destination.AttachIncomingStream(changeStream.StreamManipulated);
                        }
                        (drawing_canvas as DrawingCanvas).HoveringOver = null;
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changeStream.StreamManipulated, changeStream.TheCanvasUsed, new Point()).Execute();
                    }
                }
                else if (changeStream.CommandIssed == CanvasCommands.MoveHead)
                {
                    if (undo == true && recursion != true)
                    {
                        //we need to save the current state and push it onto the redo stack
                        Point destinationLocation = new Point((double)(changeStream.StreamManipulated.Destination as UIElement).GetValue(Canvas.LeftProperty), (double)(changeStream.StreamManipulated.Destination as UIElement).GetValue(Canvas.TopProperty));

                        //NOTE changeStream.Source is what the source of the changeStream used to be before we it changed.
                        //Whereas changeStream.StreamManipualted gets us a reference to the stream as is, so if we add .Source onto the end of it we get the current soource

                        drawing_canvas.redoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, destinationLocation));
                    }
                    else if (undo == false && recursion != true)
                    {
                        //we need to save the current state and push it onto the redo stack
                        Point destinationLocation = new Point((double)(changeStream.StreamManipulated.Destination as UIElement).GetValue(Canvas.LeftProperty), (double)(changeStream.StreamManipulated.Destination as UIElement).GetValue(Canvas.TopProperty));

                        //NOTE changeStream.Source is what the source of the changeStream used to be before we it changed.
                        //Whereas changeStream.StreamManipualted gets us a reference to the stream as is, so if we add .Source onto the end of it we get the current soource

                        drawing_canvas.undoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, destinationLocation));
                    }
                    //now we need to issue the undo command
                    if (changeStream.Destination is TemporaryProcessUnit)
                    {
                        CommandFactory.CreateCommand(CanvasCommands.MoveHead, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.Location).Execute();
                    }
                    else if (changeStream.Destination is IProcessUnit)
                    {
                        //if the stream currently points to a a temporary process unit we need to remove it
                        if (changeStream.StreamManipulated.Destination is TemporaryProcessUnit)
                        {
                            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated.Destination, changeStream.TheCanvasUsed, new Point()).Execute();
                        }
                        else
                        {
                            //so it wasn't pointing to a temporyprocessunit so we dont need to delete but we do need to let it know we are dettaching from it.
                            changeStream.StreamManipulated.Destination.DettachIncomingStream(changeStream.StreamManipulated);
                        }
                        changeStream.StreamManipulated.Destination = changeStream.Destination;
                        changeStream.StreamManipulated.StreamDestination.DestinationIcon.Visibility = Visibility.Visible;
                        changeStream.Destination.AttachIncomingStream(changeStream.StreamManipulated);
                    }
                }
                else if (changeStream.CommandIssed == CanvasCommands.MoveTail)
                {
                    if (undo == true && recursion != true)
                    {
                        //we need to save the current state and push it onto the redo stack
                        Point sourceLocation = new Point((double)(changeStream.StreamManipulated.Source as UIElement).GetValue(Canvas.LeftProperty), (double)(changeStream.StreamManipulated.Source as UIElement).GetValue(Canvas.TopProperty));

                        //NOTE changeStream.Source is what the source of the changeStream used to be before we it changed.
                        //Whereas changeStream.StreamManipualted gets us a reference to the stream as is, so if we add .Source onto the end of it we get the current soource

                        drawing_canvas.redoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, sourceLocation));
                    }
                    else if (undo == false && recursion != true)
                    {
                        //we need to save the current state and push it onto the redo stack
                        Point sourceLocation = new Point((double)(changeStream.StreamManipulated.Source as UIElement).GetValue(Canvas.LeftProperty), (double)(changeStream.StreamManipulated.Source as UIElement).GetValue(Canvas.TopProperty));

                        //NOTE changeStream.Source is what the source of the changeStream used to be before we it changed.
                        //Whereas changeStream.StreamManipualted gets us a reference to the stream as is, so if we add .Source onto the end of it we get the current soource

                        drawing_canvas.undoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, sourceLocation));
                    }
                    if (changeStream.Source is TemporaryProcessUnit)
                    {
                        CommandFactory.CreateCommand(CanvasCommands.MoveTail, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.Location).Execute();
                    }
                    else if (changeStream.Source is IProcessUnit)
                    {
                        //if the stream currently points to a a temporary process unit we need to remove it
                        if (changeStream.StreamManipulated.Source is TemporaryProcessUnit)
                        {
                            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated.Source, changeStream.TheCanvasUsed, new Point()).Execute();
                        }
                        else
                        {
                            //so it wasn't pointing to a temporyprocessunit so we dont need to delete but we do need to let it know we are dettaching from it.
                            changeStream.StreamManipulated.Source.DettachOutgoingStream(changeStream.StreamManipulated);
                        }

                        changeStream.StreamManipulated.Source = changeStream.Source;
                        changeStream.StreamManipulated.StreamSource.SourceIcon.Visibility = Visibility.Visible;
                        changeStream.Source.AttachOutgoingStream(changeStream.StreamManipulated);
                    }
                }
                else if (changeStream.CommandIssed == CanvasCommands.RemoveFromCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            //we dont need to save the current state since we just gotta remove it from the drawing_drawing_canvas and since everything will be the same we can just use the changeStream.
                            drawing_canvas.redoStack.AddFirst(changeStream);
                        }
                        //We need to add back the stream and if it had temporary processUnit we need to add those back too
                        if (changeStream.Source is TemporaryProcessUnit)
                        {
                            //Need to get back the source tempProcessUnit
                            drawing_canvas.undoStack.AddFirst(changeStream.SourceUndo);

                            //we are doing undo and this is recursion
                            (new ChangeStateCommand(true, true) { Drawing_Canvas = drawing_canvas }).Execute();
                        }
                        else if (changeStream.Source == null)
                        {
                            IStream streamToChange = changeStream.StreamManipulated;
                            IProcessUnit source = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Source);
                            CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, source, drawing_canvas, source.MidPoint).Execute();
                            streamToChange.Source = source;
                            source.AttachOutgoingStream(streamToChange);
                        }
                        else
                        {
                            changeStream.StreamManipulated.Source = changeStream.Source;
                            changeStream.Source.AttachOutgoingStream(changeStream.StreamManipulated);
                        }

                        if (changeStream.Destination is TemporaryProcessUnit)
                        {
                            drawing_canvas.undoStack.AddFirst(changeStream.DestinationUndo);

                            //we are doing undo and this is recursion
                            (new ChangeStateCommand(true, true) { Drawing_Canvas = drawing_canvas }).Execute();
                        }
                        else if (changeStream.Destination == null)
                        {
                            IStream streamToChange = changeStream.StreamManipulated;
                            IProcessUnit sink = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Sink);
                            CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, sink, drawing_canvas, sink.MidPoint).Execute();
                            streamToChange.Destination = sink;
                            sink.AttachIncomingStream(streamToChange);
                        }
                        else
                        {
                            changeStream.StreamManipulated.Destination = changeStream.Destination;
                            changeStream.Destination.AttachIncomingStream(changeStream.StreamManipulated);
                        }
                        (drawing_canvas as DrawingCanvas).HoveringOver = null;
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changeStream.StreamManipulated, changeStream.TheCanvasUsed, new Point()).Execute();
                    }

                    //we are doing redo
                    else
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.undoStack.AddFirst(changeStream);
                        }
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated, changeStream.TheCanvasUsed, new Point()).Execute();
                    }
                }
            }
            else if (desiredState is ProcessUnitUndo)
            {
                ProcessUnitUndo changePU = desiredState as ProcessUnitUndo;
                if (changePU.CommandIssed == CanvasCommands.AddToCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            //We need to save its current location so we can put it back when we need too but everything else can stay the same.
                            Point ipuLocation = changePU.IPUManipulated.MidPoint;
                            drawing_canvas.redoStack.AddFirst(new ProcessUnitUndo(changePU.CommandIssed, changePU.IPUManipulated, changePU.TheCanvasUsed, ipuLocation));
                        }

                        //since we just added the ProcessUnit to the drawing_drawing_canvas it cannot have anything attached so just delete it.
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, new Point()).Execute();
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.undoStack.AddFirst(changePU);
                        }

                        //this is where it gets tricky.  We removed a process unit and we might have removed streams that were attached to it so we gotta get everything back
                        //first we add the iprocessunit back to where it was.
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, changePU.Location).Execute();
                        while (changePU.ConnectedStream.Count != 0)
                        {
                            //need to push a command on the undo stack then call this undo function so it can deal with putting the stream back together.
                            drawing_canvas.redoStack.AddFirst(changePU.ConnectedStream.Pop());

                            //We are in redo, this is recursion
                            (new ChangeStateCommand(false, true) { Drawing_Canvas = drawing_canvas }).Execute();
                        }
                        //we are done putting back all the streams and the streams connected themselves back to the ProcessUnit so we are done.
                    }
                }
                else if (changePU.CommandIssed == CanvasCommands.MoveHead || changePU.CommandIssed == CanvasCommands.MoveTail)
                {
                    if (undo == true && recursion != true)
                    {
                        //we need to save the current state of the IProcessUnit and push it onto the redo stack.
                        Point ipuLocation = changePU.IPUManipulated.MidPoint;
                        drawing_canvas.redoStack.AddFirst(new ProcessUnitUndo(changePU.CommandIssed, changePU.IPUManipulated, changePU.TheCanvasUsed, ipuLocation));
                    }
                    else if (undo == false && recursion != true)
                    {
                        Point ipuLocation = changePU.IPUManipulated.MidPoint;
                        drawing_canvas.undoStack.AddFirst(new ProcessUnitUndo(changePU.CommandIssed, changePU.IPUManipulated, changePU.TheCanvasUsed, ipuLocation));
                    }

                    //just moving it streams will update themselves.
                    CommandFactory.CreateCommand(CanvasCommands.MoveHead, changePU.IPUManipulated, changePU.TheCanvasUsed, changePU.Location).Execute();
                }
                if (changePU.CommandIssed == CanvasCommands.RemoveFromCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            //we dont need to save the current state since we just gotta remove it from the drawing_drawing_canvas and since everything will be the same we can just use the undoStream.
                            drawing_canvas.redoStack.AddFirst(changePU);
                        }
                        //this is where it gets tricky.  We removed a process unit and we might have removed streams that were attached to it so we gotta get everything back
                        //first we add the iprocessunit back to where it was.

                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, changePU.IPUManipulated.MidPoint).Execute();

                        List<StreamUndo> streamUndos = new List<StreamUndo>();
                        while (changePU.ConnectedStream.Count != 0)
                        {
                            //need to push a command on the undo stack then call this undo function so it can deal with putting the stream back together.
                            StreamUndo streamUndo = changePU.ConnectedStream.Pop();
                            streamUndos.Add(streamUndo);
                            drawing_canvas.undoStack.AddFirst(streamUndo);
                            (new ChangeStateCommand(true, true) { Drawing_Canvas = drawing_canvas }).Execute();
                        }
                        foreach (StreamUndo su in streamUndos)
                        {
                            //this reverses the order but it is a arbitray ordering anyway
                            changePU.ConnectedStream.Push(su);
                        }
                        //we are done putting back all the streams and the streams connected themselves back to the ProcessUnit so we are done.
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.undoStack.AddFirst(changePU);
                        }
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, new Point()).Execute();
                    }
                }
            }
            else if (desiredState is StickyNoteUndo)
            {
                StickyNoteUndo changeNote = desiredState as StickyNoteUndo;
                if (changeNote.CommandIssed == CanvasCommands.AddToCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            //We need to save its current location so we can put it back when we need too but everything else can stay the same.
                            Point ipuLocation = new Point((double)(changeNote.SnManipulated as UIElement).GetValue(Canvas.LeftProperty), (double)(changeNote.SnManipulated as UIElement).GetValue(Canvas.TopProperty));
                            drawing_canvas.redoStack.AddFirst(new StickyNoteUndo(changeNote.CommandIssed, changeNote.SnManipulated, changeNote.TheCanvasUsed, ipuLocation));
                        }

                        //since we just added the ProcessUnit to the drawing_drawing_canvas it cannot have anything attached so just delete it.
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeNote.SnManipulated, changeNote.TheCanvasUsed, new Point()).Execute();
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.undoStack.AddFirst(changeNote);
                        }
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changeNote.SnManipulated, changeNote.TheCanvasUsed, changeNote.Location).Execute();
                    }
                }
                else if (changeNote.CommandIssed == CanvasCommands.MoveHead || changeNote.CommandIssed == CanvasCommands.MoveTail)
                {
                    if (undo == true && recursion != true)
                    {
                        //we need to save the current state of the IProcessUnit and push it onto the redo stack.
                        Point ipuLocation = new Point((double)(changeNote.SnManipulated as UIElement).GetValue(Canvas.LeftProperty), (double)(changeNote.SnManipulated as UIElement).GetValue(Canvas.TopProperty));
                        drawing_canvas.redoStack.AddFirst(new StickyNoteUndo(changeNote.CommandIssed, changeNote.SnManipulated, changeNote.TheCanvasUsed, ipuLocation));
                    }
                    else if (undo == false && recursion != true)
                    {
                        Point ipuLocation = new Point((double)(changeNote.SnManipulated as UIElement).GetValue(Canvas.LeftProperty), (double)(changeNote.SnManipulated as UIElement).GetValue(Canvas.TopProperty));
                        drawing_canvas.undoStack.AddFirst(new StickyNoteUndo(changeNote.CommandIssed, changeNote.SnManipulated, changeNote.TheCanvasUsed, ipuLocation));
                    }

                    //just moving it streams will update themselves.
                    CommandFactory.CreateCommand(CanvasCommands.MoveHead, changeNote.SnManipulated, changeNote.TheCanvasUsed, changeNote.Location, new Point(-3, -3)).Execute();
                }
                else if (changeNote.CommandIssed == CanvasCommands.RemoveFromCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            //we dont need to save the current state since we just gotta remove it from the drawing_drawing_canvas and since everything will be the same we can just use the undoStream.
                            drawing_canvas.redoStack.AddFirst(changeNote);
                        }
                        //this is where it gets tricky.  We removed a process unit and we might have removed streams that were attached to it so we gotta get everything back
                        //first we add the iprocessunit back to where it was.
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changeNote.SnManipulated, changeNote.TheCanvasUsed, changeNote.Location).Execute();
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            drawing_canvas.undoStack.AddFirst(changeNote);
                        }
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeNote.SnManipulated, changeNote.TheCanvasUsed, new Point()).Execute();
                    }
                }
            }
            drawing_canvas.SelectedElement = null;
            drawing_canvas.CurrentState = drawing_canvas.NullState;
            return true;
        }
    }
}