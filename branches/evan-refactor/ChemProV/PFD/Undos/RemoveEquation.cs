/*
Copyright 2012 HELP Lab @ Washington State University

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
using ChemProV.Core;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will remove an equation from the equation collection in 
    /// a workspace.
    /// </summary>
    public class RemoveEquation : IUndoRedoAction
    {
        private int m_index;

        private Workspace m_ws;

        public RemoveEquation(Workspace workspace, int index)
        {
            m_ws = workspace;
            m_index = index;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            EquationModel item = m_ws.Equations[m_index];
            m_ws.Equations.Remove(item);
            return new InsertEquation(m_ws, item, m_index);
        }
    }
}
