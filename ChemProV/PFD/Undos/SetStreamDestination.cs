/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Undos;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will set the destination of a stream to a process 
    /// unit. This action only affects the stream and the process unit is not modified 
    /// during execution.
    /// Since setting stream destinations potentially changes the location of the draggable stream 
    /// endpoint, this undo takes care of positioning the endpoint as well.
    /// </summary>
    public class SetStreamDestination : IUndoRedoAction
    {
        private Point m_location;

        private IUndoRedoAction m_opposite = null;
        
        private IProcessUnit m_pu;

        private AbstractStream m_stream;

        public SetStreamDestination(IStream stream, IProcessUnit destinationForThis, IProcessUnit destinationForOpposite)
        {
            m_stream = (AbstractStream)stream;
            m_pu = destinationForThis;
            m_location = m_stream.DestinationDragIcon.Location;
            
            // Create opposite - it will have to set the destination back to what it 
            // is now
            m_opposite = new SetStreamDestination(stream as AbstractStream, destinationForOpposite, this);
        }

        /// <summary>
        /// Constructor for an undo/redo that sets the stream destination to null and restores 
        /// the location of the destination drag icon. The opposite action (redo for this undo or 
        /// undo for this redo) is also initialized to connect back to the destination specified 
        /// in the third parameter.
        /// </summary>
        public SetStreamDestination(IStream stream, Point endpointLocationToRestore,
            IProcessUnit destinationForOppositeAction)
        {
            m_stream = stream as AbstractStream;
            m_pu = null;
            m_location = endpointLocationToRestore;
            m_opposite = new SetStreamDestination(m_stream, destinationForOppositeAction, this);
        }

        private SetStreamDestination(AbstractStream stream, IProcessUnit destination, SetStreamDestination opposite)
        {
            m_stream = stream;
            m_pu = destination;
            m_location = opposite.m_location;
            m_opposite = opposite;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {            
            // Set the destination
            m_stream.Destination = m_pu;

            if (null == m_pu)
            {
                m_stream.DestinationDragIcon.Location = m_location;
            }

            return m_opposite;
        }
    }
}
