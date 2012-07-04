using System;
using System.Collections.Generic;
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
using ChemProV.PFD.StickyNote;

namespace ChemProV.PFD.Undos
{
    public class InsertComment : IUndoRedoAction
    {
        private IList<StickyNote_UIIndependent> m_collection;

        private StickyNote_UIIndependent m_comment;

        private int m_index;

        public InsertComment(IList<StickyNote_UIIndependent> collection, StickyNote_UIIndependent comment, int insertionIndex)
        {
            m_collection = collection;
            m_comment = comment;
            m_index = insertionIndex;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            m_collection.Insert(m_index, m_comment);

            return new RemoveComment(m_collection, m_index);
        }
    }
}
