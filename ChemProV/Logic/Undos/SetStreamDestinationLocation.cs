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
    public class SetStreamDestinationLocation : ChemProV.Core.IUndoRedoAction
    {
        private MathCore.Vector m_location;

        private SetStreamDestinationLocation m_opposite;

        private AbstractStream m_stream;

        private SetStreamDestinationLocation() { }

        public SetStreamDestinationLocation(AbstractStream stream, MathCore.Vector location)
        {
            m_stream = stream as AbstractStream;
            m_location = location;

            // Create the opposite action to set it back to where it is now
            m_opposite = new SetStreamDestinationLocation();
            m_opposite.m_location = stream.DestinationLocation;
            m_opposite.m_opposite = this;
            m_opposite.m_stream = m_stream;
        }

        public ChemProV.Core.IUndoRedoAction Execute(ChemProV.Core.Workspace sender)
        {
            m_stream.DestinationLocation = m_location;
            return m_opposite;
        }
    }
}
