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
    /// <summary>
    /// Represents an undo/redo action that will add a stream, process unit, or sticky 
    /// note to the workspace.
    /// </summary>
    public class AddToWorkspace : IUndoRedoAction
    {
        private object m_toAdd;
        
        public AddToWorkspace(object objToAdd)
        {
            if (!(objToAdd is AbstractProcessUnit) && !(objToAdd is AbstractStream) &&
                !(objToAdd is StickyNote))
            {
                throw new InvalidOperationException(
                    "Object for an AddToWorkspace undo/redo action must be a process unit, " +
                    "stream, or sticky note. Object was of type: " + objToAdd.GetType().ToString());
            }

            m_toAdd = objToAdd;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            if (m_toAdd is AbstractProcessUnit)
            {
                sender.AddProcessUnit(m_toAdd as AbstractProcessUnit);
                return new RemoveProcessUnit(m_toAdd as AbstractProcessUnit);
            }
            else if (m_toAdd is AbstractStream)
            {
                sender.AddStream(m_toAdd as AbstractStream);
                return new RemoveStream(m_toAdd as AbstractStream);
            }
            else
            {
                sender.StickyNotes.Add(m_toAdd as ChemProV.Logic.StickyNote);
                return new RemoveStickyNote(m_toAdd as ChemProV.Logic.StickyNote);
            }
        }
    }
}
