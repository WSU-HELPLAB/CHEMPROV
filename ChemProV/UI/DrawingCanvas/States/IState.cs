/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows.Input;

namespace ChemProV.UI.DrawingCanvasStates
{
    /// <summary>
    /// This is the base class that all states inherit from.
    /// </summary>
    public interface IState
    {
        void MouseEnter(object sender, MouseEventArgs e);

        void MouseLeave(object sender, MouseEventArgs e);

        void MouseMove(object sender, MouseEventArgs e);

        void MouseLeftButtonDown(object sender, MouseButtonEventArgs e);

        void MouseLeftButtonUp(object sender, MouseButtonEventArgs e);

        void MouseWheel(object sender, MouseEventArgs e);

        void LostMouseCapture(object sender, MouseEventArgs e);

        /// <summary>
        /// This is invoked by the drawing canvas when this state is currently active but 
        /// is about to be deactivated for another state to come in (or perhaps no state). 
        /// This can be used for things like hiding a popup menu that was added to the 
        /// canvas.
        /// While states generally have permission to set a new drawing canvas state from 
        /// their mouse processing functions, the must not set the CurrentState property of 
        /// the drawing canvas from this function.
        /// </summary>
        void StateEnding();
    }
}