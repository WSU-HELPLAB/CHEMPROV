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
    public class InsertBasicComment : IUndoRedoAction
    {
        private BasicComment m_comment;
        
        private IList<BasicComment> m_comments;

        private int m_index;

        public InsertBasicComment(BasicComment comment, IList<BasicComment> collection, int index)
        {
            m_comment = comment;
            m_comments = collection;
            m_index = index;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            m_comments.Insert(m_index, m_comment);
            return new RemoveBasicComment(m_comments, m_index);
        }
    }
}
