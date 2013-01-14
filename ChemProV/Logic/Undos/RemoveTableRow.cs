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

namespace ChemProV.Logic.Undos
{
    public class RemoveTableRow : IUndoRedoAction
    {
        private int m_index;

        private StreamPropertiesTable m_table;

        public RemoveTableRow(int index, StreamPropertiesTable table)
        {
            m_index = index;
            m_table = table;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Get the row we're about to remove
            IStreamDataRow row = m_table.Rows[m_index];

            // Remove it
            m_table.RemoveRowAt(m_index);

            // Return an action that will insert it back where it was
            return new InsertTableRow(m_index, row, m_table);
        }
    }
}
