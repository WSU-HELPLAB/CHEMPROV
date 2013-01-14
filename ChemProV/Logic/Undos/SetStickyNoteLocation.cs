namespace ChemProV.Logic.Undos
{
    public class SetStickyNoteLocation : IUndoRedoAction
    {
        private MathCore.Vector m_location;

        private StickyNote m_note;

        public SetStickyNoteLocation(StickyNote note)
            : this(note, new MathCore.Vector(note.LocationX, note.LocationY))
        {
        }
        
        public SetStickyNoteLocation(StickyNote note, MathCore.Vector location)
        {
            m_note = note;
            m_location = location;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            MathCore.Vector currentLocation = new MathCore.Vector(
                m_note.LocationX, m_note.LocationY);
            m_note.LocationX = m_location.X;
            m_note.LocationY = m_location.Y;
            return new SetStickyNoteLocation(m_note, currentLocation);
        }
    }
}
