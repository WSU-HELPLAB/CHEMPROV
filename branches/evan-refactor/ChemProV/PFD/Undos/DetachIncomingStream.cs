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
using ChemProV.Core;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will detach an incoming stream from a process unit when 
    /// it is executed. Note that this is a very simple action with nothing more than a detachment 
    /// for the process unit. No properties of the stream are modified and no locations of anything 
    /// are modified.
    /// </summary>
    public class DetachIncomingStream : IUndoRedoAction
    {        
        private IProcessUnit m_pu;

        private IStream m_stream;

        public DetachIncomingStream(IProcessUnit processUnit, IStream incomingStream)
        {
            // A nice change for a future version would be to create a ReadOnlyStream 
            // class that wraps around a stream and prevents modifications. The 
            // incoming stream for this undo/redo could be of this type to further 
            // emphasize that only the process unit gets changed here.
            
            m_pu = processUnit;
            m_stream = incomingStream;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            m_pu.DettachIncomingStream(m_stream);

            return new AttachIncomingStream(m_pu, m_stream);
        }
    }
}
