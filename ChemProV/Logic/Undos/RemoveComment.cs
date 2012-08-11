/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Collections.Generic;

namespace ChemProV.Logic.Undos
{
    public class RemoveComment :IUndoRedoAction
    {
        private int m_index;
        
        private IList<StickyNote> m_owner;

        public RemoveComment(IList<StickyNote> owner, int index)
        {
            m_owner = owner;
            m_index = index;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            StickyNote current = m_owner[m_index];
            m_owner.RemoveAt(m_index);
            return new InsertComment(m_owner, current, m_index);
        }
    }
}
