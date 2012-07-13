namespace ChemProV.Logic.Undos
{
    /// <summary>
    /// Represents an undo/redo action that will insert a row into a StreamPropertiesTable at 
    /// a specific index.
    /// </summary>
    public class InsertTableRow : IUndoRedoAction
    {
        private int m_index;

        private IStreamData m_row;

        private StreamPropertiesTable m_table;

        public InsertTableRow(int index, IStreamData row, StreamPropertiesTable table)
        {
            m_index = index;
            m_row = row;
            m_table = table;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Insert the row
            m_table.InsertRow(m_index, m_row);

            // Return an action that will remove it
            return new RemoveTableRow(m_index, m_table);
        }
    }
}
