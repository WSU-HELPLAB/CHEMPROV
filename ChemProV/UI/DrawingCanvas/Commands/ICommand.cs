/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
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