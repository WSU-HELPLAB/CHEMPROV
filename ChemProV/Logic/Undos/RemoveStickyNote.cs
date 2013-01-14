using ChemProV.Core;

namespace ChemProV.Logic.Undos
{
    public class RemoveStickyNote : IUndoRedoAction
    {
        private ChemProV.Logic.StickyNote m_note;
        
        public RemoveStickyNote(ChemProV.Logic.StickyNote note)
        {
            m_note = note;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            sender.StickyNotes.Remove(m_note);
            return new AddToWorkspace(m_note);
        }
    }
}
