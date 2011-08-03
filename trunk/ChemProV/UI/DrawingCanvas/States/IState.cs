/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows.Input;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the base class that all states inherite from.
    /// </summary>
    public interface IState
    {
        void MouseEnter(object sender, MouseEventArgs e);

        void MouseLeave(object sender, MouseEventArgs e);

        void MouseMove(object sender, MouseEventArgs e);

        void MouseLeftButtonDown(object sender, MouseButtonEventArgs e);

        void MouseLeftButtonUp(object sender, MouseButtonEventArgs e);

        void MouseRightButtonDown(object sender, MouseButtonEventArgs e);

        void MouseRightButtonUp(object sender, MouseButtonEventArgs e);

        void MouseWheel(object sender, MouseEventArgs e);

        void LostMouseCapture(object sender, MouseEventArgs e);
    }
}