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
    public class SetStreamSourceLocation : IUndoRedoAction
    {
        private MathCore.Vector m_location;

        private SetStreamSourceLocation m_opposite;

        private AbstractStream m_stream;

        private SetStreamSourceLocation() { }

        public SetStreamSourceLocation(AbstractStream stream, MathCore.Vector location)
        {
            m_stream = stream as AbstractStream;
            m_location = location;

            // Create the opposite action to set it back to where it is now
            m_opposite = new SetStreamSourceLocation();
            m_opposite.m_location = stream.SourceLocation;
            m_opposite.m_opposite = this;
            m_opposite.m_stream = m_stream;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            m_stream.SourceLocation = m_location;
            return m_opposite;
        }
    }
}
