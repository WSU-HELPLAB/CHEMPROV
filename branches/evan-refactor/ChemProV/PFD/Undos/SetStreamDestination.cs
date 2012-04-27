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
    /// </summary>
    public class SetStreamDestination : IUndoRedoAction
    {
        private IProcessUnit m_pu;

        private IStream m_stream;

        public SetStreamDestination(IStream stream, IProcessUnit destination)
        {
            m_stream = stream;
            m_pu = destination;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // For the opposite action we have to set the destination back to what it
            // is at the current moment
            IUndoRedoAction opposite = new SetStreamDestination(m_stream, m_stream.Destination);

            // Set the destination
            m_stream.Destination = m_pu;

            return opposite;
        }
    }
}
