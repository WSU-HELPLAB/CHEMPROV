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
    public class SetStreamId : IUndoRedoAction
    {
        private int m_id;
        
        private AbstractStream m_stream;

        public SetStreamId(AbstractStream stream, int id)
        {
            m_stream = stream;
            m_id = id;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Create the opposite action to set the ID back to what it is now
            IUndoRedoAction opposite = new SetStreamId(m_stream, m_stream.Id);

            // Set the ID
            m_stream.Id = m_id;

            return opposite;
        }
    }
}
