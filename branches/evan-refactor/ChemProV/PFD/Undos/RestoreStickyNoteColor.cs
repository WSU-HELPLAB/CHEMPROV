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
using ChemProV.PFD.Undos;

namespace ChemProV.PFD.Undos
{
    public class RestoreStickyNoteColor : IUndoRedoAction
    {
        /// <summary>
        /// The original designer of the sticky notes (which was not me) used an enumerated 
        /// type to refer to colors. I can understand this to an extent, since it's really 
        /// a color scheme that sticky notes use and not just a single defining color. I 
        /// still think that a bit of refactoring around this wouldn't hurt, but for now 
        /// I'm dealing with it the way it is.
        /// This value stores the sticky note color that will be restored on execution.
        /// </summary>
        private StickyNote.StickyNoteColors m_color;

        private ChemProV.PFD.StickyNote.StickyNote m_note;

        public RestoreStickyNoteColor(ChemProV.PFD.StickyNote.StickyNote stickyNote)
        {
            m_color = stickyNote.ColorScheme;
            m_note = stickyNote;
        }
        
        #region IUndoRedoAction Members

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            // Create the opposite item that will set the color back to what it is now
            IUndoRedoAction opposite = new RestoreStickyNoteColor(m_note);

            // Set the color scheme
            m_note.ColorChange(m_color);

            return opposite;
        }

        #endregion
    }
}
