/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.Logic.Equations;

namespace ChemProV.Logic.Undos
{
    public class SetEquationText : IUndoRedoAction
    {
        private EquationModel m_equation;

        private string m_text;

        public SetEquationText(EquationModel model, string textToSetOnExecution)
        {
            m_equation = model;
            m_text = textToSetOnExecution;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Create the opposite action that will set the text back to what it is now
            IUndoRedoAction opposite = new SetEquationText(m_equation, m_equation.Equation);

            m_equation.Equation = m_text;

            return opposite;
        }
    }
}
