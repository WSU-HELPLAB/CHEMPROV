/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using ChemProV.Core;

namespace ChemProV.Logic.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will attach an incoming stream to a process unit when 
    /// it is executed. Note that this is a very simple action with nothing more than a attachment 
    /// to the process unit. No properties of the stream are modified and no locations of anything 
    /// are modified.
    /// </summary>
    public class AttachOutgoingStream : IUndoRedoAction
    {
        private AbstractProcessUnit m_pu;

        private AbstractStream m_stream;

        public AttachOutgoingStream(AbstractProcessUnit processUnit, AbstractStream outgoingStream)
        {
            if (null == processUnit || null == outgoingStream)
            {
                throw new ArgumentNullException();
            }
            
            m_pu = processUnit;
            m_stream = outgoingStream;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // First allocate the undo/redo that does the opposite
            IUndoRedoAction opposite = new DetachOutgoingStream(m_pu, m_stream);

            // Do the simple attachment
            m_pu.AttachOutgoingStream(m_stream);

            return opposite;
        }
    }
}
