/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Windows.Input;
using ChemProV.PFD.StickyNote;
using ChemProV.UI.DrawingCanvas.Commands;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is used whenever we are resizing an object
    /// </summary>
    public class ResizeingState : IState
    {
        private DrawingCanvas canvas;

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="c">A reference to the drawing drawing_canvas</param>
        public ResizeingState(DrawingCanvas c)
        {
            canvas = c;
        }

        /// <summary>
        /// This fires whenever the mouse is moved and resizes the object based on where the mouse is.
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">used for the location of the mouse</param>
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.SelectedElement is StickyNote)
            {
                CommandFactory.CreateCommand(CanvasCommands.Resize, canvas.SelectedElement as StickyNote, canvas, e.GetPosition(canvas)).Execute();
            }
        }

        /// <summary>
        /// This changes the Cursor back to the default and changes the state to selectedState
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas.Cursor = Cursors.Arrow;
            canvas.CurrentState = canvas.SelectedState;
        }

        #region Unused Mouse Events

        /// <summary>
        /// Does Nothing
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Does Nothing
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Does Nothing
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Does Nothing
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Does Nothing
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Does Nothing
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        #endregion Unused Mouse Events

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
            MouseLeftButtonUp(sender, EventArgs.Empty as MouseButtonEventArgs);
        }
    }
}