/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;

namespace ChemProV.Logic.Undos
{
    public class RemoveBasicComment : IUndoRedoAction
    {
        private IList<BasicComment> m_comments;

        private int m_index;

        public RemoveBasicComment(IList<BasicComment> comments, int index)
        {
            m_comments = comments;
            m_index = index;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Get the comment that we're about to remove
            BasicComment bc = m_comments[m_index];

            // Remove it
            m_comments.RemoveAt(m_index);

            // Return an action that will insert it back into the collection
            return new InsertBasicComment(bc, m_comments, m_index);
        }
    }
}
