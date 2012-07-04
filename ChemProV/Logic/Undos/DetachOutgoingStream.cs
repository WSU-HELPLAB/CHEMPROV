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
    /// Represents an undo/redo action that will detach an incoming stream from a process unit when 
    /// it is executed. Note that this is a very simple action with nothing more than a detachment 
    /// for the process unit. No properties of the stream are modified and no locations of anything 
    /// are modified.
    /// </summary>
    public class DetachOutgoingStream : IUndoRedoAction
    {
        private AbstractProcessUnit m_pu;

        private AbstractStream m_stream;

        public DetachOutgoingStream(AbstractProcessUnit processUnit, AbstractStream incomingStream)
        {
            m_pu = processUnit;
            m_stream = incomingStream;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            m_pu.DetachOutgoingStream(m_stream);

            return new AttachOutgoingStream(m_pu, m_stream);
        }
    }
}
