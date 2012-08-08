/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.Logic;

namespace ChemProV.Logic.Undos
{
    public class SetStickyNoteText : IUndoRedoAction
    {
        private StickyNote m_sn;

        private string m_text;

        public SetStickyNoteText(StickyNote stickyNote, string textToSetOnExecution)
        {
            m_sn = stickyNote;
            m_text = textToSetOnExecution;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Create the opposite action that will set the text back to what it is now
            IUndoRedoAction opposite = new SetStickyNoteText(m_sn, m_sn.Text);

            m_sn.Text = m_text;

            return opposite;
        }
    }
}
