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
using ChemProV.PFD.Streams.PropertiesTable;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This state stops all normal operations and waits for a click on either drawing canvas or on the menu
    /// What the menu options call are at the bottem of this class
    /// </summary>
    public class MenuState : IState
    {

        private DrawingCanvas canvas;

        public MenuState(DrawingCanvas c)
        {
            canvas = c;
        }

        #region IState Members

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.Children.Remove(canvas.rightClickMenu);
            if (canvas.SelectedElement != null)
                canvas.CurrentState = canvas.SelectedState;
            else
                canvas.CurrentState = canvas.NullState;
        }

        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.Children.Remove(canvas.rightClickMenu);
            if (canvas.SelectedElement != null)
                canvas.CurrentState = canvas.SelectedState;
            else
                canvas.CurrentState = canvas.NullState;
        }

        #region Unused Mouse Events

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }



        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
        }
        #endregion

        /// <summary>
        /// This is called when the user selects the Delete from the right click menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Delete(object sender, EventArgs e)
        {
            canvas.Children.Remove(canvas.rightClickMenu);

            //need to clear redoStack because we might not already have and we are not undoing or redoing.
            canvas.redoStack.Clear();
            if (canvas.SelectedElement is StreamEnd)
            {
                Point previousLocation = new Point((double)((canvas.SelectedElement as StreamEnd).Stream as UIElement).GetValue(Canvas.LeftProperty), (double)((canvas.SelectedElement as StreamEnd).Stream as UIElement).GetValue(Canvas.TopProperty));
                canvas.saveState(CanvasCommands.RemoveFromCanvas, (canvas.SelectedElement as StreamEnd).Stream, canvas, previousLocation);
            }
            else if (canvas.SelectedElement is IPropertiesTable)
            {
                canvas.SelectedElement = (canvas.SelectedElement as IPropertiesTable).ParentStream;
                Point previousLocation = new Point((double)(canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                canvas.saveState(CanvasCommands.RemoveFromCanvas, canvas.SelectedElement, canvas, previousLocation);
            }
            else
            {
                Point previousLocation = new Point((double)(canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                canvas.saveState(CanvasCommands.RemoveFromCanvas, canvas.SelectedElement, canvas, previousLocation);
            }
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.SelectedElement, canvas, new Point()).Execute();
            canvas.ChildrenModified();
        }


        /// <summary>
        /// This function is called from undo or redo 
        /// </summary>
        /// <param name="undo">If we want to undo this is true but if we want to redo this is false</param>
        /// <param name="recursion">if statechanger is calling itself set to true else false, this is so we dont save intermedier steps</param>
        public void stateChanger(bool undo, bool recursion)
        {

            /*
             * The logic of this funcition
             * first find if we are dealing with a stream or a processUnit.
             * Then figure out what command we are doing.  This effects the command we do and how we save the state
             * Then figure out if we are doing undo or redo. If undo and command is delete then do the opposite which is add but if redo and command is delete then do delete
             * Then figure out if we are doing recursion, if not doing recursion save state if we are doing recurions save state.
             */
            SavedStateObject desiredState;
            if (undo == true)
            {
                if (canvas.undoStack.Count > 0)
                {
                    desiredState = canvas.undoStack.First.Value;
                    canvas.undoStack.RemoveFirst();
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (canvas.redoStack.Count > 0)
                {
                desiredState = canvas.redoStack.First.Value;
                canvas.redoStack.RemoveFirst();
                }
                else
                {
                    return;
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
                            canvas.redoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.Source as IProcessUnit, changeStream.Destination as IProcessUnit, new Point()));
                        }
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated, changeStream.TheCanvasUsed, new Point()).Execute();
                    }
                    else
                    {
                        if(recursion != true)
                        {
                            canvas.undoStack.AddFirst(changeStream);
                        }
                        //We need to add back the stream and if it had temporary processUnit we need to add those back too
                        if ((changeStream.Source is TemporaryProcessUnit))
                        {
                            //Need to get back the source tempProcessUnit 
                            canvas.redoStack.AddFirst(changeStream.SourceUndo);

                            //We are redoing so undo is false, and we are doing recursion so that is true
                            stateChanger(false, true);
                        }
                        else
                        {
                            changeStream.StreamManipulated.Source = changeStream.Source;
                            changeStream.Source.AttachOutgoingStream(changeStream.StreamManipulated);
                        }

                        if ((changeStream.Destination is TemporaryProcessUnit))
                        {
                            canvas.redoStack.AddFirst(changeStream.DestinationUndo);
                            stateChanger(false, true);
                        }
                        else
                        {
                            changeStream.StreamManipulated.Destination = changeStream.Destination;
                            changeStream.Destination.AttachIncomingStream(changeStream.StreamManipulated);
                        }
                        (canvas as DrawingCanvas).HoveringOver = null;
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

                                canvas.redoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, destinationLocation));
                         }
                        else if(undo == false && recursion != true)
                        {
                                //we need to save the current state and push it onto the redo stack
                                Point destinationLocation = new Point((double)(changeStream.StreamManipulated.Destination as UIElement).GetValue(Canvas.LeftProperty), (double)(changeStream.StreamManipulated.Destination as UIElement).GetValue(Canvas.TopProperty));

                                //NOTE changeStream.Source is what the source of the changeStream used to be before we it changed.
                                //Whereas changeStream.StreamManipualted gets us a reference to the stream as is, so if we add .Source onto the end of it we get the current soource

                                canvas.undoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, destinationLocation));
                        }
                            //now we need to issue the undo command
                            if (changeStream.Destination is TemporaryProcessUnit)
                            {
                                CommandFactory.CreateCommand(CanvasCommands.MoveHead, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.Location).Execute();
                            }
                            else if (changeStream.Destination is IProcessUnit)
                            {
                                CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated.Destination, changeStream.TheCanvasUsed, new Point()).Execute();
                                changeStream.StreamManipulated.Destination = changeStream.Destination;
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

                        canvas.redoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, sourceLocation));
                    }
                    else if (undo == false && recursion != true)
                    {
                        //we need to save the current state and push it onto the redo stack
                        Point sourceLocation = new Point((double)(changeStream.StreamManipulated.Source as UIElement).GetValue(Canvas.LeftProperty), (double)(changeStream.StreamManipulated.Source as UIElement).GetValue(Canvas.TopProperty));

                        //NOTE changeStream.Source is what the source of the changeStream used to be before we it changed.
                        //Whereas changeStream.StreamManipualted gets us a reference to the stream as is, so if we add .Source onto the end of it we get the current soource

                        canvas.undoStack.AddFirst(new StreamUndo(changeStream.CommandIssed, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.StreamManipulated.Source as IProcessUnit, changeStream.StreamManipulated.Destination as IProcessUnit, sourceLocation));
                    }
                    if (changeStream.Source is TemporaryProcessUnit)
                    {
                        CommandFactory.CreateCommand(CanvasCommands.MoveTail, changeStream.StreamManipulated, changeStream.TheCanvasUsed, changeStream.Location).Execute();
                    }
                    else if (changeStream.Source is IProcessUnit)
                    {
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changeStream.StreamManipulated.Source, changeStream.TheCanvasUsed, new Point()).Execute();
                        changeStream.StreamManipulated.Source = changeStream.Source;
                        changeStream.Source.AttachOutgoingStream(changeStream.StreamManipulated);
                    }
                }
                else if (changeStream.CommandIssed == CanvasCommands.RemoveFromCanvas)
                {
                    if (undo == true)
                    {
                        if (recursion != true)
                        {
                            //we dont need to save the current state since we just gotta remove it from the canvas and since everything will be the same we can just use the changeStream.
                            canvas.redoStack.AddFirst(changeStream);
                        }
                        //We need to add back the stream and if it had temporary processUnit we need to add those back too
                        if (changeStream.Source is TemporaryProcessUnit)
                        {
                            //Need to get back the source tempProcessUnit 
                            canvas.undoStack.AddFirst(changeStream.SourceUndo);

                            //we are doing undo and this is recursion
                            stateChanger(true, true);
                        }
                        else if (changeStream.Source == null)
                        {
                            IStream streamToChange = changeStream.StreamManipulated;
                            IProcessUnit source = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Source);
                            CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, source, canvas, changeStream.Location).Execute();
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
                            canvas.undoStack.AddFirst(changeStream.DestinationUndo);

                            //we are doing undo and this is recursion
                            stateChanger(true, true);
                        }
                        else if (changeStream.Destination == null)
                        {
                            IStream streamToChange = changeStream.StreamManipulated;
                            IProcessUnit sink = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Sink);
                            CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, sink, canvas, changeStream.Location).Execute();
                            streamToChange.Destination = sink;
                            sink.AttachIncomingStream(streamToChange);
                        }
                        else
                        {
                            changeStream.StreamManipulated.Destination = changeStream.Destination;
                            changeStream.Destination.AttachIncomingStream(changeStream.StreamManipulated);
                        }
                        (canvas as DrawingCanvas).HoveringOver = null;
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changeStream.StreamManipulated, changeStream.TheCanvasUsed, new Point()).Execute();
                    }

                    //we are doing redo
                    else
                    {
                        if (recursion != true)
                        {
                            canvas.undoStack.AddFirst(changeStream);
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
                            Point ipuLocation = new Point((double)(changePU.IPUManipulated as UIElement).GetValue(Canvas.LeftProperty), (double)(changePU.IPUManipulated as UIElement).GetValue(Canvas.TopProperty));
                            canvas.redoStack.AddFirst(new ProcessUnitUndo(changePU.CommandIssed, changePU.IPUManipulated, changePU.TheCanvasUsed, ipuLocation));
                        }

                        //since we just added the ProcessUnit to the canvas it cannot have anything attached so just delete it.
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, new Point()).Execute();
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            canvas.undoStack.AddFirst(changePU);
                        }

                        //this is where it gets tricky.  We removed a process unit and we might have removed streams that were attached to it so we gotta get everything back
                        //first we add the iprocessunit back to where it was.
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, changePU.Location).Execute();
                        while (changePU.ConnectedStream.Count != 0)
                        {
                            //need to push a command on the undo stack then call this undo function so it can deal with putting the stream back together.
                            canvas.redoStack.AddFirst(changePU.ConnectedStream.Pop());
                            
                            //We are in redo, this is recursion
                            stateChanger(false, true);
                        }
                        //we are done putting back all the streams and the streams connected themselves back to the ProcessUnit so we are done.
                    }
                }
                else if (changePU.CommandIssed == CanvasCommands.MoveHead || changePU.CommandIssed == CanvasCommands.MoveTail)
                {
                    if (undo == true && recursion != true)
                    {
                        //we need to save the current state of the IProcessUnit and push it onto the redo stack.
                        Point ipuLocation = new Point((double)(changePU.IPUManipulated as UIElement).GetValue(Canvas.LeftProperty), (double)(changePU.IPUManipulated as UIElement).GetValue(Canvas.TopProperty));
                        canvas.redoStack.AddFirst(new ProcessUnitUndo(changePU.CommandIssed, changePU.IPUManipulated, changePU.TheCanvasUsed, ipuLocation));
                    }
                    else if (undo == false && recursion != true)
                    {
                        Point ipuLocation = new Point((double)(changePU.IPUManipulated as UIElement).GetValue(Canvas.LeftProperty), (double)(changePU.IPUManipulated as UIElement).GetValue(Canvas.TopProperty));
                        canvas.undoStack.AddFirst(new ProcessUnitUndo(changePU.CommandIssed, changePU.IPUManipulated, changePU.TheCanvasUsed, ipuLocation));
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

                            //we dont need to save the current state since we just gotta remove it from the canvas and since everything will be the same we can just use the undoStream.
                            canvas.redoStack.AddFirst(changePU);
                        }
                        //this is where it gets tricky.  We removed a process unit and we might have removed streams that were attached to it so we gotta get everything back
                        //first we add the iprocessunit back to where it was.
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, changePU.Location).Execute();
                        while (changePU.ConnectedStream.Count != 0)
                        {
                            //need to push a command on the undo stack then call this undo function so it can deal with putting the stream back together.
                            canvas.undoStack.AddFirst(changePU.ConnectedStream.Pop());
                            stateChanger(true, true);
                        }
                        //we are done putting back all the streams and the streams connected themselves back to the ProcessUnit so we are done.
                    }
                    else
                    {
                        if (recursion != true)
                        {
                            canvas.undoStack.AddFirst(changePU);
                        }
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, changePU.IPUManipulated, changePU.TheCanvasUsed, new Point()).Execute();
                    }
                }
            }

            canvas.ChildrenModified();
            canvas.SelectedElement = null;
            canvas.CurrentState = canvas.NullState;

        }
        /// <summary>
        /// This function first saves the current state and pushes it onto our redo stack and then executes the undo command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        public void Undo(object sender, EventArgs e)
        {
            canvas.Children.Remove(canvas.rightClickMenu);
             //call stateChanger to take care of everything, it is undo and is not recursion
            stateChanger(true, false);
        }

        public void Redo(object sender, EventArgs e)
        {
            canvas.Children.Remove(canvas.rightClickMenu);
            //call stateChanger to take care of everything, it is not undo and is not recursion
            stateChanger(false, false);
        }

        #endregion
    }
}
