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

namespace ChemProV.PFD.Undos
{
    public class SetProcessSubgroup : IUndoRedoAction
    {
        private Color m_color;

        private IProcessUnit m_pu;

        public SetProcessSubgroup(IProcessUnit processUnit)
        {
            m_color = processUnit.Subgroup;
            m_pu = processUnit;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Use the current color for the opposite action
            IUndoRedoAction opposite = new SetProcessSubgroup(m_pu);

            // Restore the color
            m_pu.Subgroup = m_color;

            return opposite;
        }
    }
}
