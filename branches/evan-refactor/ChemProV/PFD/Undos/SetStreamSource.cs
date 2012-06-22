/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System.Windows;
using ChemProV.Core;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will set the source of a stream to a process 
    /// unit. This action only affects the stream and the process unit is not modified 
    /// during execution.
    /// Since setting stream sources potentially changes the location of the draggable stream 
    /// endpoint, this undo takes care of positioning the endpoint as well.
    /// </summary>
    public class SetStreamSource : IUndoRedoAction
    {
        private Point m_location;

        private SetStreamSource m_opposite;
        
        private IProcessUnit m_pu;

        private AbstractStream m_stream;

        private SetStreamSource() { }

        public SetStreamSource(IStream stream, IProcessUnit sourceForThis, IProcessUnit sourceForOpposite,
            Point locationForDSE)
        {
            m_stream = stream as AbstractStream;
            m_pu = sourceForThis;
            m_location = locationForDSE;

            // Create the opposite action
            m_opposite = new SetStreamSource();
            m_opposite.m_location = locationForDSE;
            m_opposite.m_opposite = this;
            m_opposite.m_pu = sourceForOpposite;
            m_opposite.m_stream = m_stream;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Set the source
            m_stream.Source = m_pu;

            // If we're setting the source to null then we need to restore the DSE location
            if (null == m_pu)
            {
                // Set the icon location
                m_stream.SourceDragIcon.Location = m_location;
            }

            return m_opposite;
        }
    }
}
