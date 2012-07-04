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
    /// Represents an undo/redo action that will set the destination of a stream to a process 
    /// unit. This action only affects the stream and the process unit is not modified 
    /// during execution.
    /// Since setting stream destinations potentially changes the location, this undo takes 
    /// care of restoring position as well.
    /// </summary>
    public class SetStreamDestination : ChemProV.Core.IUndoRedoAction
    {
        private MathCore.Vector m_location;

        private SetStreamDestination m_opposite;

        private AbstractProcessUnit m_pu;

        private AbstractStream m_stream;

        private SetStreamDestination() { }

        public SetStreamDestination(AbstractStream stream, AbstractProcessUnit destForThis,
            AbstractProcessUnit destForOpposite, MathCore.Vector location)
        {
            m_stream = stream as AbstractStream;
            m_pu = destForThis;
            m_location = location;

            // Create the opposite action
            m_opposite = new SetStreamDestination();
            m_opposite.m_location = location;
            m_opposite.m_opposite = this;
            m_opposite.m_pu = destForOpposite;
            m_opposite.m_stream = m_stream;
        }

        public ChemProV.Core.IUndoRedoAction Execute(ChemProV.Core.Workspace sender)
        {
            // Set the destination
            m_stream.Destination = m_pu;

            // If we're setting the destination to null then we need to restore the location
            if (null == m_pu)
            {
                // Set the location
                m_stream.DestinationLocation = m_location;
            }

            return m_opposite;
        }
    }
}
