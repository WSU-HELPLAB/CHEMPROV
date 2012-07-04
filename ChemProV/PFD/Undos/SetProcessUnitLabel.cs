/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.Core;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will set the label on an AbstractProcessUnit
    /// </summary>
    public class SetProcessUnitLabel : IUndoRedoAction
    {
        private string m_label;
        
        private AbstractProcessUnit m_lpu;

        public SetProcessUnitLabel(AbstractProcessUnit unit, string labelToSetOnExecution)
        {
            m_lpu = unit;
            m_label = labelToSetOnExecution;
        }
        
        public IUndoRedoAction Execute(Workspace sender)
        {
            // Create the opposite action to set the label back to what it is right now
            IUndoRedoAction opposite = new SetProcessUnitLabel(m_lpu, m_lpu.Label);

            // Set the label
            m_lpu.Label = m_label;

            return opposite;
        }
    }
}
