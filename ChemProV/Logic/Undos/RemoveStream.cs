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
    /// <summary>
    /// Represents an undo/redo action that will remove a stream from the workspace.
    /// </summary>
    public class RemoveStream : IUndoRedoAction
    {
        private AbstractStream m_streamToRemove = null;

        public RemoveStream(AbstractStream streamToRemove)
        {
            m_streamToRemove = streamToRemove;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            sender.RemoveStream(m_streamToRemove);
            return new AddToWorkspace(m_streamToRemove);
        }
    }
}
