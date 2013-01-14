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

namespace ChemProV.Logic.Undos
{
    public class ResizeStickyNote : IUndoRedoAction
    {
        private double m_height;
        
        private StickyNote m_note;

        private double m_width;

        public ResizeStickyNote(StickyNote stickyNote, double width, double height)
        {
            m_height = height;
            m_note = stickyNote;
            m_width = width;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            IUndoRedoAction opposite = new ResizeStickyNote(m_note, m_note.Width, m_note.Height);
            
            m_note.Width = m_width;
            m_note.Height = m_height;

            return opposite;
        }
    }
}
