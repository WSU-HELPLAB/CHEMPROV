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

namespace ChemProV.PFD.Undos
{
    /// <summary>
    /// Undo/redo action that will set the Annotation property of an EquationModel object
    /// </summary>
    public class SetAnnotation : IUndoRedoAction
    {
        private EquationEditor.Models.EquationModel m_model;

        private string m_val;

        public SetAnnotation(EquationEditor.Models.EquationModel model, string value)
        {
            m_model = model;
            m_val = value;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            string current = m_model.Annotation;
            m_model.Annotation = m_val;
            return new SetAnnotation(m_model, current);
        }
    }
}
