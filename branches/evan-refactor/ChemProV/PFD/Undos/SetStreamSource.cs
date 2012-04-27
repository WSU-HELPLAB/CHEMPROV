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
    /// Represents an undo/redo action that will set the source of a stream to a process 
    /// unit. This action only affects the stream and the process unit is not modified 
    /// during execution.
    /// </summary>
    public class SetStreamSource : IUndoRedoAction
    {
        private IProcessUnit m_pu;

        private IStream m_stream;

        public SetStreamSource(IStream stream, IProcessUnit source)
        {
            m_stream = stream;
            m_pu = source;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // For the opposite action we have to set the source back to what it
            // is at the current moment
            IUndoRedoAction opposite = new SetStreamSource(m_stream, m_stream.Source);

            // Set the source
            m_stream.Source = m_pu;

            return opposite;
        }
    }
}
