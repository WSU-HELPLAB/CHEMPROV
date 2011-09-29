/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows.Controls;
using System.Windows.Input;
using ChemProV.PFD;
using ChemProV.UI.DrawingCanvas.Commands;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// These is used whenever there is a Selected Element on the drawing drawing_canvas
    /// </summary>
    public class SelectedState : IState
    {
        private DrawingCanvas canvas;
        private CommandFactory commandFactory;

        /// <summary>
        /// This is SelectedState Consstructer
        /// </summary>
        /// <param name="c">a reference to the DrawingCanvas</param>
        /// <param name="cf">Optional: reference to a CommandFactory or it just makes one</param>
        public SelectedState(DrawingCanvas c, CommandFactory cf = null)
        {
            canvas = c;
            if (cf == null)
            {
                cf = new CommandFactory();
            }
            commandFactory = cf;
        }

        #region IState Members

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseMove(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Try to move whatever we clicked on, if sender is not IPfdElement the cast will return null
        /// and so we go to null state.
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.SelectedElement = sender as IPfdElement;
            if (canvas.SelectedElement != null)
            {
                canvas.CurrentState = canvas.MovingState;
            }
            else
            {
                canvas.CurrentState = canvas.NullState;
            }
        }

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// This makes our right click menu with delete since something must be selected since we are
        /// in the selected state.
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!canvas.IsReadOnly)
            {
                CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, new ContextMenu(), canvas, e.GetPosition(canvas)).Execute();

                canvas.CurrentState = canvas.MenuState;

                e.Handled = true;
            }
        }

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// NOT USED
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        #endregion IState Members
    }
}