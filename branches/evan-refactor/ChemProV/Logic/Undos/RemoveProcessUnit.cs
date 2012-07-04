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
    /// Represents an undo/redo action that will remove a process unit from the workspace.
    /// </summary>
    public class RemoveProcessUnit : IUndoRedoAction
    {
        private AbstractProcessUnit m_remove = null;

        public RemoveProcessUnit(AbstractProcessUnit unitToRemove)
        {
            m_remove = unitToRemove;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            sender.RemoveProcessUnit(m_remove);
            return new AddToWorkspace(m_remove);
        }
    }
}
