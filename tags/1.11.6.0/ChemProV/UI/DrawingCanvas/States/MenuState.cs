/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemProV.PFD.StickyNote;
using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.UI.DrawingCanvas.Commands.DrawingCanvasCommands;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This state stops all normal operations and waits for a click on either drawing drawing_canvas or on the menu
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
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();
            if (canvas.SelectedElement != null)
                canvas.CurrentState = canvas.SelectedState;
            else
                canvas.CurrentState = canvas.NullState;
        }

        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();
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

        #endregion Unused Mouse Events

        /// <summary>
        /// This is used to changed color of the sticky notes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChangeColor(object sender, EventArgs e)
        {
            string header = ((sender as MenuItem).Header as string);
            StickyNoteColors color = StickyNote.StickyNoteColorsFromString(header);

            (canvas.SelectedElement as StickyNote).ColorChange(color);
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();
            canvas.SelectedElement = null;
            canvas.CurrentState = canvas.NullState;
        }

        public void HideStickyNotes(object sender, EventArgs e)
        {
            var stickyNotes = from c in canvas.Children
                              where c is StickyNote
                              select c as StickyNote;

            foreach (StickyNote s in stickyNotes)
            {
                s.Visibility = Visibility.Collapsed;
            }

            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();
            canvas.SelectedElement = null;
            canvas.CurrentState = canvas.NullState;
        }

        public void ShowStickyNotes(object sender, EventArgs e)
        {
            var stickyNotes = from c in canvas.Children
                              where c is StickyNote
                              select c as StickyNote;

            foreach (StickyNote s in stickyNotes)
            {
                s.Visibility = Visibility.Visible;
            }

            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();
            canvas.SelectedElement = null;
            canvas.CurrentState = canvas.NullState;
        }

        /// <summary>
        /// This is called when the user selects the Delete from the right click menu
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        public void Delete(object sender, EventArgs e)
        {
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();

            if (canvas.IsReadOnly)
            {
                MessageBox.Show("Sorry read-only mode you are not allowed to delete");
            }
            else
            {
                //need to clear redoStack because we might not already have and we are not undoing or redoing.
                canvas.redoStack.Clear();

                DeleteIPfdElementCommand command = DeleteIPfdElementCommand.GetInstance() as DeleteIPfdElementCommand;
                command.Drawing_Canvas = canvas;
                command.Execute();
                canvas.PFDModified();
            }
        }

        /// <summary>
        /// This function first saves the current state and pushes it onto our redo stack and then executes the undo command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        public void Undo(object sender, EventArgs e)
        {
            //first we remove the contextMenu if there is one
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();

            //call stateChanger to take care of everything, it is undo and is not recursion
            ChangeStateCommand c = ChangeStateCommand.GetInstance() as ChangeStateCommand;
            c.Undo = true;
            c.Drawing_Canvas = canvas;
            c.Execute();
        }

        public void Redo(object sender, EventArgs e)
        {
            //first we remove the contextMenu if there is one
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, canvas.NewContextMenu, canvas).Execute();

            //call stateChanger to take care of everything, it is not undo and is not recursion
            ChangeStateCommand c = ChangeStateCommand.GetInstance() as ChangeStateCommand;
            c.Undo = false;
            c.Drawing_Canvas = canvas;
            c.Execute();
        }

        #endregion IState Members

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }
    }
}