/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.Core;
using ChemProV.Logic;
using ChemProV.Logic.Equations;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will insert an equation into the equation collection in 
    /// a workspace.
    /// </summary>
    public class InsertEquation : IUndoRedoAction
    {
        private int m_index;
        
        private EquationModel m_item;

        private Workspace m_ws;

        public InsertEquation(Workspace workspace, EquationModel item, int index)
        {
            m_ws = workspace;
            m_item = item;
            m_index = index;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            m_ws.Equations.Insert(m_index, m_item);
            return new RemoveEquation(m_ws, m_index);
        }
    }
}
