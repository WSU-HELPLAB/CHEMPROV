/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using ChemProV.Core;

namespace ChemProV.Logic.Undos
{
    public class SetSubprocess : IUndoRedoAction
    {
        private string m_value;

        private AbstractProcessUnit m_pu;

        public SetSubprocess(AbstractProcessUnit processUnit)
        {
            m_value = processUnit.Subprocess;
            m_pu = processUnit;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Use the current color string for the opposite action
            IUndoRedoAction opposite = new SetSubprocess(m_pu);

            // Restore the color
            m_pu.Subprocess = m_value;

            return opposite;
        }
    }
}
