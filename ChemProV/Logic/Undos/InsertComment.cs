using System.Collections.Generic;

namespace ChemProV.Logic.Undos
{
    public class InsertComment : IUndoRedoAction
    {
        private IList<StickyNote> m_collection;

        private StickyNote m_comment;

        private int m_index;

        public InsertComment(IList<StickyNote> collection, StickyNote comment, int insertionIndex)
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
