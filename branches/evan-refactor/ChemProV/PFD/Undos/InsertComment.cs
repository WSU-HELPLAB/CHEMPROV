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
    public class InsertComment : IUndoRedoAction
    {
        private Core.ICommentCollection m_cc;

        private Core.IComment m_comment;

        private int m_index;

        public InsertComment(Core.ICommentCollection collection, Core.IComment comment, int insertionIndex)
        {
            m_cc = collection;
            m_comment = comment;
            m_index = insertionIndex;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            m_cc.InsertCommentAt(m_comment, m_index);

            return new RemoveComment(m_cc, m_index);
        }
    }
}
