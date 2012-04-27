/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

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

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will set the label on a labeled process unit
    /// </summary>
    public class SetProcessUnitLabel : IUndoRedoAction
    {
        private string m_label;
        
        private LabeledProcessUnit m_lpu;

        public SetProcessUnitLabel(LabeledProcessUnit lpu, string labelToSetOnExecution)
        {
            m_lpu = lpu;
            m_label = labelToSetOnExecution;
        }
        
        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Create the opposite action to set the label back to what it is right now
            IUndoRedoAction opposite = new SetProcessUnitLabel(m_lpu, m_lpu.ProcessUnitLabel);

            // Set the label
            m_lpu.ProcessUnitLabel = m_label;

            return opposite;
        }
    }
}
