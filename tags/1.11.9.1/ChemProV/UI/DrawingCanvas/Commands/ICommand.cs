/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.UI.DrawingCanvas.Commands
{
    /// <summary>
    /// This is what all commands inherriet from
    /// </summary>
    public interface ICommand
    {
        bool Execute();
    }
}