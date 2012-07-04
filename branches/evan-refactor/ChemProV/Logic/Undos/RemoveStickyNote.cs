using ChemProV.Core;

namespace ChemProV.Logic.Undos
{
    public class RemoveStickyNote : IUndoRedoAction
    {
        private ChemProV.PFD.StickyNote.StickyNote_UIIndependent m_note;
        
        public RemoveStickyNote(ChemProV.PFD.StickyNote.StickyNote_UIIndependent note)
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
