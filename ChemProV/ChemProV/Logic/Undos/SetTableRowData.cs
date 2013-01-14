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
using ChemProV.Logic;

namespace ChemProV.Logic.Undos
{
    public class SetTableRowData : IUndoRedoAction
    {
        private int m_column;

        private object m_data;

        private IStreamDataRow m_row;

        public SetTableRowData(IStreamDataRow row, int columnIndex, object data)
        {
            m_row = row;
            m_column = columnIndex;
            m_data = data;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            SetTableRowData opposite = new SetTableRowData(m_row, m_column, m_row[m_column]);
            m_row[m_column] = m_data;
            return opposite;
        }
    }
}
